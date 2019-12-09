using System;
using System.Collections.Generic;
using Tweetinvi;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Xaas
{
    public class twitterConnection
    {

        public IConfiguration _config;

        public twitterConnection() {}


        #region Conection Twitter

        public void twitterFunction(IConfiguration config)
        {
            int tweetsNumber = 0;
            _config = config;
            List<string> arrayTweets = new List<string>();

            try
            {
                //NOTE: find the twitter app setteing in the appsettings.json
                //call the settings of twitter and add to the class Tweetinvi (installed for get the conection with twitter)
                Auth.SetUserCredentials(_config["ConsumerKey"], _config["ConsumerSecret"], _config["AccessToken"], _config["AccessTokenSecret"]);
                tweetsNumber = Int32.Parse(_config["MaximunNumber"]);
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

                stream.ResumeStream();

                //add the stream to the list
                arrayTweets.Add(arguments.Tweet.Text);
                
                if (arrayTweets.Count().Equals(tweetsNumber))
                {
                    //pauses the stream  if the list have 30 tweets
                    stream.PauseStream();

                    //call the function that read the list of tweets
                    MongoConnection MongoConnectionClass = new MongoConnection();
                    MongoConnectionClass.insertTweets(arrayTweets, _config);
                    Console.WriteLine("Execution finished");

                    //finish the stream
                    stream.StopStream();
                }


            };
            //start the stream based on the parameters added
            stream.StartStreamMatchingAllConditions();
        }
        #endregion
    }
}
