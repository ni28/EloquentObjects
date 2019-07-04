﻿using System;
using System.Runtime.Serialization;
using System.Threading;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public sealed class PropertiesTests
    {
        [EloquentContract]
        public interface IContract
        {
            [EloquentProperty]
            int Get { get; }
            
            [EloquentProperty]
            int Set { set; }

            [EloquentProperty(IsOneWay = true)]
            int OneWaySet { set; }

            [EloquentProperty]
            int GetSet { get; set; }

            [EloquentProperty(IsOneWay = true)]
            int OneWayGetSet { get; set; }
            
            [EloquentProperty]
            ComplexParameter ComplexGetSet { get; set; }            
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
            
            public int Value { get; set; }
            public ComplexParameter ComplexValue { get; set; }

            public Action Action { get; set; } = () => { };

            public void WaitSet()
            {
                _autoResetEvent.WaitOne();
            }
            
            #region Implementation of IContract

            public int Get
            {
                get
                {
                    Action();
                    return Value;
                }
            }

            public int Set
            {
                set
                {
                    Action();
                    Value = value;
                }
            }

            public int OneWaySet
            {
                set
                {
                    Action();
                    Value = value;
                    _autoResetEvent.Set();
                }
            }

            public int GetSet
            {
                get
                {
                    Action();
                    return Value;
                }
                set
                {
                    Action();
                    Value = value;
                }
            }

            public int OneWayGetSet
            {
                get
                {
                    Action();
                    return Value;
                }
                set
                {
                    Action();
                    Value = value;
                    _autoResetEvent.Set();
                }
            }

            public ComplexParameter ComplexGetSet
            {
                get
                {
                    Action();
                    return ComplexValue;
                }
                set
                {
                    Action();
                    ComplexValue = value;
                }
            }

            #endregion
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallGetValuesForProperties(string serverAddress, string clientAddress)
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

                    //Act
                    hostedObject.Value = 5;
                    hostedObject.ComplexValue = new ComplexParameter
                    {
                        A=1,
                        B=true,
                        D=123.123,
                        S="qwerty"
                    };

                    //Assert
                    Assert.AreEqual(5, remoteObject.Get);
                    Assert.AreEqual(5, remoteObject.GetSet);
                    Assert.AreEqual(5, remoteObject.OneWayGetSet);
                    AssertComplexParameter(new ComplexParameter
                    {
                        A=1,
                        B=true,
                        D=123.123,
                        S="qwerty"
                    }, remoteObject.ComplexGetSet );

                    //Act
                    hostedObject.Value = 7;
                    hostedObject.ComplexValue = new ComplexParameter
                    {
                        A=2,
                        B=false,
                        D=234.234,
                        S="asd"
                    };

                    //Assert
                    Assert.AreEqual(7, remoteObject.Get);
                    Assert.AreEqual(7, remoteObject.GetSet);
                    Assert.AreEqual(7, remoteObject.OneWayGetSet);
                    AssertComplexParameter(new ComplexParameter
                    {
                        A=2,
                        B=false,
                        D=234.234,
                        S="asd"
                    }, remoteObject.ComplexGetSet );
                }
            }
        }
        
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallSetValuesForProperties(string serverAddress, string clientAddress)
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

                    var complexParameter = new ComplexParameter
                    {
                        A = 2,
                        B = false,
                        D = 234.234,
                        S = "asd"
                    };
                    hostedObject.ComplexValue = complexParameter;

                    Assert.AreEqual(0, hostedObject.Value);
                    AssertComplexParameter(complexParameter, hostedObject.ComplexValue);
                    
                    //Act
                    remoteObject.Set = 5;
                    //Assert
                    Assert.AreEqual(5, hostedObject.Value);

                    //Act
                    remoteObject.OneWaySet = 7;
                    hostedObject.WaitSet();
                    //Assert
                    Assert.AreEqual(7, hostedObject.Value);

                    //Act
                    remoteObject.OneWayGetSet = 9;
                    hostedObject.WaitSet();
                    //Assert
                    Assert.AreEqual(9, hostedObject.Value);

                    //Act
                    remoteObject.GetSet = 11;
                    //Assert
                    Assert.AreEqual(11, hostedObject.Value);
                    
                    //Act
                    var complexParameter2 = new ComplexParameter
                    {
                        A=1,
                        B=true,
                        D=123.123,
                        S="qwerty"
                    };
                    remoteObject.ComplexGetSet = complexParameter2;
                    //Assert
                    AssertComplexParameter(complexParameter2, hostedObject.ComplexValue);

                }
            }
        }
        
        [Test]
        [TestCase("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        [TestCase("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        public void ShallRaiseExceptionForTwoWayProperties(string serverAddress, string clientAddress)
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
                    hostedObject.Value = 5;
                    hostedObject.Action = () =>
                    {
                        throw new InvalidOperationException("qwerty");
                    };
                    
                    //Act
                    int val = 0;
                    var exception = Assert.Catch<FaultException>(() =>
                    {
                        val = remoteObject.Get;
                    } );
                    //Assert
                    Assert.AreEqual(0, val);
                    Assert.AreEqual("qwerty", exception.Message);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, exception.ExceptionType);
                    
                    //Act
                    exception = Assert.Catch<FaultException>(() =>
                    {
                        val = remoteObject.GetSet;
                    } );
                    //Assert
                    Assert.AreEqual(0, val);
                    Assert.AreEqual("qwerty", exception.Message);
                    Assert.AreEqual(typeof(InvalidOperationException).FullName, exception.ExceptionType);
                    
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