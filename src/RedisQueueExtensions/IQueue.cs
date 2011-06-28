using System;
namespace RedisQueueExtensions
{
    public interface IQueue<T>
    {
        void Flush();
        T[] GetElements();
        T Pop();
        void Push(T item);
    }
}
