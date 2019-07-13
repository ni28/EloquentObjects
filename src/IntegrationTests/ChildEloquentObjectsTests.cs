using System;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class ChildEloquentObjectsTests
    {
        public interface IContract
        {
            IChildContract GetChild();
        }

        public interface IChildContract
        {
            string Name { get; }
        }
        
        private sealed class ParentObject : IContract, IDisposable
        {
            private readonly IChildContract _child;
            private readonly IDisposable _host;

            public ParentObject(IEloquentServer server, string objectId, IChildContract child)
            {
                _child = child;
                _host = server.Add<IContract>(objectId, this);
            }
            
            #region Implementation of IContract

            public IChildContract GetChild()
            {
                return _child;
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                _host?.Dispose();
            }

            #endregion
        }

        private sealed class ChildObject : IChildContract, IDisposable
        {
            private IObjectHost<IChildContract> _host;
            public ChildObject(IEloquentServer server, string objectId, string name)
            {
                _host = server.Add<IChildContract>(objectId, this);
                Name = name;
            }
            
            #region Implementation of IChildContract

            public string Name { get; }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                _host?.Dispose();
            }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallReturnConnectableEloquentObject(string serverAddress, string clientAddress)
        {
            //Arrange
            using (var server = new EloquentServer(serverAddress))
            using (var child = new ChildObject(server, "child", "qwerty"))
            using (new ParentObject(server, "parent", child))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                //Arrange
                var remoteParent = client.Connect<IContract>("parent");

                //Act
                var remoteChild = remoteParent.GetChild();

                //Assert
                Assert.AreEqual("qwerty", remoteChild.Name);
            }
        }
        
    }
}