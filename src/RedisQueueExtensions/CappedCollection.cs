using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis.Generic;
using ServiceStack.Redis;

namespace RedisQueueExtensions
{
    public static class RedisCappedCollectionExtension
    {
        public static CappedCollection<T> CappedCollection<T>(this IRedisTypedClient<T> redisClient, string name, int size)
        {
            return new CappedCollection<T>(redisClient, name, size);
        }
    }

    public class CappedCollection<T>
    {
        private IRedisTypedClient<T> _RedisClient;
        private string _Name;
        private int _Size;

        public CappedCollection(IRedisClient redisClient, string name, int size)
        {
            _RedisClient = redisClient.GetTypedClient<T>();
            _Name = name;
            _Size = size;
        }

        public CappedCollection(IRedisTypedClient<T> redisClient, string name, int size)
        {
            _RedisClient = redisClient;
            _Name = name;
            _Size = size;
        }

        public void Push(T item)
        {
            _RedisClient.Lists[_Name].Prepend(item);
            using (var trans = _RedisClient.CreateTransaction())
            {
                trans.QueueCommand(c => c.Lists[_Name].Push(item));
                trans.QueueCommand(c => c.Lists[_Name].Trim(0, _Size - 1));
                trans.Commit();
            }
        }

        public void Flush()
        {
            _RedisClient.Lists[_Name].Clear();
        }

        public T Pop()
        {
            return _RedisClient.Lists[_Name].RemoveEnd(); // RPOP
        }

        public T[] GetElements()
        {
            var list = _RedisClient.Lists[_Name].GetRange(0, -1);
            if (list != null) return list.ToArray();
            return null;
        }
    }
}
