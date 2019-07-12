using System;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests.Robustness
{
    [TestFixture]
    public sealed class MemoryLeaks
    {
        [EloquentContract]
        public interface IContract
        {
            [EloquentProperty]
            int Value { get; set; }

            [EloquentEvent]
            event EventHandler ValueChanged;
        }
        
        private sealed class HostedObject : IContract
        {
            #region Implementation of IContract

            public int Value { get; set; }
            public event EventHandler ValueChanged;

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
                    var remoteObject = client.Get<IContract>(objectId);
                    remoteObject.Value = 5;
                    Assert.AreEqual(5, remoteObject.Value);

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
                    var remoteObject = client.Get<IContract>(objectId);
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
                    var remoteObject = client.Get<IContract>(objectId);
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