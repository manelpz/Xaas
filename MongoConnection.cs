using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

namespace Xaas
{
    public class MongoConnection
    {
        public MongoConnection()
        {
        }
        public IConfiguration _config;

        #region Conection MongoDB

        public void insertTweets(List<string> ListTweets, IConfiguration config)
        {
            _config = config;
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
                    Console.WriteLine("Tweet duplicated");
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
                        Console.WriteLine("Tweet inserted successfully");
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
}
