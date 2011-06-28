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
    public class QueueManager<T> : BaseManager<T>, IQueue<T>, IDisposable
    {

        public QueueManager(string name) : base(name)
        {
        }

        #region IQueue implementation
        /// <summary>
        /// Clean the queue
        /// </summary>
        public void Flush()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(Name).Flush();
            }
        }

        /// <summary>
        /// Return all elements from queue. But they remains on the queue.
        /// </summary>
        /// <returns></returns>
        public T[] GetElements()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                return client.Queue<T>(Name).GetElements();
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
                return client.Queue<T>(Name).Pop();
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
        #endregion

        /// <summary>
        /// Push an element into the queue. 
        /// This method optionaly can push a notification.
        /// </summary>
        /// <param name="item">Element to enqueue</param>
        /// <param name="sendNotification">If true a notification will be sent to every client subscripted to this queue.</param>
        public void Push(T item, bool sendNotification)
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(Name).Push(item);
            }

            // Send notifications to subscribers
            if (sendNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    Log.Debug("Publishing New Item Message");

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
