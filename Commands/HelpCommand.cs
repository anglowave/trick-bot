
using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;
using Trick.Services;

namespace Trick.Commands;

public class HelpCommand : ICommand
{
    private readonly ILogger<HelpCommand> _logger;
    private readonly ChatService _chatService;

    public HelpCommand(ILogger<HelpCommand> logger, ChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }

    public string Name => "help";
    public string Description => "Shows a list of commands and their descriptions";
    
    public Task Execute(ChatMessage message, string[] args)
    {
        _logger.LogInformation($"Help command executed by user: {message.Username}");
        
        var helpMessage = "🟣 Beep boop, I'm your friendly neighborhood bot (probably Rick's cousin).\n\n" +
                         "💬 Commands I actually understand:\n" +
                         "• %dexpaid <token> — find out if someone paid the DEX tax 😤\n" +
                         "• %dexboosts — shows recently boosted tokens 🚀\n" +
                         "• %help — because even bots need therapy\n" +
                         "• %<token> — get token info faster than your ex ghosted you\n\n" +
                         "Powered by caffeine, code, and mild confusion," +
                         "btw: we have no affiliation with the real Rick.";
        
        _chatService.SendMessage(message.Channel, helpMessage);
        return Task.CompletedTask;
    }
}