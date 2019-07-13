using System;
using System.Runtime.Serialization;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class TwoWayMethodTests
    {
        public interface IContract
        {
            void CallTwoWay();

            void CallTwoWayWithParameters(int a, bool b, string s, double d, ComplexParameter p, int[] iArr,
                bool[] bArr, string[] sArr, double[] dArr, ComplexParameter[] pArr);

            void CallTwoWayWithException(string message);

            ComplexParameter CallTwoWayWithReturnValue(int i, bool b, string s, double d);

            void CallTwoWayWithDefaultParameter(int i = 5);
        }

        [DataContract]
        public sealed class ComplexParameter
        {
            [DataMember] public int A { get; set; }

            [DataMember] public bool B { get; set; }

            [DataMember] public string S { get; set; }

            [DataMember] public double D { get; set; }
        }

        private sealed class HostedObject : IContract
        {
            public bool TwoWayCalled { get; private set; }

            public object[] Parameters { get; set; }

            public bool TwoWayWithParameterCalled { get; set; }

            public bool TwoWayWithExceptionCalled { get; private set; }
            public bool TwoWayWithReturnValueCalled { get; private set; }

            #region Implementation of IContract

            public void CallTwoWay()
            {
                TwoWayCalled = true;
            }

            public void CallTwoWayWithParameters(int a, bool b, string s, double d, ComplexParameter p, int[] iArr,
                bool[] bArr, string[] sArr, double[] dArr, ComplexParameter[] pArr)
            {
                TwoWayWithParameterCalled = true;
                Parameters = new object[] {a, b, s, d, p, iArr, bArr, sArr, dArr, pArr};
            }

            public void CallTwoWayWithException(string message)
            {
                TwoWayWithExceptionCalled = true;
                throw new InvalidOperationException(message);
            }

            public ComplexParameter CallTwoWayWithReturnValue(int i, bool b, string s, double d)
            {
                TwoWayWithReturnValueCalled = true;
                return new ComplexParameter
                {
                    A = i,
                    B = b,
                    S = s,
                    D = d
                };
            }

            public void CallTwoWayWithDefaultParameter(int i = 5)
            {
                Parameters = new object[] {i};
            }

            #endregion
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallCallRemoteMethodsTwoWay(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject = client.Connect<IContract>(objectId);

                Assert.IsFalse(hostedObject.TwoWayCalled);

                //Act
                remoteObject.CallTwoWay();

                //Assert
                Assert.IsTrue(hostedObject.TwoWayCalled);
            }
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallCallReturnValue(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject = client.Connect<IContract>(objectId);

                Assert.IsFalse(hostedObject.TwoWayCalled);

                //Act
                var result = remoteObject.CallTwoWayWithReturnValue(1, true, "qwerty", 123.123);

                //Assert
                Assert.IsTrue(hostedObject.TwoWayWithReturnValueCalled);
                AssertComplexParameter(new ComplexParameter
                {
                    A = 1,
                    B = true,
                    S = "qwerty",
                    D = 123.123
                }, result);

            }
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallRethrowExceptionWhenCalledRemoteMethodsTwoWay(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject = client.Connect<IContract>(objectId);

                Assert.IsFalse(hostedObject.TwoWayWithExceptionCalled);

                //Act
                var e = Assert.Catch<FaultException>(() => { remoteObject.CallTwoWayWithException("qwerty"); });

                //Assert
                Assert.IsTrue(hostedObject.TwoWayWithExceptionCalled);
                Assert.IsInstanceOf<FaultException>(e);
                Assert.AreEqual("qwerty", e.Message);
                Assert.AreEqual(typeof(InvalidOperationException).FullName, e.ExceptionType);
            }
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallCallRemoteMethodTwoWayWithParameters(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject = client.Connect<IContract>(objectId);

                Assert.IsFalse(hostedObject.TwoWayWithParameterCalled);

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

                remoteObject.CallTwoWayWithParameters(
                    1,
                    false,
                    "qwerty",
                    123.123,
                    complexParameter1,
                    new[] {1, 2, 3},
                    new[] {false, true, false},
                    new[] {"123", "qwe", "zxc"},
                    new[] {1.123, 2.234, 3.345},
                    new[]
                    {
                        complexParameter2,
                        complexParameter3,
                    });

                //Assert
                Assert.IsTrue(hostedObject.TwoWayWithParameterCalled);

                Assert.AreEqual(1, hostedObject.Parameters[0]);
                Assert.AreEqual(false, hostedObject.Parameters[1]);
                Assert.AreEqual("qwerty", hostedObject.Parameters[2]);
                Assert.AreEqual(123.123, hostedObject.Parameters[3]);
                AssertComplexParameter(complexParameter1, (ComplexParameter) hostedObject.Parameters[4]);
                Assert.AreEqual(new[] {1, 2, 3}, hostedObject.Parameters[5]);
                Assert.AreEqual(new[] {false, true, false}, hostedObject.Parameters[6]);
                Assert.AreEqual(new[] {"123", "qwe", "zxc"}, hostedObject.Parameters[7]);
                Assert.AreEqual(new[] {1.123, 2.234, 3.345}, hostedObject.Parameters[8]);
                Assert.AreEqual(2, ((ComplexParameter[]) hostedObject.Parameters[9]).Length);
                AssertComplexParameter(complexParameter2, ((ComplexParameter[]) hostedObject.Parameters[9])[0]);
                AssertComplexParameter(complexParameter3, ((ComplexParameter[]) hostedObject.Parameters[9])[1]);

                //Act
                hostedObject.TwoWayWithParameterCalled = false;
                hostedObject.Parameters = null;

                Assert.IsFalse(hostedObject.TwoWayWithParameterCalled);

                remoteObject.CallTwoWayWithParameters(
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

                //Assert
                Assert.IsTrue(hostedObject.TwoWayWithParameterCalled);

                Assert.IsNotNull(hostedObject.Parameters);
                Assert.AreEqual(2, hostedObject.Parameters[0]);
                Assert.AreEqual(true, hostedObject.Parameters[1]);
                Assert.AreEqual(null, hostedObject.Parameters[2]);
                Assert.AreEqual(234.234, hostedObject.Parameters[3]);
                Assert.AreEqual(null, hostedObject.Parameters[4]);
                Assert.AreEqual(null, hostedObject.Parameters[5]);
                Assert.AreEqual(null, hostedObject.Parameters[6]);
                Assert.AreEqual(null, hostedObject.Parameters[7]);
            }
        }



        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallUseDefaultParametersIfAny(string serverAddress, string clientAddress)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client = new EloquentClient(serverAddress, clientAddress))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject = client.Connect<IContract>(objectId);

                Assert.IsFalse(hostedObject.TwoWayCalled);

                //Act
                remoteObject.CallTwoWayWithDefaultParameter();

                //Assert
                Assert.AreEqual(new[] {5}, hostedObject.Parameters);

                //Act
                remoteObject.CallTwoWayWithDefaultParameter(6);

                //Assert
                Assert.AreEqual(new[] {6}, hostedObject.Parameters);
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