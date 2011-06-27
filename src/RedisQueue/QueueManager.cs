using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using StructureMap;
using RedisQueueExtensions;

namespace RedisQueue
{
    public class QueueManager<T> : IQueue<T>
    {
        private string _Name;

        private IRedisClient _RedisClient
        {
            get
            {
                return ObjectFactory.GetInstance<IRedisClient>();
            }
        }

        public QueueManager(string name)
        {
            _Name = name;
        }

        public void Flush()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(_Name).Flush();
            }
        }

        public T[] GetElements()
        {
            using (var client = _RedisClient.GetTypedClient<T>())
            {
                return client.Queue<T>(_Name).GetElements();
            }
        }

        public T Pop()
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                return client.Queue<T>(_Name).Pop();
            }
        }

        public void Push(T item)
        {
            using (var c = _RedisClient)
            using (var client = c.GetTypedClient<T>())
            {
                client.Queue<T>(_Name).Push(item);
            }
        }
    }
}
