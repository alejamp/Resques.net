using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis.Generic;
using ServiceStack.Redis;

namespace RedisQueueExtensions
{

    public static class RedisStackExtension
    {
        public static Stack<T> Stack<T>(this IRedisTypedClient<T> redisClient, string name)
        {
            return new Stack<T>(redisClient, name);
        }

    }
    public class Stack<T>
    {
        private IRedisTypedClient<T> _RedisClient;
        private string _Name;

        public Stack(IRedisClient redisClient, string name)
        {
            _RedisClient = redisClient.GetTypedClient<T>();
            _Name = name;
        }

        public Stack(IRedisTypedClient<T> redisClient, string name)
        {
            _RedisClient = redisClient;
            _Name = name;
        }

        public void Push(T item)
        {
            _RedisClient.Lists[_Name].Prepend(item);
        }

        public void Flush()
        {
            _RedisClient.Lists[_Name].Clear();
        }

        public T Pop()
        {
            return _RedisClient.Lists[_Name].RemoveStart();
        }

        public T[] GetElements()
        {
            var list = _RedisClient.Lists[_Name].GetRange(0, -1);
            if (list != null) return list.ToArray();
            return null;
        }
    }
}
