using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;

namespace Trick.Commands;

public class HelloCommand : ICommand
{
    private readonly ILogger<HelloCommand> _logger;

    public string Name => "hello";
    public string Description => "Says hello to the user";

    public HelloCommand(ILogger<HelloCommand> logger)
    {
        _logger = logger;
    }

    public void Execute(ChatMessage message, string[] args)
    {
        var response = $"Hello {message.Username}! Welcome to the stream!";
        _logger.LogInformation($"Hello command executed for {message.Username}");
        
        // In a real implementation, you would send this message back to chat
        // This would typically be done through the TwitchBotService
        Console.WriteLine($"Bot: {response}");
    }
}
