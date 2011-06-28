using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using StructureMap;
using System.Threading.Tasks;
using Gurock.SmartInspect;
using RedisQueueExtensions;

namespace RedisQueue
{
    public class BaseManager<T>
    {
        #region Members
        public const string MSG_NEWITEM = "NewItem";

        private string _Name;
        public string Name { get { return _Name; } }
        public IRedisClient _RedisClient { get {return ObjectFactory.GetInstance<IRedisClient>();}}
        public string PubSubChannelName { get { return "PSChannel:" + _Name;} }
        private Action<T> OnNewItem;

        /// <summary>
        /// Use this flag to indicate if you are ready to process new items.
        /// If there are no resources to process a new item, you must set Busy = true.
        /// When Busy = true, the manager ignores the notifications for "New Item".
        /// </summary>
        public bool Busy { get; set; }
        #endregion

        public BaseManager(string name)
        {
            _Name = name;
        }

        /// <summary>
        /// Subscribe to new item notification. This Action is executed for each new item poped from the queue.
        /// If a notification has arrived but no item is returned from Pop(), there will not execute the Action.
        /// </summary>
        /// <param name="action">Invoked Action on each New Item</param>
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
                        Log.Debug("Subscribed to '{0}'", channel);
                    };
                    subscription.OnUnSubscribe = channel =>
                    {
                        Log.Debug("UnSubscribed from '{0}'", channel);
                    };
                    subscription.OnMessage = (channel, msg) =>
                    {
                        Log.Debug("Received '{0}' from channel '{1}' Busy:", msg, channel, Busy.ToString());
                        if (!Busy)
                        {
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
                        }
                    };

                    Log.Debug("Started Listening On '{0}' Channel", PubSubChannelName);

                    //Bocking call
                    subscription.SubscribeToChannels(PubSubChannelName);
                }
            });
        }
    }
}
