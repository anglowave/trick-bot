using Microsoft.Extensions.Logging;
using Trick.Services;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using System.Text.RegularExpressions;

namespace Trick.Events;

public class OnMessageReceived
{
    private readonly ILogger<OnMessageReceived> _logger;
    private readonly DexScreenerService _dexScreenerService;
    private readonly ChatService _chatService;
    
    private readonly Regex _solanaTokenRegex = new Regex(@"\$([A-Za-z0-9]{32,44})", RegexOptions.IgnoreCase);
    private readonly Regex _ethBscTokenRegex = new Regex(@"\$([0x][A-Za-z0-9]{40})", RegexOptions.IgnoreCase);
    private readonly Regex _symbolTokenRegex = new Regex(@"\$([A-Za-z0-9]{2,10})|#([A-Za-z0-9]{2,10})", RegexOptions.IgnoreCase);

    public OnMessageReceived(ILogger<OnMessageReceived> logger, DexScreenerService dexScreenerService, ChatService chatService)
    {
        _logger = logger;
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
            return; 
        }

        var ethBscMatches = _ethBscTokenRegex.Matches(message.Message);
        foreach (Match match in ethBscMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
            return; 
        }

        var symbolMatches = _symbolTokenRegex.Matches(message.Message);
        foreach (Match match in symbolMatches)
        {
            var token = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            
            // Skip if token matches a command name
            if (IsCommandName(token))
            {
                _logger.LogDebug($"Skipping token search for command name: {token}");
                return;
            }
            
            await ProcessSymbolSearchAsync(message, token.ToUpper());
            return;
        }
    }

   private async Task ProcessTokenCallAsync(ChatMessage message, string token)
{
    try
    {
        var tokenInfo = await _dexScreenerService.GetTokenInfoAsync(token);

        if (tokenInfo != null)
        {
            var response = $"🚀 {tokenInfo.BaseToken.Name} ({tokenInfo.BaseToken.Symbol})\n" +
                           $"💰 Price: ${tokenInfo.PriceUsd} | 24H: {FormatPercentage(tokenInfo.PriceChange.H24)} {GetTrendIcon(tokenInfo.PriceChange.H24)}\n" +
                           $"📊 Vol: {FormatNumber(tokenInfo.Volume.H24)} | FDV: {FormatNumber(tokenInfo.Fdv)} | Age: {GetTokenAge(tokenInfo.PairCreatedAt)}\n" +
                           $"🌐 {tokenInfo.DexId} | {tokenInfo.ChainId.ToUpper()} | Updated: {DateTime.UtcNow:HH:mm} UTC\n" +
                           $"🔗 Contract: {token}\n";

            if (tokenInfo.ChainId.ToLower() == "solana")
            {
                response += $"🔗 Axiom: https://axiom.trade/t/{token}/@q9 \n" +
                            $"🔗 GMGN: https://gmgn.ai/sol/token/trick_{token}";
            }
            else if (tokenInfo.ChainId.ToLower() == "bsc")
            {
                response += $"🔗 GMGN: https://gmgn.ai/bsc/token/trick_{token}";
            }
            else if (tokenInfo.ChainId.ToLower() == "ethereum")
            {
                response += $"🔗 GMGN: https://gmgn.ai/eth/token/trick_{token}";
            }

            _chatService.SendMessage(message.Channel, response);
            _logger.LogInformation($"Token info displayed cleanly for {token} requested by {message.Username}");
        }
        else
        {
            _chatService.SendMessage(message.Channel, $"❌ No token found for '{token}'");
            _logger.LogWarning($"No token data found for {token} requested by {message.Username}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error processing token info for {token} by {message.Username}");
    }
}

private async Task ProcessSymbolSearchAsync(ChatMessage message, string symbol)
{
    try
    {
        _logger.LogInformation($"Searching for symbol: {symbol} by user: {message.Username}");
        
        var tokenInfo = await _dexScreenerService.SearchTokenAsync(symbol);

        if (tokenInfo != null)
        {
            var response = $"🚀 {tokenInfo.BaseToken.Name} ({tokenInfo.BaseToken.Symbol})\n" +
                           $"💰 Price: ${tokenInfo.PriceUsd} | 24H: {FormatPercentage(tokenInfo.PriceChange.H24)} {GetTrendIcon(tokenInfo.PriceChange.H24)}\n" +
                           $"📊 Vol: {FormatNumber(tokenInfo.Volume.H24)} | FDV: {FormatNumber(tokenInfo.Fdv)} | Age: {GetTokenAge(tokenInfo.PairCreatedAt)}\n" +
                           $"🌐 {tokenInfo.DexId} | {tokenInfo.ChainId.ToUpper()} | Updated: {DateTime.UtcNow:HH:mm} UTC\n" +
                           $"🔗 Contract: {tokenInfo.BaseToken.Address}\n";

            if (tokenInfo.ChainId.ToLower() == "solana")
            {
                response += $"🔗 Axiom: https://axiom.trade/t/{tokenInfo.BaseToken.Address}/@q9 \n" +
                            $"🔗 GMGN: https://gmgn.ai/sol/token/trick_{tokenInfo.BaseToken.Address}\n" +
                            $"🔗 DexScreener: {tokenInfo.Url}";
            }
            else if (tokenInfo.ChainId.ToLower() == "bsc")
            {
                response += $"🔗 GMGN: https://gmgn.ai/bsc/token/trick_{tokenInfo.BaseToken.Address}\n" +
                            $"🔗 DexScreener: {tokenInfo.Url}";
            }
            else if (tokenInfo.ChainId.ToLower() == "ethereum")
            {
                response += $"🔗 GMGN: https://gmgn.ai/eth/token/trick_{tokenInfo.BaseToken.Address}\n" +
                            $"🔗 DexScreener: {tokenInfo.Url}";
            }

            _chatService.SendMessage(message.Channel, response);
            _logger.LogInformation($"Symbol search completed for {symbol} by user {message.Username}");
        }
        else
        {
            _chatService.SendMessage(message.Channel, $"❌ No token found for '{symbol}'");
            _logger.LogWarning($"No search results found for symbol: {symbol} by user: {message.Username}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error searching for symbol {symbol} by user {message.Username}");
        _chatService.SendMessage(message.Channel, "❌ Error searching for token. Please try again.");
    }
}

    private string FormatNumber(decimal value)
    {
        if (value >= 1000000000)
            return $"${value / 1000000000:F1}B";
        if (value >= 1000000)
            return $"${value / 1000000:F1}M";
        if (value >= 1000)
            return $"${value / 1000:F1}K";
        return $"${value:F2}";
    }

    private string FormatPercentage(decimal percentage)
    {
        var sign = percentage >= 0 ? "+" : "";
        return $"{sign}{percentage:F1}%";
    }

    private string GetTrendIcon(decimal percentage)
    {
        return percentage >= 0 ? "📈" : "📉";
    }

    private string GetTokenAge(long pairCreatedAt)
    {
        var created = DateTimeOffset.FromUnixTimeMilliseconds(pairCreatedAt).DateTime;
        var age = DateTime.UtcNow - created;
        
        if (age.TotalDays >= 365)
            return $"{(int)(age.TotalDays / 365)}y";
        if (age.TotalDays >= 30)
            return $"{(int)(age.TotalDays / 30)}mo";
        if (age.TotalDays >= 7)
            return $"{(int)(age.TotalDays / 7)}w";
        if (age.TotalDays >= 1)
            return $"{(int)age.TotalDays}d";
        if (age.TotalHours >= 1)
            return $"{(int)age.TotalHours}h";
        return $"{(int)age.TotalMinutes}m";
    }

    private bool IsCommandName(string token)
    {
        var commandNames = new[] { "dexpaid", "help", "dexboosts" };
        return commandNames.Contains(token.ToLower());
    }
}
