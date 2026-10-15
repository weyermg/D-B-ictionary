using StackExchange.Redis;

namespace Redis
{
    class Connection
    {
        IDatabase db;
        ConnectionMultiplexer r;
        public Connection()
        {
            ConfigurationOptions conf = new ConfigurationOptions
            {
                EndPoints = { "10.0.0.28"}
            };
            r = ConnectionMultiplexer.Connect(conf);
            db = r.GetDatabase();
        }

        
    }
}