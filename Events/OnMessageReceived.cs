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
    private readonly Regex _symbolTokenRegex = new Regex(@"\$([A-Z]{2,10})|#([A-Z]{2,10})", RegexOptions.IgnoreCase);

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

        // Check for Solana token addresses (32-44 characters)
        var solanaMatches = _solanaTokenRegex.Matches(message.Message);
        foreach (Match match in solanaMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
            return; // Exit after processing first token found
        }

        // Check for Ethereum/BSC token addresses (0x + 40 hex characters)
        var ethBscMatches = _ethBscTokenRegex.Matches(message.Message);
        foreach (Match match in ethBscMatches)
        {
            var tokenAddress = match.Groups[1].Value;
            await ProcessTokenCallAsync(message, tokenAddress);
            return; // Exit after processing first token found
        }

        // Check for symbol-based tokens (2-10 characters)
        var symbolMatches = _symbolTokenRegex.Matches(message.Message);
        foreach (Match match in symbolMatches)
        {
            var token = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            await ProcessTokenCallAsync(message, token.ToUpper());
            return; // Exit after processing first token found
        }
    }

    private async Task ProcessTokenCallAsync(ChatMessage message, string token)
    {
        try
        {
            var tokenInfo = await _dexScreenerService.GetTokenInfoAsync(token);
            
            if (tokenInfo != null)
            {
                var response = $"🚀 {tokenInfo.BaseToken.Name} [{FormatNumber(tokenInfo.MarketCap)}/{FormatPercentage(tokenInfo.PriceChange.H24)}] " +
                              $"{tokenInfo.BaseToken.Symbol}/{tokenInfo.QuoteToken.Symbol} {GetTrendIcon(tokenInfo.PriceChange.H24)} " +
                              $"{tokenInfo.ChainId.ToUpper()} @ {tokenInfo.DexId} " +
                              $"💰 USD: ${tokenInfo.PriceUsd} " +
                              $"💎 FDV: {FormatNumber(tokenInfo.Fdv)} " +
                              $"📊 Vol: {FormatNumber(tokenInfo.Volume.H24)} " +
                              $"⏰ Age: {GetTokenAge(tokenInfo.PairCreatedAt)} " +
                              $"📈 24H: {FormatPercentage(tokenInfo.PriceChange.H24)} " +
                              $"📈 1H: {FormatPercentage(tokenInfo.PriceChange.H1)} " +
                              $"🔗 Contract: {token} " +
                              $"🗓️ Updated: {DateTime.UtcNow:HH:mm} UTC";

                if (tokenInfo.ChainId.ToLower() == "solana")
                    response += $" axiom: https://axiom.trade/meme/{tokenInfo.PairAddress} | gmgn: https://gmgn.ai/sol/token/{token}";
                else if(tokenInfo.ChainId.ToLower() == "bsc")
                    response += $" gmgn: https://gmgn.ai/bsc/token/{token}";
                else if(tokenInfo.ChainId.ToLower() == "ethereum")
                    response += $" gmgn: https://gmgn.ai/eth/token/{token}";

                _chatService.SendMessage(message.Channel, response);
                _logger.LogInformation($"Token info displayed: {message.Username} requested {token} info");
            }
            else
            {
                _logger.LogWarning($"No token data found for {token} requested by {message.Username}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing token info for {token} by {message.Username}");
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
}
