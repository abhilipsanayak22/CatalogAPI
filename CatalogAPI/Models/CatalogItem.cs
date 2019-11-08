using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Models
{
    public class CatalogItem
    {
        public CatalogItem()
        {
            Vendors = new List<Vendor>();
        }
        [BsonId(IdGenerator=typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int RecordLevel { get; set; }
        public string ImageUrl { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public List<Vendor> Vendors { get; set; }
    }

    public class Vendor
    {
        public string Name { get; set; }

        public string ContactNO { get; set; }

        public string Address { get; set; }
    }
}
