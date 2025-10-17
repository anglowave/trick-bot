using Microsoft.Extensions.Logging;
using Trick.Services;
using TwitchLib.Client.Models;

namespace Trick.Commands;

public class PnlCommand : ICommand
{
    private readonly ILogger<PnlCommand> _logger;
    private readonly CallService _callService;

    public string Name => "pnl";
    public string Description => "Shows PnL for a specific token call";

    public PnlCommand(ILogger<PnlCommand> logger, CallService callService)
    {
        _logger = logger;
        _callService = callService;
    }

    public void Execute(ChatMessage message, string[] args)
    {
        if (args.Length == 0)
        {
            var response = "Usage: $pnl <token> - Shows PnL for your latest call of that token";
            Console.WriteLine($"Bot: {response}");
            return;
        }

        var token = args[0].ToUpper();
        var userId = message.UserId;

        try
        {
            var call = _callService.GetLatestCallByUserAndTokenAsync(userId, token).Result;
            
            if (call == null)
            {
                var response = $"No call found for token {token}";
                Console.WriteLine($"Bot: {response}");
                return;
            }

            // TODO: Update current market cap with real-time data
            // For now, we'll use the stored value
            var pnl = call.PnL;
            var pnlPercentage = call.PnLPercentage;
            var callTime = call.CallTime.ToString("MM/dd HH:mm");
            
            var response = $"{message.Username}'s {token} call: " +
                          $"Called at ${call.MarketCapAtCall:N2} ({callTime}) | " +
                          $"Current: ${call.CurrentMarketCap:N2} | " +
                          $"PnL: ${pnl:N2} ({pnlPercentage:+#.##;-#.##;0}%)";
            
            Console.WriteLine($"Bot: {response}");
            _logger.LogInformation($"PnL command executed for {message.Username} - Token: {token}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing PnL command for {message.Username}");
            Console.WriteLine($"Bot: Error retrieving PnL data for {token}");
        }
    }
}
