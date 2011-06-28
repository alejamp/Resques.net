using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using StructureMap;
using RedisQueueExtensions;
using System.Threading;
using System.Threading.Tasks;
using Gurock.SmartInspect;

namespace RedisQueue
{

    /// <summary>
    /// FIFO Queue implementation which supports "new item" notification to the clients.
    /// QueueManager uses StructureMap IoC in order to instantiate the RedisClient.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueManager<T> : IQueue<T>, IDisposable
    {

        #region Members
        private const string MSG_NEWITEM = "NewItem";

        private string _Name;
        private IRedisClient _RedisClient
        {
            get
            {
                return ObjectFactory.GetInstance<IRedisClient>();
            }
        }
        private string PubSubChannelName { get { return "PSChannel:" + _Name;} }
        private Action<T> OnNewItem;
        public QueueManager(string name)
        {
            _Name = name;
        }
        #endregion

        /// <summary>
        /// Subscribe to new item notification. Action will be execute on each new item poped from the queue.
        /// If a notification has arrived but no item is returned from Pop(), there will not execute the Action.
        /// </summary>
        /// <param name="action">Action to process on each New Item</param>
        public void SubscribeForNewItem(Action<T> action)
        {
            OnNewItem = action;
            Task.Factory.StartNew(() =>
                {
                    using (var redisConsumer = _RedisClient)
                    using (var subscription = redisConsumer.CreateSubscription())
                    {
                        subscription.OnSubscribe = channel =>
                        {
                            SiAuto.Main.LogMessage("Subscribed to '{0}'", channel);
                        };
                        subscription.OnUnSubscribe = channel =>
                        {
                            SiAuto.Main.LogMessage("UnSubscribed from '{0}'", channel);
                        };
                        subscription.OnMessage = (channel, msg) =>
                        {
                            SiAuto.Main.LogMessage("Received '{0}' from channel '{1}'", msg, channel);
                            if (msg == MSG_NEWITEM)
                            {
                                using (var c = _RedisClient)
                                using (var client = c.GetTypedClient<T>())
                                {
                                    var pop = client.Queue<T>(_Name).Pop();
                                    if (pop != null)
                                        OnNewItem(pop);
                                }
                            }
                        };

                        SiAuto.Main.LogMessage("Started Listening On '{0}' Channel", PubSubChannelName);
                        subscription.SubscribeToChannels(PubSubChannelName); //blocking
                    }
                });
        }

        /// <summary>
        /// Clean the queue
        /// </summary>
        public void Flush()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(_Name).Flush();
            }
        }

        /// <summary>
        /// Return all elements from queue. But they remains on the queue.
        /// </summary>
        /// <returns></returns>
        public T[] GetElements()
        {
            using (var client = _RedisClient.GetTypedClient<T>())
            {
                return client.Queue<T>(_Name).GetElements();
            }
        }

        /// <summary>
        /// Removes an returns the first element in the queue.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                return client.Queue<T>(_Name).Pop();
            }
        }

        /// <summary>
        /// Push an element into the queue. No notification will be sent.
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            Push(item, false);
        }

        /// <summary>
        /// Push an element into the queue. With notification support.
        /// </summary>
        /// <param name="item">Element to enqueue</param>
        /// <param name="sendNotification">If true a notification will be sent to every client subscripted to this queue.</param>
        public void Push(T item, bool sendNotification)
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(_Name).Push(item);
            }

            // Send notifications to subscribers
            if (sendNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    SiAuto.Main.LogMessage("Publishing New Item Message");

                    using (var redisPublisher = _RedisClient)
                    {
                        redisPublisher.PublishMessage(PubSubChannelName, MSG_NEWITEM);
                    }
                });
            }
        }

        public void Dispose()
        {
            
        }
    }
}
