using StackExchange.Redis;

namespace RedisRabbitmqDotnetCoreWriter  
{  
    public class Redis
    {  
        private Redis()
        {}  
        public static ConnectionMultiplexer conn;
        public static ConnectionMultiplexer GetInstance(string host, int port)
        {  
            if(conn==null)  
            {  
                var configOptions = new ConfigurationOptions
                {
                    ConnectTimeout = 5000,
                    ConnectRetry = 5,
                    SyncTimeout = 5000,
                    AbortOnConnectFail = false,
                };
                configOptions.EndPoints.Add(host, port);
                conn = ConnectionMultiplexer.Connect(configOptions);
            }
            return conn;  
        }
    }  
}  