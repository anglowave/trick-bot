using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Trick.Models;

namespace Trick.Services;

public class CallService
{
    private readonly IMongoCollection<Call> _calls;
    private readonly ILogger<CallService> _logger;

    public CallService(IConfiguration configuration, ILogger<CallService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _calls = database.GetCollection<Call>("calls");
    }

    public async Task<Call> CreateCallAsync(string userId, string username, string token, decimal marketCapAtCall)
    {
        var call = new Call
        {
            UserId = userId,
            Username = username,
            Token = token,
            MarketCapAtCall = marketCapAtCall,
            CurrentMarketCap = marketCapAtCall, // Initially same as call price
            CallTime = DateTime.UtcNow
        };

        await _calls.InsertOneAsync(call);
        _logger.LogInformation($"Created call for {username}: {token} at ${marketCapAtCall:N2}");
        
        return call;
    }

    public async Task<List<Call>> GetCallsByUserAsync(string userId)
    {
        var filter = Builders<Call>.Filter.Eq(c => c.UserId, userId);
        return await _calls.Find(filter).ToListAsync();
    }

    public async Task<List<Call>> GetCallsByTokenAsync(string token)
    {
        var filter = Builders<Call>.Filter.Eq(c => c.Token, token.ToUpper());
        return await _calls.Find(filter).ToListAsync();
    }

    public async Task<Call?> GetLatestCallByUserAndTokenAsync(string userId, string token)
    {
        var filter = Builders<Call>.Filter.And(
            Builders<Call>.Filter.Eq(c => c.UserId, userId),
            Builders<Call>.Filter.Eq(c => c.Token, token.ToUpper())
        );
        
        return await _calls.Find(filter)
            .SortByDescending(c => c.CallTime)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateMarketCapAsync(string callId, decimal currentMarketCap)
    {
        var filter = Builders<Call>.Filter.Eq(c => c.Id, callId);
        var update = Builders<Call>.Update.Set(c => c.CurrentMarketCap, currentMarketCap);
        
        await _calls.UpdateOneAsync(filter, update);
    }

    public async Task<List<Call>> GetAllCallsAsync()
    {
        return await _calls.Find(_ => true).ToListAsync();
    }
}
