using Newtonsoft.Json;
using System;

namespace CosmosDBPOC.Model
{
    public class HardwareOrder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string CustomerNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShipDate { get; set; }
        public LineItem[] LineItems { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class LineItem
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class DFIntegrationMapping
    {
        [JsonProperty(PropertyName = "id")]
        public string MappingId { get; set; }
        public int SourceAppId { get; set; }
        public int DFEntityId { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string SourceAppName { get; set; }
        public string SourceAppDesc { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
