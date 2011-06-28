using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Redis;
using StructureMap;
using RedisQueue;
using System.Threading;
using System.Threading.Tasks;
using Gurock.SmartInspect;

namespace RedisQueueExtensions.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class QueueManagerTests
    {

        public List<string> data;

        public QueueManagerTests()
        {
            data = new List<string>(new string[] { "str1", "str2", "str3" });
            RedisQueue.Bootstrap.BootstrapManager.InitLogging();
            RedisQueue.Bootstrap.BootstrapManager.BootRedis();
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
        public void QueueManager_EnqueTest()
        {
            //Init
            var qn = "q1";
            var q = new QueueManager<string>(qn);
            q.Flush();
            // Push items into queue
            data.ForEach(x => q.Push(x));
            // Test 
            Assert.AreEqual(q.GetElements().Length, data.Count);        
        }

        [TestMethod]
        public void Queue_Enque_Pop_Test()
        {
            // Init
            var qn = "q1";
            var q = new QueueManager<string>(qn);
            q.Flush();

            // Push items into queue q1
            data.ForEach(x => q.Push(x));

            // Pop and test
            Assert.AreEqual(q.Pop(), data.First());
        }

        [TestMethod]
        public void Test_Multiple_Push_And_Pop()
        {
            int count = 100;
            var qn = "q1";
            var q = new QueueManager<string>(qn);
            q.Flush();

            for (int i = 0; i < count; i++)
            {
                q.Push(i.ToString());
            }
            for (int i = 0; i < count; i++)
            {
                var p = q.Pop();
                Assert.AreEqual(int.Parse(p), i);
            }
        }

        [TestMethod]
        public void Test_Queue_Notifications_Concurrency()
        {
            int count = 100;
            var qn = "q1";
            var cq1 = new QueueManager<string>(qn);
            var cq2 = new QueueManager<string>(qn);
            var incomingMessages1 = new List<string>();
            var incomingMessages2 = new List<string>();
            cq1.Flush();
            cq2.Flush();

            cq1.SubscribeForNewItem(x => {
                incomingMessages1.Add(x);
                SiAuto.Main.LogMessage("Incoming item cq1 Value:" + x);
            });
            cq2.SubscribeForNewItem(x =>
            {
                incomingMessages2.Add(x);
                SiAuto.Main.LogMessage("Incoming item cq2 Value:" + x);
            });

            int xx = 0;
            for (int i = 0; i < count; i++)
            {
                Task.Factory.StartNew(() => {
                    int item = xx++;
                    SiAuto.Main.LogMessage("New Push:" + item.ToString() + " Thread:" + Thread.CurrentThread.ManagedThreadId);
                    cq1.Push(item.ToString(), true); 
                });
            }

            SiAuto.Main.LogMessage("Waiting...");

            Thread.Sleep(2000);
            Assert.AreEqual(incomingMessages1.Count + incomingMessages2.Count, count);

            SiAuto.Main.LogMessage("Check if the queues are empty.");
            var p1 = cq1.Pop();
            Assert.IsNull(p1);
            SiAuto.Main.LogMessage("Done!");
        }

        // TODO
        //[TestMethod]
        //public void CappedCollection_Enque_Test()
        //{
        //    var qn = "cc1";
        //    using (var client = RedisClient.GetTypedClient<string>())
        //    {
        //        var q = client.CappedCollection<string>(qn, 2);
        //        q.Flush();

        //        data.ForEach(x => q.Push(x));
        //        var length = q.GetElements().Length;
        //        Assert.AreEqual(length, 2);
        //    }
        //}

        //[TestMethod]
        //public void CappedCollection_Enque_Pop_Test()
        //{
        //    var qn = "cc1";
        //    using (var client = RedisClient.GetTypedClient<string>())
        //    {
        //        var q = client.CappedCollection<string>(qn, 2);
        //        q.Flush();

        //        data.ForEach(x => q.Push(x));
        //        var pop1 = q.Pop();
        //        var pop2 = q.Pop();
        //        Assert.AreEqual(pop1, data[1]);
        //        Assert.AreEqual(pop2, data.Last());
        //    }
        //}


        //[TestMethod]
        //public void Stack_Enque_Test()
        //{
        //    var qn = "cc1";
        //    using (var client = RedisClient.GetTypedClient<string>())
        //    {
        //        var q = client.CappedCollection<string>(qn, 2);
        //        q.Flush();

        //        data.ForEach(x => q.Push(x));
        //        var length = q.GetElements().Length;
        //        Assert.AreEqual(length, 2);
        //    }
        //}
    }
}
