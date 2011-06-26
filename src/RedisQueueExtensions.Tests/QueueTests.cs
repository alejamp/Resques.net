using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Redis;
using StructureMap;

namespace RedisQueueExtensions.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExtensionTests
    {

        public List<string> data;

        public ExtensionTests()
        {
            data = new List<string>(new string[] { "str1", "str2", "str3"});
            RedisQueue.Bootstrap.BootstrapManager.BootRedis();
        }

        private IRedisClient RedisClient
        {
            get
            {
                return ObjectFactory.GetInstance<IRedisClient>();
            }
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Queue_EnqueTest()
        {
            var qn = "q1";
            using(var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.Queue<string>(qn);
                q.Flush();

                data.ForEach(x => q.Push(x));
                Assert.AreEqual(client.Lists[qn].First(), data.Last());
            }
        }

        [TestMethod]
        public void Queue_Enque_Pop_Test()
        {
            var qn = "q1";
            using (var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.Queue<string>(qn);
                q.Flush();

                data.ForEach(x => q.Push(x));
                var pop = q.Pop();
                Assert.AreEqual(pop, data.First());
            }
        }

        [TestMethod]
        public void Queue_Enque_GetElements_Test()
        {
            var qn = "q1";            
            using (var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.Queue<string>(qn);
                q.Flush();

                data.ForEach(x => client.Queue<string>(qn).Push(x));
                var elements = q.GetElements();
                Assert.AreEqual(elements.First(), data.Last());
                Assert.AreEqual(elements.Last(), data.First());
            }
        }

        [TestMethod]
        public void CappedCollection_Enque_Test()
        {
            var qn = "cc1";
            using (var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.CappedCollection<string>(qn, 2);
                q.Flush();

                data.ForEach(x => q.Push(x));
                var length = q.GetElements().Length;
                Assert.AreEqual(length, 2);
            }
        }

        [TestMethod]
        public void CappedCollection_Enque_Pop_Test()
        {
            var qn = "cc1";
            using (var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.CappedCollection<string>(qn, 2);
                q.Flush();

                data.ForEach(x => q.Push(x));
                var pop1 = q.Pop();
                var pop2 = q.Pop();
                Assert.AreEqual(pop1, data[1]);
                Assert.AreEqual(pop2, data.Last());
            }
        }


        [TestMethod]
        public void Stack_Enque_Test()
        {
            var qn = "cc1";
            using (var client = RedisClient.GetTypedClient<string>())
            {
                var q = client.CappedCollection<string>(qn, 2);
                q.Flush();

                data.ForEach(x => q.Push(x));
                var length = q.GetElements().Length;
                Assert.AreEqual(length, 2);
            }
        }
    }
}
