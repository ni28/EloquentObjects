using System;
using System.Runtime.Serialization;
using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class OneWayMethodTests
    {
        [EloquentInterface]
        public interface IContract
        {
            [EloquentMethod(IsOneWay = true)]
            void CallOneWay();

            [EloquentMethod(IsOneWay = true)]
            void CallOneWayWithParameters(int a, bool b, string s, double d, ComplexParameter p, int[] iArr, bool[] bArr, string[] sArr, double[] dArr, ComplexParameter[] pArr );

            [EloquentMethod(IsOneWay = true)]
            void CallOneWayWithException();
        }

        [DataContract]
        public sealed class ComplexParameter
        {
            [DataMember]
            public int A { get; set; }
            
            [DataMember]
            public bool B { get; set; }

            [DataMember]
            public string S { get; set; }

            [DataMember]
            public double D { get; set; }
        }
        
        private sealed class HostedObject : IContract
        {
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
            public bool OneWayCalled { get; private set; }

            public object[] Parameters { get; set; }

            public bool OneWayWithParameterCalled { get; set; }

            public bool OneWayWithExceptionCalled { get; private set; }
            
            public void WaitCallCompleted()
            {
                _autoResetEvent.WaitOne(5000);
            }
            
            #region Implementation of IContract

            public void CallOneWay()
            {
                OneWayCalled = true;
                _autoResetEvent.Set();
            }

            public void CallOneWayWithParameters(int a, bool b, string s, double d, ComplexParameter p, int[] iArr, bool[] bArr, string[] sArr, double[] dArr, ComplexParameter[] pArr )
            {
                OneWayWithParameterCalled = true;
                Parameters = new object[] {a, b, s, d, p, iArr, bArr, sArr, dArr, pArr};
                _autoResetEvent.Set();
            }

            public void CallOneWayWithException()
            {
                OneWayWithExceptionCalled = true;
                throw new InvalidOperationException();
            }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        public void ShallCallRemoteMethodsOneWay(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();
            
            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var connection = client.Connect<IContract>(objectId))
                {
                    var remoteObject = connection.Object;

                    Assert.IsFalse(hostedObject.OneWayCalled);
                    
                    //Act
                    remoteObject.CallOneWay();
                    hostedObject.WaitCallCompleted();

                    //Assert
                    Assert.IsTrue(hostedObject.OneWayCalled);
                }
            }
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        public void ShallHideExceptionWhenCalledRemoteMethodsOneWay(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();
            
            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var connection = client.Connect<IContract>(objectId))
                {
                    var remoteObject = connection.Object;

                    Assert.IsFalse(hostedObject.OneWayWithExceptionCalled);
                    
                    //Act
                    remoteObject.CallOneWayWithException();
                    hostedObject.WaitCallCompleted();
                    
                    //Assert
                    Assert.IsTrue(hostedObject.OneWayWithExceptionCalled);
                }
            }
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        public void ShallCallRemoteMethodsOneWayWithParameters(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();
            
            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                using (var connection = client.Connect<IContract>(objectId))
                {
                    var remoteObject = connection.Object;

                    Assert.IsFalse(hostedObject.OneWayWithParameterCalled);
                    
                    //Act
                    var complexParameter1 = new ComplexParameter
                    {
                        A = 5,
                        B = true,
                        D = 543.543,
                        S = "asd"
                    };
                    var complexParameter2 = new ComplexParameter
                    {
                        A = 6,
                        B = false,
                        D = 345.345,
                        S = "zxc"
                    };
                    var complexParameter3 = new ComplexParameter
                    {
                        A = 5,
                        B = true,
                        D = 543.543,
                        S = "123"
                    };
                    
                    remoteObject.CallOneWayWithParameters(
                        1,
                        false, 
                        "qwerty", 
                        123.123, 
                        complexParameter1,
                        new[] { 1, 2, 3 }, 
                        new[] { false, true, false },
                        new[] { "123", "qwe", "zxc" },
                        new[] { 1.123, 2.234, 3.345 },
                        new[]
                        {
                            complexParameter2,
                            complexParameter3,
                        });
                    hostedObject.WaitCallCompleted();
                    
                    //Assert
                    Assert.IsTrue(hostedObject.OneWayWithParameterCalled);
                    
                    Assert.AreEqual(1, hostedObject.Parameters[0]);
                    Assert.AreEqual(false, hostedObject.Parameters[1]);
                    Assert.AreEqual("qwerty", hostedObject.Parameters[2]);
                    Assert.AreEqual(123.123, hostedObject.Parameters[3]);
                    AssertComplexParameter(complexParameter1, (ComplexParameter)hostedObject.Parameters[4]);
                    Assert.AreEqual(new[] {1, 2, 3}, hostedObject.Parameters[5]);
                    Assert.AreEqual(new[] { false, true, false } , hostedObject.Parameters[6]);
                    Assert.AreEqual(new[] { "123", "qwe", "zxc" } , hostedObject.Parameters[7]);
                    Assert.AreEqual(new[] { 1.123, 2.234, 3.345 } , hostedObject.Parameters[8]);
                    Assert.AreEqual(2, ((ComplexParameter[])hostedObject.Parameters[9]).Length);
                    AssertComplexParameter(complexParameter2, ((ComplexParameter[])hostedObject.Parameters[9])[0]);
                    AssertComplexParameter(complexParameter3, ((ComplexParameter[])hostedObject.Parameters[9])[1]);
                    
                    //Act
                    hostedObject.OneWayWithParameterCalled = false;
                    hostedObject.Parameters = null;

                    Assert.IsFalse(hostedObject.OneWayWithParameterCalled);
                    
                    remoteObject.CallOneWayWithParameters(
                        2,
                        true, 
                        null, 
                        234.234, 
                        null,
                        null, 
                        null,
                        null,
                        null,
                        null);
                    hostedObject.WaitCallCompleted();
                    
                    //Assert
                    Assert.IsTrue(hostedObject.OneWayWithParameterCalled);
                    
                    Assert.AreEqual(2, hostedObject.Parameters[0]);
                    Assert.AreEqual(true, hostedObject.Parameters[1]);
                    Assert.AreEqual(null, hostedObject.Parameters[2]);
                    Assert.AreEqual(234.234, hostedObject.Parameters[3]);
                    Assert.AreEqual(null, hostedObject.Parameters[4]);
                    Assert.AreEqual(null , hostedObject.Parameters[5]);
                    Assert.AreEqual(null , hostedObject.Parameters[6]);
                    Assert.AreEqual(null , hostedObject.Parameters[7]);
                }
            }
        }

        private void AssertComplexParameter(ComplexParameter expected, ComplexParameter actual)
        {
            Assert.AreEqual(expected.A, actual.A);
            Assert.AreEqual(expected.B, actual.B);
            Assert.AreEqual(expected.D, actual.D);
            Assert.AreEqual(expected.S, actual.S);
        }
    }
}