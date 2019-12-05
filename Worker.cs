using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using MongoDB.Driver;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Xaas
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                // twitterConnection();
                mongoConnectionTest();
                 await Task.Delay(1000, stoppingToken);
            }
        }

        public void twitterConnection() {
            // Authentication
            Auth.SetUserCredentials("ConsumerKey", "ConsumerSecret", "AccessToken", "AccessTokenSecret");
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack("KEYWORD_TO_TRACK");

            // Limit to English 
            //stream.AddTweetLanguageFilter(LanguageFilter.English);

            Console.WriteLine("Listening to Twitter");

            stream.MatchingTweetReceived += (sender, arguments) =>
            {
                Console.WriteLine(arguments.Tweet.Text);
            };

            stream.StartStreamMatchingAllConditions();
        }

        public void mongoConnection() {

            MongoCRUD db = MongoCRUD("startupTweets");
            db.InsertRecord("ID", new tweetModel { tweet = "test tweet"});
            Console.ReadLine(); 

        }

        private MongoCRUD MongoCRUD(string v)
        {
            throw new NotImplementedException();
        }

        public class tweetModel {
            [BsonId]

            public Guid Id { get; set; }
            public string tweet { get; set; }
        }


        public async void mongoConnectionTest() {

            try
            {
                string connectstring1 = "mongodb://tweetdb:xaas123@ds251948.mlab.com:51948/tweetdb";
                MongoClient client = new MongoClient(connectstring1);
                var db = client.GetDatabase("tweetdb");
                var collection = db.GetCollection<BsonDocument>("tweet");
                var filter1 = Builders<BsonDocument>.Filter.Empty;
                var filter = new BsonDocument();
                /*using (var cursor = await collection.FindAsync(filter))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            Console.WriteLine(document[1].ToString());
                        }
                    }
                }*/


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }


    public class MongoCRUD {
        private IMongoDatabase db;

        public MongoCRUD(string database) {
            var client = new MongoClient();
            db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(String table, T record) {
            var collection = db.GetCollection<T>(table);
            collection.InsertOne(record);
        }
    }



   
}
