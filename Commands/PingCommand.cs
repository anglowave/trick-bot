using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;

namespace Trick.Commands;

public class PingCommand : ICommand
{
    private readonly ILogger<PingCommand> _logger;

    public string Name => "ping";
    public string Description => "Responds with pong";

    public PingCommand(ILogger<PingCommand> logger)
    {
        _logger = logger;
    }

    public async Task Execute(ChatMessage message, string[] args)
    {
        var response = "Pong!";
        _logger.LogInformation($"Ping command executed for {message.Username}");
        
        // In a real implementation, you would send this message back to chat
        // This would typically be done through the TwitchBotService
        Console.WriteLine($"Bot: {response}");
        await Task.CompletedTask;
    }
}
