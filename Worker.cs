using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using MongoDB.Driver;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Microsoft.Extensions.Configuration;

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
                twitterConnection();
                await Task.Delay(30 * 1000, stoppingToken);
            }
        }


        public void twitterConnection()
        {

            List<string> arrayTweets = new List<string>();

            Auth.SetUserCredentials("rtI8kLDqS1Q8K8NwHf4MGYOiA", "niRhPhndBvgwjThqmWq7iaDcvzihq5GhPcOYA4U82BYMadIMEO", "80160043-7tTguB43pjToVfPwPqk0kyAf6L2SzJLFKz4Ac1oY6", "FNch3gXRk0wzTgFe8uiqAJcs6mSwrsDN9WqpoZPfCQc5X");
            var stream = Stream.CreateFilteredStream();
            //stream.AddTweetLanguageFile(LanguageFilter.English);
            stream.AddTrack("love");

            stream.MatchingTweetReceived += (sender, arguments) =>
            {
                //  stream
                arrayTweets.Add(arguments.Tweet.Text);
                //Console.WriteLine(arguments.Tweet.Text);
                if (arrayTweets.Count().Equals(30))
                {
                    stream.PauseStream();
                    getTweet(arrayTweets);
                }
                

            };

            stream.StartStreamMatchingAllConditions();
        }

        #region tweeter
        public void getTweet(List<string> arrayTweets)
        {
            IMongoClient client = new MongoClient("mongodb://tweetUser:xaas123@ds251948.mlab.com:51948/tweetdb");
            IMongoDatabase database = client.GetDatabase("tweetdb");

            var collection = database.GetCollection<BsonDocument>("tweets");

            foreach (string arrayTweet in arrayTweets)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("body", arrayTweet);
                var result = collection.Find(filter).FirstOrDefault();
                if (result != null)
                {
                    Console.WriteLine("value duplicated");
                }
                else
                {
                    //insert
                    Console.WriteLine("Insert");
                    tweetClass tweetClass = new tweetClass();
                    tweetClass.body = arrayTweet;

                    insertTweet(tweetClass);

                }
            }

        }

        public void insertTweet(tweetClass tweetClass)
        {
            IMongoClient client = new MongoClient("mongodb://tweetUser:xaas123@ds251948.mlab.com:51948/tweetdb?retryWrites=false");
            IMongoDatabase database = client.GetDatabase("tweetdb");
            var collection = database.GetCollection<BsonDocument>("tweets");
            BsonDocument documento = tweetClass.ToBsonDocument();

            try
            {
                collection.InsertOne(documento);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        #endregion tweeter

    }

    #region body class

        public class tweetClass
        {

            [BsonId]
            public ObjectId _id { get; set; }

            public string body { get; set; }

        }
    #endregion
}

