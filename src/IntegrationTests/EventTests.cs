using System;
using System.Runtime.Serialization;
using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class EventTests
    {
        [EloquentContract]
        public interface IContract
        {
            [EloquentEvent]
            event EventHandler<ComplexParameter> RegularEvent;

            [EloquentEvent]
            event Action NoParameterEvent;

            [EloquentEvent]
            event Action<int, bool, string, double, ComplexParameter, int[], bool[], string[], double[], ComplexParameter[]> EventWithParameters;
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
            public void SendRegularEvent(ComplexParameter complexParameter)
            {
                RegularEvent?.Invoke(this, complexParameter);
            }
            
            public void SendNoParametersEvent()
            {
                NoParameterEvent?.Invoke();
            }
            
            public void SendEventWithParameters(int a, bool b, string s, double d, ComplexParameter p, int[] iArr, bool[] bArr, string[] sArr, double[] dArr, ComplexParameter[] pArr)
            {
                EventWithParameters?.Invoke(a, b, s, d, p, iArr, bArr, sArr, dArr, pArr);
            }
            
            #region Implementation of IContract

            public event EventHandler<ComplexParameter> RegularEvent;
            public event Action NoParameterEvent;
            
            public event Action<int, bool, string, double, ComplexParameter, int[], bool[], string[], double[], ComplexParameter[]> EventWithParameters;

            #endregion
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001", "tcp://127.0.0.1:50002")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001", "pipe://127.0.0.1:50002")]
        public void ShallRaiseRegularEventWithClientAsSender(string serverAddress, string client1Address,
            string client2Address)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client1 = new EloquentClient(serverAddress, client1Address))
            using (var client2 = new EloquentClient(serverAddress, client2Address))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject1 = client1.Get<IContract>(objectId);
                var remoteObject2 = client2.Get<IContract>(objectId);
                var remoteObject3 = client2.Get<IContract>(objectId);
                Assert.AreNotSame(remoteObject1, remoteObject2);
                Assert.AreNotSame(remoteObject2, remoteObject3);

                object sender1 = null;
                object sender2 = null;
                object sender3 = null;
                object sender4 = null;
                ComplexParameter args1 = null;
                ComplexParameter args2 = null;
                ComplexParameter args3 = null;
                ComplexParameter args4 = null;

                var autoResetEvent1 = new AutoResetEvent(false);
                var autoResetEvent2 = new AutoResetEvent(false);
                var autoResetEvent3 = new AutoResetEvent(false);
                var autoResetEvent4 = new AutoResetEvent(false);

                remoteObject1.RegularEvent += (s, args) =>
                {
                    sender1 = s;
                    args1 = args;
                    autoResetEvent1.Set();
                };
                remoteObject2.RegularEvent += (s, args) =>
                {
                    sender2 = s;
                    args2 = args;
                    autoResetEvent2.Set();
                };
                remoteObject3.RegularEvent += (s, args) =>
                {
                    sender3 = s;
                    args3 = args;
                    autoResetEvent3.Set();
                };
                remoteObject3.RegularEvent += (s, args) =>
                {
                    sender4 = s;
                    args4 = args;
                    autoResetEvent4.Set();
                };
                
                var complexParameter1 = new ComplexParameter
                {
                    A = 5,
                    B = true,
                    D = 543.543,
                    S = "asd"
                };

                Assert.IsNull(sender1);
                Assert.IsNull(sender2);
                Assert.IsNull(sender3);
                Assert.IsNull(sender4);
                Assert.IsNull(args1);
                Assert.IsNull(args2);
                Assert.IsNull(args3);
                Assert.IsNull(args4);

                //Act
                hostedObject.SendRegularEvent(complexParameter1);

                autoResetEvent1.WaitOne(2000);
                autoResetEvent2.WaitOne(2000);
                autoResetEvent3.WaitOne(2000);
                autoResetEvent4.WaitOne(2000);

                //Assert
                Assert.AreSame(remoteObject1, sender1);
                Assert.AreSame(remoteObject2, sender2);
                Assert.AreSame(remoteObject3, sender3);
                Assert.AreSame(remoteObject3, sender4);
                AssertComplexParameter(complexParameter1, args1);
                AssertComplexParameter(complexParameter1, args2);
                AssertComplexParameter(complexParameter1, args3);
                AssertComplexParameter(complexParameter1, args4);
            }
        }

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001", "tcp://127.0.0.1:50002")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001", "pipe://127.0.0.1:50002")]
        public void ShallRaiseEventWithoutParameters(string serverAddress, string client1Address, string client2Address)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();
            
            using (var server = new EloquentServer(serverAddress))
            using (var client1 = new EloquentClient(serverAddress, client1Address))
            using (var client2 = new EloquentClient(serverAddress, client2Address))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject1 = client1.Get<IContract>(objectId);
                var remoteObject2 = client2.Get<IContract>(objectId);

                var noParameterEvent1Called = false;
                var noParameterEvent2Called = false;

                var autoResetEvent1 = new AutoResetEvent(false);
                var autoResetEvent2 = new AutoResetEvent(false);

                remoteObject1.NoParameterEvent += () =>
                {
                    noParameterEvent1Called = true;
                    autoResetEvent1.Set();
                };
                remoteObject2.NoParameterEvent += () =>
                {
                    noParameterEvent2Called = true;
                    autoResetEvent2.Set();
                };

                //Act
                Assert.IsFalse(noParameterEvent1Called);
                Assert.IsFalse(noParameterEvent2Called);
                hostedObject.SendNoParametersEvent();

                autoResetEvent1.WaitOne(2000);
                autoResetEvent2.WaitOne(2000);

                //Assert
                Assert.IsTrue(noParameterEvent1Called);
                Assert.IsTrue(noParameterEvent2Called);
            }
        }


        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001", "tcp://127.0.0.1:50002")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001", "pipe://127.0.0.1:50002")]
        public void ShallRaiseEventWithParameters(string serverAddress, string client1Address, string client2Address)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client1 = new EloquentClient(serverAddress, client1Address))
            using (var client2 = new EloquentClient(serverAddress, client2Address))
            {
                server.Add<IContract>(objectId, hostedObject);
                
                var remoteObject1 = client1.Get<IContract>(objectId);
                var remoteObject2 = client2.Get<IContract>(objectId);

                object[] parameters1 = null;
                object[] parameters2 = null;

                var autoResetEvent1 = new AutoResetEvent(false);
                var autoResetEvent2 = new AutoResetEvent(false);

                remoteObject1.EventWithParameters += (i, b, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                {
                    parameters1 = new object[]
                    {
                        i, b, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10
                    };

                    autoResetEvent1.Set();
                };

                remoteObject2.EventWithParameters += (i, b, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                {
                    parameters2 = new object[]
                    {
                        i, b, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10
                    };

                    autoResetEvent2.Set();
                };

                //Act
                Assert.IsNull(parameters1);
                Assert.IsNull(parameters2);

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

                hostedObject.SendEventWithParameters(
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

                autoResetEvent1.WaitOne(2000);
                autoResetEvent2.WaitOne(2000);

                Assert.NotNull(parameters1);
                Assert.NotNull(parameters2);

                Assert.AreEqual(1, parameters1[0]);
                Assert.AreEqual(false, parameters1[1]);
                Assert.AreEqual("qwerty", parameters1[2]);
                Assert.AreEqual(123.123, parameters1[3]);
                AssertComplexParameter(complexParameter1, (ComplexParameter) parameters1[4]);
                Assert.AreEqual(new[] {1, 2, 3}, parameters1[5]);
                Assert.AreEqual(new[] {false, true, false}, parameters1[6]);
                Assert.AreEqual(new[] {"123", "qwe", "zxc"}, parameters1[7]);
                Assert.AreEqual(new[] {1.123, 2.234, 3.345}, parameters1[8]);
                Assert.AreEqual(2, ((ComplexParameter[]) parameters1[9]).Length);
                AssertComplexParameter(complexParameter2, ((ComplexParameter[]) parameters1[9])[0]);
                AssertComplexParameter(complexParameter3, ((ComplexParameter[]) parameters1[9])[1]);


                Assert.AreEqual(1, parameters2[0]);
                Assert.AreEqual(false, parameters2[1]);
                Assert.AreEqual("qwerty", parameters2[2]);
                Assert.AreEqual(123.123, parameters2[3]);
                AssertComplexParameter(complexParameter1, (ComplexParameter) parameters2[4]);
                Assert.AreEqual(new[] {1, 2, 3}, parameters2[5]);
                Assert.AreEqual(new[] {false, true, false}, parameters2[6]);
                Assert.AreEqual(new[] {"123", "qwe", "zxc"}, parameters2[7]);
                Assert.AreEqual(new[] {1.123, 2.234, 3.345}, parameters2[8]);
                Assert.AreEqual(2, ((ComplexParameter[]) parameters2[9]).Length);
                AssertComplexParameter(complexParameter2, ((ComplexParameter[]) parameters2[9])[0]);
                AssertComplexParameter(complexParameter3, ((ComplexParameter[]) parameters2[9])[1]);

            }
        }

        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001", "tcp://127.0.0.1:50002")]
        //[TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001", "pipe://127.0.0.1:50002")]
        public void ShallUnsubscribe(string serverAddress, string client1Address,
            string client2Address)
        {
            //Arrange
            var objectId = "obj";

            var hostedObject = new HostedObject();

            using (var server = new EloquentServer(serverAddress))
            using (var client1 = new EloquentClient(serverAddress, client1Address))
            using (var client2 = new EloquentClient(serverAddress, client2Address))
            {
                server.Add<IContract>(objectId, hostedObject);

                var remoteObject1 = client1.Get<IContract>(objectId);
                var remoteObject2 = client1.Get<IContract>(objectId);
                var remoteObject3 = client2.Get<IContract>(objectId);
                Assert.AreNotSame(remoteObject1, remoteObject2);
                Assert.AreNotSame(remoteObject1, remoteObject3);
                Assert.AreNotSame(remoteObject2, remoteObject3);

                object sender1 = null;
                object sender2 = null;
                object sender3 = null;
                ComplexParameter args1 = null;
                ComplexParameter args2 = null;
                ComplexParameter args3 = null;

                var autoResetEvent1 = new AutoResetEvent(false);
                var autoResetEvent2 = new AutoResetEvent(false);
                var autoResetEvent3 = new AutoResetEvent(false);

                var handler1 = new EventHandler<ComplexParameter>((s, args) =>
                {
                    sender1 = s;
                    args1 = args;
                    autoResetEvent1.Set();
                });
                remoteObject1.RegularEvent += handler1;

                var handler2 = new EventHandler<ComplexParameter>((s, args) =>
                {
                    sender2 = s;
                    args2 = args;
                    autoResetEvent2.Set();
                });
                remoteObject2.RegularEvent += handler2;
                
                var handler3 = new EventHandler<ComplexParameter>((s, args) =>
                {
                    sender3 = s;
                    args3 = args;
                    autoResetEvent3.Set();
                });
                remoteObject3.RegularEvent += handler3;

                //Act
                remoteObject1.RegularEvent -= handler2;
                remoteObject2.RegularEvent -= handler2;
                remoteObject3.RegularEvent -= handler3;
                
                var complexParameter1 = new ComplexParameter
                {
                    A = 5,
                    B = true,
                    D = 543.543,
                    S = "asd"
                };

                Assert.IsNull(sender1);
                Assert.IsNull(sender2);
                Assert.IsNull(sender3);
                Assert.IsNull(args1);
                Assert.IsNull(args2);
                Assert.IsNull(args3);

                //Act
                hostedObject.SendRegularEvent(complexParameter1);

                autoResetEvent1.WaitOne(2000);
                autoResetEvent2.WaitOne(2000);
                autoResetEvent3.WaitOne(2000);

                //Assert
                Assert.AreSame(remoteObject1, sender1);
                Assert.IsNull(sender2);
                Assert.IsNull(sender3);
                AssertComplexParameter(complexParameter1, args1);
                Assert.IsNull(args2);
                Assert.IsNull(args3);
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