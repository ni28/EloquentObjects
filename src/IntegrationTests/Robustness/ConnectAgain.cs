using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests.Robustness
{
    [TestFixture]
    public sealed class ConnectAgain
    {
        public interface IContract
        {
            int Value { get; set; }
        }
        
        private sealed class HostedObject : IContract
        {
            #region Implementation of IContract

            public int Value { get; set; }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallConnectAgain(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var remoteObject = client.Connect<IContract>(objectId);

                    //Act
                    remoteObject.Value = 5;

                    //Assert
                    Assert.AreEqual(5, remoteObject.Value);
                }

                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var remoteObject = client.Connect<IContract>(objectId);

                    //Assert
                    Assert.AreEqual(5, remoteObject.Value);

                    //Act
                    remoteObject.Value = 6;

                    //Assert
                    Assert.AreEqual(6, remoteObject.Value);
                }
            }
        }

        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallHostAgain(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var remoteObject = client.Connect<IContract>(objectId);

                    //Act
                    remoteObject.Value = 5;

                    //Assert
                    Assert.AreEqual(5, remoteObject.Value);
                }
            }
            
            using (var server = new EloquentServer(serverAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var remoteObject = client.Connect<IContract>(objectId);

                    //Assert
                    Assert.AreEqual(5, remoteObject.Value);

                    //Act
                    remoteObject.Value = 6;

                    //Assert
                    Assert.AreEqual(6, remoteObject.Value);
                }
            }
        }
    }
}