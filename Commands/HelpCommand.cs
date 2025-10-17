
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
        
        var helpMessage = "Available commands:\n" +
                         "$dexpaid <token_address_or_symbol> - Check if dex was paid on a token\n" +
                         "$help - Show this help message\n"+
                         "$<token> - Get token info";
        
        _chatService.SendMessage(message.Channel, helpMessage);
        return Task.CompletedTask;
    }
}