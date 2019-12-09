using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Xaas
{
    public class tweetClass
    {
            [BsonId]
            public ObjectId _id { get; set; }

            public string body { get; set; }
    }
}
