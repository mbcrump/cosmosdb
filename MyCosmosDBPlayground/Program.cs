using Microsoft.Azure.Documents.Client;
using System.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MyCosmosDBPlayground
{
    class Program
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;

        static void Main(string[] args)
        {
            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();

            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            IQueryable<dynamic> bookQuery = client.CreateDocumentQuery<dynamic>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), queryOptions);
                    
            Console.WriteLine("Running query...");
            foreach (dynamic book in bookQuery)
            {
                Console.WriteLine("\tRead {0}", book.book);
                Console.WriteLine("\tRead {0}", book.chapters);
            }
           
            Console.Read();
        }


        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection
                        {
                            Id = CollectionId,
                            PartitionKey = new PartitionKeyDefinition() { Paths = new Collection<string>() { "/keyId" } }
                        },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public class Bible
    {
        [JsonProperty(PropertyName = "book")]
        public string Book { get; set; }

        [JsonProperty(PropertyName = "chapters")]
        public string Chapters { get; set; }
       
    }

}
