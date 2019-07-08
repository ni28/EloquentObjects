using System;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class ChildEloquentObjectsTests
    {
        [EloquentContract]
        public interface IContract
        {
            [EloquentMethod]
            IEloquent<IChildContract> GetChild();
        }

        [EloquentContract]
        public interface IChildContract
        {
            [EloquentProperty]
            string Name { get; }
        }
        
        private sealed class ParentObject : IContract, IDisposable
        {
            private readonly IObjectHost<IChildContract> _child;
            public IObjectHost<IContract> Eloquent { get; }

            public ParentObject(IEloquentServer server, string objectId, IObjectHost<IChildContract> child)
            {
                _child = child;
                Eloquent = server.Add<IContract>(objectId, this);
            }
            
            #region Implementation of IContract

            public IEloquent<IChildContract> GetChild()
            {
                return _child;
            }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                Eloquent?.Dispose();
            }

            #endregion
        }

        private sealed class ChildObject : IChildContract, IDisposable
        {
            public IObjectHost<IChildContract> Eloquent { get; }

            public ChildObject(IEloquentServer server, string objectId, string name)
            {
                Eloquent = server.Add<IChildContract>(objectId, this);
                Name = name;
            }
            
            #region Implementation of IChildContract

            public string Name { get; }

            #endregion

            #region IDisposable

            public void Dispose()
            {
                Eloquent?.Dispose();
            }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallReturnConnectableEloquentObject(string serverAddress, string clientAddress)
        {
            //Arrange
            using (var server = new EloquentServer(serverAddress))
            using (var child = new ChildObject(server, "child", "qwerty"))
            using (new ParentObject(server, "parent", child.Eloquent))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                using (var connection = client.Connect<IContract>("parent"))
                {
                    //Arrange
                    var remoteParent = connection.Object;
                    
                    //Act
                    using (var remoteChildConnection = remoteParent.GetChild().Connect())
                    {
                        var remoteChild = remoteChildConnection.Object;
                        
                        //Assert
                        Assert.AreEqual("qwerty", remoteChild.Name);
                    }

                }
            }
        }
        
    }
}