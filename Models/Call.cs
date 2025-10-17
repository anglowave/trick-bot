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

    [BsonElement("currentMarketCap")]
    public decimal CurrentMarketCap { get; set; }

    [BsonElement("callTime")]
    public DateTime CallTime { get; set; } = DateTime.UtcNow;

    [BsonElement("pnl")]
    public decimal PnL => CurrentMarketCap - MarketCapAtCall;

    [BsonElement("pnlPercentage")]
    public decimal PnLPercentage => MarketCapAtCall > 0 ? (PnL / MarketCapAtCall) * 100 : 0;
}
