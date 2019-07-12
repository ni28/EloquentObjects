using System;
using EloquentObjects;
using NUnit.Framework;

namespace IntegrationTests.Robustness
{
    [TestFixture]
    public sealed class ConfigurationExceptions
    {

        [Test]
        [TestCase("tcp://127.0.0.1:50000", "pipe://127.0.0.1:50001")]
        //[TestCase("pipe://127.0.0.1:50000", "tcp://127.0.0.1:50001")]
        public void ShallThrowWhenSchemesAreDifferent(string serverAddress, string client1Address)
        {
            //Act
            var exception = Assert.Catch<ArgumentException>(() =>
            {
                using (new EloquentClient(serverAddress, client1Address))
                {

                }
            });

            //Assert
            Assert.AreEqual("Client Uri scheme should match server Uri scheme", exception.Message);
        }

        [Test]
        public void ShallThrowWhenSchemesIsUnknown()
        {
            //Act
            var exception1 = Assert.Catch<NotSupportedException>(() =>
            {
                using (new EloquentServer("qwe://127.0.0.1:50000"))
                {

                }
            });
            var exception2 = Assert.Catch<NotSupportedException>(() =>
            {
                using (new EloquentClient("qwe://127.0.0.1:50000", "qwe://127.0.0.1:50001"))
                {

                }
            });

            //Assert
            Assert.AreEqual("Scheme is not supported: qwe", exception1.Message);
            Assert.AreEqual("Scheme is not supported: qwe", exception2.Message);
        }
    }
}