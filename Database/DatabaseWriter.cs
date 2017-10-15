using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace LibHitomi.Database
{
    public class DatabaseWriter : LibHitomi.ListDownloaderBase
    {
        MongoClient mongoClient;
        public DatabaseWriter(string connectionString)
        {
            mongoClient = new MongoClient(connectionString);
        }
        public DatabaseWriter(MongoClient mongoClient)
        {
            this.mongoClient = mongoClient;
        }
        public async void WriteAsync(string databaseName, string collectionName)
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);
            FilterDefinitionBuilder<BsonDocument> filterBuilder = new FilterDefinitionBuilder<BsonDocument>();
            UpdateDefinitionBuilder<BsonDocument> updateDefBuilder = new UpdateDefinitionBuilder<BsonDocument>();
            int chunkCount = getJsonCount();
            List<WriteModel<BsonDocument>> actions = new List<WriteModel<BsonDocument>>();
            actions.Add(new DeleteManyModel<BsonDocument>(filterBuilder.Eq("crawlMethod", GalleryCrawlMethod.Normal)));
            for(int i = 0; i < chunkCount; i++)
            {
                HttpWebRequest request = RequestHelper.CreateRequest(DownloadOptions.JsonSubdomain, $"/galleries{i}.json");
                using (WebResponse wres = await request.GetResponseAsync())
                using (Stream resstr = wres.GetResponseStream())
                using (StreamReader sre = new StreamReader(resstr))
                {
                    foreach (BsonDocument doc in BsonSerializer.Deserialize<BsonArray>(sre).Select(p => p.AsBsonDocument))
                        actions.Add(new InsertOneModel<BsonDocument>(doc));
                }
            }
            actions.Add(new UpdateManyModel<BsonDocument>(filterBuilder.Exists("crawlMethod", false), updateDefBuilder.Set("crawlMethod", GalleryCrawlMethod.Normal)));
            collection.BulkWrite(actions);
        }
    }
}
