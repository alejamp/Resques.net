using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis.Generic;
using ServiceStack.Redis;

namespace RedisQueueExtensions
{
    public static class RedisQueueExtension
    {
        public static Queue<T> Queue<T>(this IRedisTypedClient<T> redisClient, string name)
        {
            return new Queue<T>(redisClient, name);
        }
    }

    public class Queue<T> : RedisQueueExtensions.IQueue<T>
    {
        private IRedisTypedClient<T> _RedisClient;
        private string _Name;

        public Queue(IRedisClient redisClient, string name)
        {
            _RedisClient = redisClient.GetTypedClient<T>();
            _Name = name;
        }

        public Queue(IRedisTypedClient<T> redisClient, string name)
        {
            _RedisClient = redisClient;
            _Name = name;
        }

        public void Push(T item)
        {
            _RedisClient.Lists[_Name].Prepend(item);
        }

        public T Pop()
        {
            return _RedisClient.Lists[_Name].Pop();
        }

        public void Flush()
        {
            _RedisClient.Lists[_Name].Clear();
        }

        public T[] GetElements()
        {
            var list = _RedisClient.Lists[_Name].GetRange(0, -1);
            if (list != null) return list.ToArray();
            return null;
        }
    }
}
