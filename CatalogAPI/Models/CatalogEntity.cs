using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Models
{
    public class CatalogEntity : TableEntity
    {
        public CatalogEntity(string name, string id)
        {
            PartitionKey = name;
            RowKey = id;
        }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int RecordLevel { get; set; }
        public string ImageUrl { get; set; }
        public DateTime ManufacturingDate { get; set; }
    }
}
