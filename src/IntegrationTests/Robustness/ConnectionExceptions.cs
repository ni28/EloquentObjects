using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests.Robustness
{
    [TestFixture]
    public sealed class ConnectionExceptions
    {
        [EloquentContract]
        public interface IContract
        {
            [EloquentProperty]
            int Value { get; set; }
        }

        internal sealed class HostedObject : IContract
        {
            #region Implementation of IContract

            public int Value { get; set; }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallThrowWhenServerIsMissing(string serverAddress, string client1Address)
        {
            //Act
            var exception = Assert.Catch<IOException>(() =>
            {
                using (new EloquentClient(serverAddress, client1Address))
                {
                
                }
            });

            //Assert
            Assert.AreEqual("Connection failed. Server not found.", exception.Message);
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallThrowWhenConnectingToStoppedServer(string serverAddress, string client1Address)
        {
            //Arrange
            var server = new EloquentServer(serverAddress);
            using (var client = new EloquentClient(serverAddress, client1Address))
            {
                server.Dispose();
            
                //Act
                var exception = Assert.Catch<Exception>(() => { client.Connect<IContract>("objectId"); });

                //Assert
                if (exception is FaultException)
                {
                    Console.WriteLine(exception);
                    Assert.IsInstanceOf<IOException>(exception);
                }
                Assert.AreEqual("Connection failed. Check that server is still alive", exception.Message);
            }
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallThrowWhenConnectingToMissingObject(string serverAddress, string client1Address)
        {
            //Arrange
            using (new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, client1Address))
            {
                //Act
                var exception = Assert.Catch<KeyNotFoundException>(() => { client.Connect<IContract>("objectId"); });

                //Assert
                Assert.AreEqual("No objects with ID objectId are hosted on server", exception.Message);
            }
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallThrowWhenCalledRemovedObject(string serverAddress, string client1Address)
        {
            //Arrange
            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, client1Address))
            {
                var objectHost = server.Add<IContract>("objectId", new HostedObject());

                using (var connection = client.Connect<IContract>("objectId"))
                {
                    objectHost.Dispose();
                    
                    //Act
                    var exception = Assert.Catch<KeyNotFoundException>(() => { client.Connect<IContract>("objectId"); });

                    //Assert
                    Assert.AreEqual("No objects with ID objectId are hosted on server", exception.Message);
                }
                
            }
        }
    }
}