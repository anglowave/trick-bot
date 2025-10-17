using Microsoft.Extensions.Logging;
using Trick.Services;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using System.Text.RegularExpressions;

namespace Trick.Events;

public class OnMessageReceived
{
    private readonly ILogger<OnMessageReceived> _logger;
    private readonly CallService _callService;
    private readonly DexScreenerService _dexScreenerService;
    private readonly ChatService _chatService;
    
    private readonly Regex _solanaTokenRegex = new Regex(@"\$([A-Za-z0-9]{32,44})", RegexOptions.IgnoreCase);
    private readonly Regex _ethBscTokenRegex = new Regex(@"\$([0x][A-Za-z0-9]{40})", RegexOptions.IgnoreCase);
    private readonly Regex _symbolTokenRegex = new Regex(@"\$([A-Z]{2,10})|#([A-Z]{2,10})", RegexOptions.IgnoreCase);

    public OnMessageReceived(ILogger<OnMessageReceived> logger, CallService callService, DexScreenerService dexScreenerService, ChatService chatService)
    {
        _logger = logger;
        _callService = callService;
        _dexScreenerService = dexScreenerService;
        _chatService = chatService;
    }

    public async Task Handle(object? sender, OnMessageReceivedArgs e)
    {        
        var message = e.ChatMessage;
        
        _logger.LogInformation($"Message from {message.Username}: {message.Message}");

        var solanaMatches = _solanaTokenRegex.Matches(message.Message);
        foreach (Match match in solanaMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
        }

        var ethBscMatches = _ethBscTokenRegex.Matches(message.Message);
        foreach (Match match in ethBscMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
        }

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
            var tokenInfo = await _dexScreenerService.GetTokenInfoAsync(token);
            
            if (tokenInfo != null)
            {
                await _callService.CreateCallAsync(
                    message.UserId, 
                    message.Username, 
                    token, 
                    tokenInfo.MarketCap
                );
                
                var response = $"🚀 {message.Username} called {tokenInfo.BaseToken.Name} ({tokenInfo.BaseToken.Symbol}) | " +
                              $"💰 MC: ${tokenInfo.MarketCap:N0} | " +
                              $"💧 Liquidity: ${tokenInfo.Liquidity.Usd:N0} | " +
                              $"📊 24h Vol: ${tokenInfo.Volume.H24:N0} | " +
                              $"📈 24h: {tokenInfo.PriceChange.H24:+#.##;-#.##;0}% | " +
                              $"🔗 {tokenInfo.ChainId.ToUpper()}";
                
                _chatService.SendMessage(message.Channel, response);
                _logger.LogInformation($"Token call detected: {message.Username} called {token} at ${tokenInfo.MarketCap:N2}");
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
