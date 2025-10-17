using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;
using Trick.Services;

namespace Trick.Commands;

public class DexPaidCommand : ICommand
{
    private readonly ILogger<DexPaidCommand> _logger;
    private readonly DexScreenerService _dexScreenerService;
    private readonly ChatService _chatService;

    public DexPaidCommand(ILogger<DexPaidCommand> logger, DexScreenerService dexScreenerService, ChatService chatService)
    {
        _logger = logger;
        _dexScreenerService = dexScreenerService;
        _chatService = chatService;
    }

    public string Name => "dexpaid";

    public string Description => "Shows if dex was paid on a token.";

    public async Task Execute(ChatMessage message, string[] args)
    {
        if (args.Length == 0)
        {
            _chatService.SendMessage(message.Channel, "Usage: !dexpaid <token_address_or_symbol>");
            return;
        }

        var token = args[0];
        _logger.LogInformation($"Checking payment status for token: {token}");

        try
        {
            var isPaid = await _dexScreenerService.DexPaid(token);
            var status = isPaid ? "✅ PAID" : "❌ NOT PAID";
            var response = $"🔍 DexScreener Payment Status for {token}: {status}";
            
            _chatService.SendMessage(message.Channel, response);
            _logger.LogInformation($"Payment status check completed for {token}: {(isPaid ? "PAID" : "NOT PAID")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing dexpaid command for token: {token}");
            _chatService.SendMessage(message.Channel, "❌ Error checking payment status. Please try again.");
        }
    }
}
