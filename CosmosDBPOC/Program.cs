using CosmosDBPOC.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDBPOC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("setting call to async method");
            var p = new Program();
            p.RunOrderSample().Wait();

            Console.WriteLine("sample completed.");
            Console.ReadKey();
        }

        private async Task RunOrderSample()
        {
            Console.WriteLine("Creating DB and Collection if necessary.");
            // creating order CRUD class
            var orderManager = new OrderManager();

            Console.WriteLine("Creating Test data.");
            // get some test data, there are two orders in here
            var integrationData = orderManager.GetTestIntegrationData();

            // insert the two new orders
            foreach (var data in integrationData)
            {
                await orderManager.CreateIntegrationData(data);
            }

            //Console.WriteLine("Updating an Order.");

            //// get one of the orders back,
            //var smallCustomerOrders = orderManager.QueryWithSql("STARTUP123");

            //// update the quantity on one of the line items on the order
            //var smallOrder = smallCustomerOrders.First();
            //smallOrder.LineItems.First().Quantity = 16;

            //// save the changes
            //await orderManager.UpdateOrder(smallOrder);

            //// look it up again to show that it saved
            //var smallOrderUpdated = await orderManager.GetOrderById(smallOrder.Id);

            //Console.WriteLine("order was updated to - {0}", smallOrderUpdated);
            //Console.WriteLine("the quantity of line item 1 is {0}", smallOrderUpdated.LineItems.First().Quantity);
        }
    }



    public class OrderManager : IDisposable
    {
        private DocumentClient _cosmosConnection;
        private string _databaseName;
        private string _orderCollectionName;

        public OrderManager()
        {
            // Setup client connection, and create DB and order collection if they don't exist yet
            var url = "https://ncosmos.documents.azure.com:443/";
            var key = "0elOlcPejVJ9QHLugv9ZRvTOrpKANzlBsuZD0nZC7ZKNV1ADMNb1DKiw4YuJVcRyZoTIDOvF8hODnzNZC9FUWw==";

            _databaseName = "OrdersDB";
            _orderCollectionName = "DFIntegrationMapping";

            _cosmosConnection = new DocumentClient(new Uri(url), key);

            // Create the database if it does not exist
            _cosmosConnection.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseName }).Wait();

            // Create the collection for Orders in the database if it does not exist
            _cosmosConnection.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(
            _databaseName), new DocumentCollection { Id = _orderCollectionName }).Wait();

        }

        public async Task CreateOrder(HardwareOrder order)
        {
            await _cosmosConnection.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(
            _databaseName, _orderCollectionName), order);
        }

        public async Task CreateIntegrationData(DFIntegrationMapping data)
        {
            var result =  await _cosmosConnection.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(
            _databaseName, _orderCollectionName), data);
            Console.WriteLine(result.Resource.Id);
        }

        public async Task UpdateOrder(HardwareOrder order)
        {
            await _cosmosConnection.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(
            _databaseName, _orderCollectionName, order.Id), order);
        }

        public async Task DeleteOrder(HardwareOrder order)
        {
            await _cosmosConnection.DeleteDocumentAsync(UriFactory.CreateDocumentUri(
            _databaseName, _orderCollectionName, order.Id));
        }

        public async Task<HardwareOrder> GetOrderById(string orderId)
        {
            var response = await _cosmosConnection.ReadDocumentAsync<HardwareOrder>(UriFactory.CreateDocumentUri(
            _databaseName, _orderCollectionName, orderId));
            return response.Document;
        }

        public List<HardwareOrder> QueryWithLinq(string customerNumber)
        {
            var queryOptions = new FeedOptions { MaxItemCount = -1 };

            var orders = _cosmosConnection.CreateDocumentQuery<HardwareOrder>(
                UriFactory.CreateDocumentCollectionUri(
                _databaseName, _orderCollectionName), queryOptions)
                    .Where(f => f.CustomerNumber == customerNumber);

            return orders.ToList();
        }

        public List<HardwareOrder> QueryWithSql(string customerNumber)
        {
            var queryOptions = new FeedOptions { MaxItemCount = -1 };

            var orders = _cosmosConnection.CreateDocumentQuery<HardwareOrder>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, _orderCollectionName),
                $"SELECT * FROM HardwareOrder WHERE HardwareOrder.CustomerNumber = '{customerNumber}'",
                queryOptions);

            return orders.ToList();
        }

        public void Dispose()
        {
            if (_cosmosConnection != null)
            {
                _cosmosConnection.Dispose();
            }
        }

        public List<HardwareOrder> GetTestOrders()
        {
            return new List<HardwareOrder>
                (

                new HardwareOrder[] {

                    new HardwareOrder
                    {
                        Id = "BIGCORP.1",
                        CustomerNumber = "BIGCORP123",
                        OrderDate = DateTime.UtcNow,
                        ShipDate = DateTime.UtcNow.AddDays(14),
                        LineItems = new LineItem[]
                        {
                            new LineItem{
                            Id = 555,
                            Product = "Laptop",
                            Quantity = 1500,
                            UnitPrice = 953.12m
                            },
                            new LineItem{
                            Id = 556,
                            Product = "Server",
                            Quantity = 14,
                            UnitPrice = 2257.16m
                            },
                            new LineItem{
                            Id = 557,
                            Product = "Power Supply",
                            Quantity = 223,
                            UnitPrice = 24.99m
                            },
                        }
                    },

                    new HardwareOrder
                    {
                        Id = "STARTUP.1",
                        CustomerNumber = "STARTUP123",
                        OrderDate = DateTime.UtcNow,
                        ShipDate = DateTime.UtcNow.AddDays(14),
                        LineItems = new LineItem[]
                        {
                            new LineItem{
                            Id = 1555,
                            Product = "Laptop",
                            Quantity = 15,
                            UnitPrice = 1250.11m
                            },
                            new LineItem{
                            Id = 1556,
                            Product = "Server",
                            Quantity = 2,
                            UnitPrice = 2699.99m
                            },
                            new LineItem{
                            Id = 1557,
                            Product = "Power Supply",
                            Quantity = 15,
                            UnitPrice = 24.99m
                            },
                        }
                    }
                });
        }

        public List<DFIntegrationMapping> GetTestIntegrationData()
        {
            List<DFIntegrationMapping> dFIntegrationMappings = new List<DFIntegrationMapping>();

            dFIntegrationMappings.Add(new DFIntegrationMapping()
            {
                SourceAppId = 1,
                EntityId = 10,
                EntityType = "Person",
                SourceAppDesc = "Person",
                SourceAppName = "NIF ISD Azure",
            });

            return dFIntegrationMappings;
        }
    }
}
