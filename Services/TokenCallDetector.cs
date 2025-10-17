using Microsoft.Extensions.Logging;
using Trick.Services;
using TwitchLib.Client.Models;
using System.Text.RegularExpressions;

namespace Trick.Services;

public class TokenCallDetector
{
    private readonly ILogger<TokenCallDetector> _logger;
    private readonly CallService _callService;
    private readonly DexScreenerService _dexScreenerService;
    
    // Regex patterns to detect different token formats
    private readonly Regex _solanaTokenRegex = new Regex(@"\$([A-Za-z0-9]{32,44})", RegexOptions.IgnoreCase);
    private readonly Regex _ethBscTokenRegex = new Regex(@"\$([0x][A-Za-z0-9]{40})", RegexOptions.IgnoreCase);
    private readonly Regex _symbolTokenRegex = new Regex(@"\$([A-Z]{2,10})|#([A-Z]{2,10})", RegexOptions.IgnoreCase);

    public TokenCallDetector(ILogger<TokenCallDetector> logger, CallService callService, DexScreenerService dexScreenerService)
    {
        _logger = logger;
        _callService = callService;
        _dexScreenerService = dexScreenerService;
    }

    public async Task ProcessMessageAsync(ChatMessage message)
    {
        // Skip if it's a command (starts with $)
        if (message.Message.Trim().StartsWith("$"))
            return;

        // Check for Solana token addresses (32-44 characters)
        var solanaMatches = _solanaTokenRegex.Matches(message.Message);
        foreach (Match match in solanaMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
        }

        // Check for Ethereum/BSC token addresses (0x + 40 hex characters)
        var ethBscMatches = _ethBscTokenRegex.Matches(message.Message);
        foreach (Match match in ethBscMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
        }

        // Check for symbol-based tokens (2-10 characters)
        var symbolMatches = _symbolTokenRegex.Matches(message.Message);
        foreach (Match match in symbolMatches)
        {
            var token = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            await ProcessTokenCallAsync(message, token.ToUpper());
        }
    }

    private async Task ProcessTokenCallAsync(ChatMessage message, string token)
    {
        try
        {
            var marketCapAtCall = await _dexScreenerService.GetMarketCapAsync(token);
            
            if (marketCapAtCall.HasValue && marketCapAtCall.Value > 0)
            {
                await _callService.CreateCallAsync(
                    message.UserId, 
                    message.Username, 
                    token, 
                    marketCapAtCall.Value
                );
                
                _logger.LogInformation($"Token call detected: {message.Username} called {token} at ${marketCapAtCall.Value:N2}");
            }
            else
            {
                _logger.LogWarning($"No market cap data found for token {token} called by {message.Username}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing token call for {token} by {message.Username}");
        }
    }

}
