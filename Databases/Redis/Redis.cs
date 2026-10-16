using StackExchange.Redis;

namespace Redis
{
    class Connection
    {
        private IDatabase redisDb;
        private ConnectionMultiplexer r;
        public Connection()
        {
            ConfigurationOptions conf = new ConfigurationOptions
            {
                EndPoints = { "10.0.0.28"}
            };
            r = ConnectionMultiplexer.Connect(conf);
            redisDb = r.GetDatabase();
        }

        public bool addWord(int id, string word, string language)
        {
            return redisDb.StringSet(id.ToString(), word);
        }
    }
}