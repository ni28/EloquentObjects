using System;
using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class RemoteObjectParameterTests
    {
        private sealed class OneWayAttribute : Attribute
        {
        }

        public interface IParent
        {
            void SetChild1(IChild child);
            void SetChild2(int a, IChild child);
            void SetChild3(int a, IChild child, string b);
            [OneWay]
            void SetChild4(IChild child);
            [OneWay]
            void SetChild5(int a, IChild child);
            [OneWay]
            void SetChild6(int a, IChild child, string b);
        }

        public interface IChild
        {
            
        }
  
        private sealed class Parent : IParent
        {
            public IChild Value { get; set; }
            public int A { get; set; }
            public string B { get; set; }

            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            public void WaitNotificationReceived()
            {
                _autoResetEvent.WaitOne(3000);
            }
            
            #region Implementation of IParent

            public void SetChild1(IChild child)
            {
                Value = child;
            }

            public void SetChild2(int a, IChild child)
            {
                Value = child;
                A = a;
            }

            public void SetChild3(int a, IChild child, string b)
            {
                Value = child;
                A = a;
                B = b;
            }

            public void SetChild4(IChild child)
            {
                Value = child;
                _autoResetEvent.Set();
            }

            public void SetChild5(int a, IChild child)
            {
                Value = child;
                A = a;
                _autoResetEvent.Set();
            }

            public void SetChild6(int a, IChild child, string b)
            {
                Value = child;
                A = a;
                B = b;
                _autoResetEvent.Set();
            }

            #endregion
        }
        
        private sealed class Child : IChild
        {
            
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallPassRemoteObjectAsParameter(string serverAddress, string clientAddress)
        {
            //Arrange
            var parent = new Parent();
            var child = new Child();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IParent>("parent", parent);
                server.Add<IChild>("child", child);

                var remoteParent = client.Connect<IParent>("parent");
                var remoteChild = client.Connect<IChild>("child");

                parent.Value = null;
                
                //Act
                remoteParent.SetChild1(remoteChild);
                //Assert
                Assert.AreEqual(0, parent.A);
                Assert.AreEqual(null, parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

                //Act
                remoteParent.SetChild2(5, remoteChild);
                //Assert
                Assert.AreEqual(5, parent.A);
                Assert.AreEqual(null, parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

                //Act
                remoteParent.SetChild3(7, remoteChild, "qwerty");
                //Assert
                Assert.AreEqual(7, parent.A);
                Assert.AreEqual("qwerty", parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

                //Act
                remoteParent.SetChild4(remoteChild);
                parent.WaitNotificationReceived();
                //Assert
                Assert.AreEqual(7, parent.A);
                Assert.AreEqual("qwerty", parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

                //Act
                remoteParent.SetChild5(5, remoteChild);
                parent.WaitNotificationReceived();
                //Assert
                Assert.AreEqual(5, parent.A);
                Assert.AreEqual("qwerty", parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

                //Act
                remoteParent.SetChild6(0, remoteChild, null);
                parent.WaitNotificationReceived();
                //Assert
                Assert.AreEqual(0, parent.A);
                Assert.AreEqual(null, parent.B);
                Assert.AreSame(child, parent.Value);

                parent.Value = null;

            }
        }

    }

}