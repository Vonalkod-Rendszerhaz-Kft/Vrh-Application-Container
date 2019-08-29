using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.Redis.DataPoolHandler;

namespace Vrh.DataToRedisCore
{
    public class PoolHandler: Vrh.Redis.DataPoolHandler.PoolHandler
    {
        #region Constructors
        public PoolHandler(RedisConnectionString connectionstring)
            : base(connectionstring)
        {
            Name = connectionstring.PoolName;
        }
        public PoolHandler(string poolName,Serializers serializer)
            : base(poolName, serializer)
        {
            Name = poolName;
        }

        public PoolHandler(string poolName, string redisServer, Serializers serializer)
            : base(poolName, redisServer, serializer)
        {
            Name = poolName;
        }

        public PoolHandler(string poolName, string redisServer, int redisPort, Serializers serializer)
            : base(poolName, redisServer, redisPort, serializer)
        {
            Name = poolName;
        }

        public PoolHandler(string poolName, string redisServer, int redisPort, int syncTimout, Serializers serializer)
            : base(poolName, redisServer, redisPort, syncTimout, serializer)
        {
            Name = poolName;
        }

        public PoolHandler(string poolName, string redisServer, int redisPort, int syncTimout, int connectTimout, Serializers serializer)
            : base(poolName, redisServer, redisPort, syncTimout, connectTimout, serializer)
        {
            Name = poolName;
        }
        #endregion

        public string Name { get; private set; }

    }
}
