using Microsoft.Extensions.Logging;
using Trick.Services;
using TwitchLib.Client.Models;
using System.Text.RegularExpressions;

namespace Trick.Services;

public class TokenCallDetector
{
    private readonly ILogger<TokenCallDetector> _logger;
    private readonly CallService _callService;
    
    // Regex pattern to detect token calls (adjust as needed)
    // This example looks for patterns like: $TOKEN, #TOKEN, or TOKEN with $ prefix
    private readonly Regex _tokenCallRegex = new Regex(@"\$([A-Z]{2,10})|#([A-Z]{2,10})", RegexOptions.IgnoreCase);

    public TokenCallDetector(ILogger<TokenCallDetector> logger, CallService callService)
    {
        _logger = logger;
        _callService = callService;
    }

    public async Task ProcessMessageAsync(ChatMessage message)
    {
        var matches = _tokenCallRegex.Matches(message.Message);
        
        foreach (Match match in matches)
        {
            var token = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            token = token.ToUpper();

            // Skip if it's a command (starts with $)
            if (message.Message.Trim().StartsWith("$"))
                continue;

            try
            {
                // TODO: Get real-time market cap from API
                // For now, we'll use a placeholder value
                var marketCapAtCall = await GetCurrentMarketCapAsync(token);
                
                if (marketCapAtCall > 0)
                {
                    await _callService.CreateCallAsync(
                        message.UserId, 
                        message.Username, 
                        token, 
                        marketCapAtCall
                    );
                    
                    _logger.LogInformation($"Token call detected: {message.Username} called {token} at ${marketCapAtCall:N2}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing token call for {token} by {message.Username}");
            }
        }
    }

    private async Task<decimal> GetCurrentMarketCapAsync(string token)
    {
        // TODO: Implement real-time market cap fetching
        // This is a placeholder - you'll need to integrate with a crypto API
        // like CoinGecko, CoinMarketCap, or similar
        
        await Task.Delay(1); // Placeholder async operation
        
        // Return a random value for demonstration
        // In production, replace this with actual API call
        var random = new Random();
        return random.Next(1000000, 100000000); // Random market cap between 1M and 100M
    }
}
