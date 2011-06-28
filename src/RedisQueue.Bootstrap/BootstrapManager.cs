using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurock.SmartInspect;
using StructureMap;
using ServiceStack.Redis;
using System.Configuration;

namespace RedisQueue.Bootstrap
{
    public static class BootstrapManager
    {
        public static void BootRedis()
        {
            ObjectFactory.Configure(x =>
            {
                // RedisIO
                x.For<IRedisClientsManager>().Singleton().Use(InitRedisClientsManager);
                x.For<IRedisClient>().Use(GetRedisClient);
            });
        }

        public static void InitLogging()
        {
            SiAuto.Si.Enabled = true;
            SiAuto.Main.Name = "RedisExtensions";
        }

        private static IRedisClientsManager InitRedisClientsManager()
        {
            List<string> rwServers = new List<string>();
            List<string> roServers = new List<string>();
            rwServers.Add(ConfigurationManager.AppSettings["RedisMasterServerUrl"]);
            roServers.Add(ConfigurationManager.AppSettings["RedisSlave1ServerUrl"]);
            var redisClientManager = new PooledRedisClientManager(rwServers, roServers);
            return redisClientManager;
        }

        private static IRedisClient GetRedisClient()
        {
            var redisClientManager = ObjectFactory.GetInstance<IRedisClientsManager>();
            return redisClientManager.GetClient();
        }
    }
}
