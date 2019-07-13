using System;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests.Robustness
{
    [TestFixture]
    public sealed class MemoryLeaks
    {
        public interface IContract
        {
            int Value { get; set; }

            event EventHandler ValueChanged;
            
            IContract Child { get; set; }
        }
        
        private sealed class HostedObject : IContract
        {
            #region Implementation of IContract

            public int Value { get; set; }
            public event EventHandler ValueChanged;
            
            public IContract Child { get; set; }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallReleaseIfNotUsed(string serverAddress, string clientAddress)
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
                    remoteObject.Value = 5;
                    Assert.AreEqual(5, remoteObject.Value);

                    var weakRef = new WeakReference(remoteObject);
                    
                    Assert.IsTrue(weakRef.IsAlive);
                    
                    //Act
                    remoteObject = null;
                    GC.Collect();
                    
                    //Assert
                    Assert.IsNull(remoteObject);
                    Assert.IsFalse(weakRef.IsAlive);
                }
            }
        }
                
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallReleaseParentIfNotUsed(string serverAddress, string clientAddress)
        {
            //Arrange
            var parentObjectId = "1";
            var childObjectId = "2";

            var parent = new HostedObject();
            parent.Child = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            {
                server.Add<IContract>(parentObjectId, parent);
                server.Add(childObjectId, parent.Child);

                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var parentRemoteObject = client.Connect<IContract>(parentObjectId);

                    var childRemoteObject = parentRemoteObject.Child;
                    childRemoteObject.Value = 5;
                    Assert.AreEqual(5, childRemoteObject.Value);

                    var parentWeakRef = new WeakReference(parentRemoteObject);
                    var childWeakRef = new WeakReference(childRemoteObject);
                    
                    Assert.IsTrue(parentWeakRef.IsAlive);
                    Assert.IsTrue(childWeakRef.IsAlive);
                    
                    //Act
                    parentRemoteObject = null;
                    GC.Collect();
                    
                    //Assert
                    Assert.AreEqual(5, childRemoteObject.Value);
                    Assert.IsNull(parentRemoteObject);
                    Assert.IsFalse(parentWeakRef.IsAlive);
                    Assert.IsTrue(childWeakRef.IsAlive);
                    
                    //Act
                    childRemoteObject = null;
                    GC.Collect();
                    
                    //Assert
                    Assert.IsNull(childRemoteObject);
                    Assert.IsFalse(parentWeakRef.IsAlive);
                    Assert.IsFalse(childWeakRef.IsAlive);
                }
            }
        }
                
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallKeepReferenceIfSubscribed(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                WeakReference weakRef;
                
                using (var client = new EloquentClient(serverAddress, clientAddress))
                {
                    var remoteObject = client.Connect<IContract>(objectId);
                    remoteObject.ValueChanged += RemoteObjectOnValueChanged;

                    weakRef = new WeakReference(remoteObject);
                    
                    Assert.IsTrue(weakRef.IsAlive);
                    
                    //Act
                    remoteObject = null;
                    GC.Collect();
                    
                    Assert.IsNull(remoteObject);
                    Assert.IsTrue(weakRef.IsAlive);
                }

                GC.Collect();
                    
                Assert.IsFalse(weakRef.IsAlive);
            }
        }

                
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallReleaseIfUnsubscribed(string serverAddress, string clientAddress)
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
                    remoteObject.ValueChanged += RemoteObjectOnValueChanged;

                    remoteObject.ValueChanged -= RemoteObjectOnValueChanged;
                    
                    var weakRef = new WeakReference(remoteObject);
                    
                    Assert.IsTrue(weakRef.IsAlive);
                    
                    //Act
                    remoteObject = null;
                    GC.Collect();
                    
                    Assert.IsNull(remoteObject);
                    Assert.IsFalse(weakRef.IsAlive);
                }
            }
        }

        private void RemoteObjectOnValueChanged(object sender, EventArgs e)
        {
        }
    }
}