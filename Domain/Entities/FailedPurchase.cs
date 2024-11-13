using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FailedPurchase
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("CardNumber")]
        public string CardNumber { get; set; }

        [BsonElement("PurchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [BsonElement("Amount")]
        public decimal Amount { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [BsonElement("IsRetriable")]
        public bool IsRetriable { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
