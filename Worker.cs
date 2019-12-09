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
        private IConfiguration _config;

        
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        #region Worker service
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //call the function that connects with twitter API
                twitterConnection();

                //delay the time to recall the worker service, multiply by 30 to get a call each 30 seconds
                await Task.Delay(30 * 1000, stoppingToken);
            }
        }
        #endregion

        #region Conection Twitter


        public void twitterConnection()
        {

            List<string> arrayTweets = new List<string>();

            try
            {
                //NOTE: find the twitter app setteing in the appsettings.json
                //call the settings of twitter and add to the class Tweetinvi (installed for get the conection with twitter)
                Auth.SetUserCredentials(_config["ConsumerKey"], _config["ConsumerSecret"], _config["AccessToken"], _config["AccessTokenSecret"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var stream = Stream.CreateFilteredStream();

            //NOTE: If you want to minimize the number of tweets catched uncomment the next line
            //stream.AddTweetLanguageFile(LanguageFilter.English);

            //NOTE: I chaged the string to love cause the stream tweet ts more fast
            stream.AddTrack(_config["HashTag"]);

            stream.MatchingTweetReceived += (sender, arguments) =>
            {
                //add the stream to the list
                arrayTweets.Add(arguments.Tweet.Text);
              
                if (arrayTweets.Count().Equals(30))
                {
                    //pauses the stream  if the list have 30 tweets
                    stream.PauseStream();

                    //call the function that read the list of tweets
                    insertTweets(arrayTweets);
                }
                

            };
            //start the stream based on the parameters added
            stream.StartStreamMatchingAllConditions();
        }
        #endregion

        #region Conection MongoDB

        public void insertTweets(List<string> ListTweets)
        {
            //get the conection to the database
            IMongoClient client = new MongoClient(_config["ConectionURL"]);
            IMongoDatabase database = client.GetDatabase(_config["ConectionDB"]);
            var collection = database.GetCollection<BsonDocument>(_config["ConectionCollection"]);

            //instance the class param
            tweetClass tweetClass = new tweetClass();

            //read the list looking for matches
            foreach (string ListTweet in ListTweets)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("body", ListTweet);
                var result = collection.Find(filter).FirstOrDefault();

                //if the tweet body its equals to the text of the query send the duplicate mesage
                if (result != null)
                {
                    Console.WriteLine("value duplicated");
                }
                else
                {
                    //call the insert the tweets

                    tweetClass.body = ListTweet;
                    //insertTweet(tweetClass);

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
            }

        }

        
        #endregion 

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

