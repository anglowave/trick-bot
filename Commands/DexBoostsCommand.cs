using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;
using Trick.Services;
using Trick.Models;

namespace Trick.Commands;

public class DexBoostsCommand : ICommand
{
    private readonly ILogger<DexBoostsCommand> _logger;
    private readonly DexScreenerService _dexScreenerService;
    private readonly ChatService _chatService;

    public DexBoostsCommand(ILogger<DexBoostsCommand> logger, DexScreenerService dexScreenerService, ChatService chatService)
    {
        _logger = logger;
        _dexScreenerService = dexScreenerService;
        _chatService = chatService;
    }

    public string Name => "dexboosts";

    public string Description => "Shows recently boosted tokens from DexScreener.";

    public async Task Execute(ChatMessage message, string[] args)
    {
        _logger.LogInformation($"Executing dexboosts command for user: {message.Username}");

        try
        {
            var boosts = await _dexScreenerService.GetTokenBoostsAsync();
            _logger.LogInformation($"Retrieved {boosts.Count} boosted tokens from API");
            
            if (!boosts.Any())
            {
                _chatService.SendMessage(message.Channel, "❌ No boosted tokens found at the moment.");
                return;
            }

            // Take the first 10 boosted tokens
            var topBoosts = boosts.Take(10).ToList();
            
            var response = "🚀 Top 10 Recently Boosted Tokens:\n";
            
            for (int i = 0; i < topBoosts.Count; i++)
            {
                var boost = topBoosts[i];
                var tokenName = GetTokenName(boost);
                var chainEmoji = GetChainEmoji(boost.ChainId);
                
                // Highlight top 3 with special formatting
                if (i < 3)
                {
                    response += $"🥇 {chainEmoji} {tokenName} ({boost.TotalAmount} boosts) | {boost.ChainId.ToUpper()}\n";
                }
                else
                {
                    response += $"   {chainEmoji} {tokenName} ({boost.TotalAmount} boosts) | {boost.ChainId.ToUpper()}\n";
                }
            }

            _logger.LogInformation($"Sending response message: {response}");
            _chatService.SendMessage(message.Channel, response);
            _logger.LogInformation($"Successfully displayed {topBoosts.Count} boosted tokens");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dexboosts command");
            _chatService.SendMessage(message.Channel, "❌ Error fetching boosted tokens. Please try again.");
        }
    }

    private string GetTokenName(TokenBoost boost)
    {
        // Try to extract a meaningful name from the description
        if (!string.IsNullOrEmpty(boost.Description))
        {
            // Look for common patterns in descriptions
            var description = boost.Description;
            
            // Check for patterns like "GOLDCOIN ON BNB", "Silvercoin", etc.
            if (description.Contains("GOLDCOIN", StringComparison.OrdinalIgnoreCase))
                return "GOLDCOIN";
            if (description.Contains("Silvercoin", StringComparison.OrdinalIgnoreCase))
                return "Silvercoin";
            if (description.Contains("electrum", StringComparison.OrdinalIgnoreCase))
                return "Electrum";
            if (description.Contains("MadDegen", StringComparison.OrdinalIgnoreCase))
                return "MadDegen";
            if (description.Contains("MOG", StringComparison.OrdinalIgnoreCase))
                return "Based MOG";
            if (description.Contains("USDARK", StringComparison.OrdinalIgnoreCase))
                return "USDARK";
            if (description.Contains("Loki", StringComparison.OrdinalIgnoreCase))
                return "Loki";
            if (description.Contains("Blocky Boy", StringComparison.OrdinalIgnoreCase))
                return "Blocky Boy";
            if (description.Contains("GUARDBAG", StringComparison.OrdinalIgnoreCase))
                return "GUARDBAG";
            if (description.Contains("Futardio", StringComparison.OrdinalIgnoreCase))
                return "Futardio";
            if (description.Contains("Respawn", StringComparison.OrdinalIgnoreCase))
                return "Respawn";
            if (description.Contains("LOL", StringComparison.OrdinalIgnoreCase))
                return "LOL";
            if (description.Contains("PXP", StringComparison.OrdinalIgnoreCase))
                return "PXP";
            if (description.Contains("SILVER", StringComparison.OrdinalIgnoreCase))
                return "SILVER";
            if (description.Contains("GOLD", StringComparison.OrdinalIgnoreCase))
                return "GOLD";
            if (description.Contains("poop", StringComparison.OrdinalIgnoreCase))
                return "Poop";
            if (description.Contains("piss", StringComparison.OrdinalIgnoreCase))
                return "Piss";
            if (description.Contains("Yeti", StringComparison.OrdinalIgnoreCase))
                return "Yeti";
            if (description.Contains("American Eagle", StringComparison.OrdinalIgnoreCase))
                return "American Eagle";
            if (description.Contains("jeans", StringComparison.OrdinalIgnoreCase))
                return "Jeans";
            if (description.Contains("wall", StringComparison.OrdinalIgnoreCase))
                return "Wall";
            if (description.Contains("fluid", StringComparison.OrdinalIgnoreCase))
                return "Fluid";
            if (description.Contains("mumu", StringComparison.OrdinalIgnoreCase))
                return "Mumu";
            if (description.Contains("bull", StringComparison.OrdinalIgnoreCase))
                return "Bull";
            if (description.Contains("panda", StringComparison.OrdinalIgnoreCase))
                return "Panda";
            if (description.Contains("gold", StringComparison.OrdinalIgnoreCase))
                return "Gold";
            if (description.Contains("silver", StringComparison.OrdinalIgnoreCase))
                return "Silver";
            if (description.Contains("coin", StringComparison.OrdinalIgnoreCase))
                return "Coin";
            
            // If no specific pattern found, try to get the first meaningful word
            var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                var firstWord = words[0].Trim('$', '#', '!', '?', '.', ',', ';', ':');
                if (firstWord.Length > 1 && firstWord.Length < 20)
                    return firstWord;
            }
        }

        // Fallback to using part of the token address
        return $"Token {boost.TokenAddress[..8]}...";
    }

    private string GetChainEmoji(string chainId)
    {
        return chainId.ToLower() switch
        {
            "solana" => "☀️",
            "ethereum" => "🔷",
            "bsc" => "🟡",
            "base" => "🔵",
            "arbitrum" => "🔴",
            "polygon" => "🟣",
            "avalanche" => "🔺",
            _ => "⛓️"
        };
    }
}
