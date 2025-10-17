using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Trick.Models;

public class Call
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("marketCapAtCall")]
    public decimal MarketCapAtCall { get; set; }

    [BsonElement("callTime")]
    public DateTime CallTime { get; set; } = DateTime.UtcNow;

    public decimal CalculatePnL(decimal currentMarketCap) => currentMarketCap - MarketCapAtCall;

    public decimal CalculatePnLPercentage(decimal currentMarketCap) => 
        MarketCapAtCall > 0 ? (CalculatePnL(currentMarketCap) / MarketCapAtCall) * 100 : 0;
}
