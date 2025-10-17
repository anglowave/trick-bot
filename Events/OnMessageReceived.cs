using Microsoft.Extensions.Logging;
using TwitchLib.Client.Events;

namespace Trick.Events;

public class OnMessageReceived
{
    private readonly ILogger<OnMessageReceived> _logger;

    public OnMessageReceived(ILogger<OnMessageReceived> logger)
    {
        _logger = logger;
    }

    public void Handle(object? sender, OnMessageReceivedArgs e)
    {
        var message = e.ChatMessage;
        
        _logger.LogInformation($"Message from {message.Username}: {message.Message}");
        
        // Add custom message handling logic here
        // For example: logging, analytics, custom responses, etc.
        
        // Example: Log first-time chatters
        if (message.IsFirstMessage)
        {
            _logger.LogInformation($"First message from {message.Username}!");
        }
        
        // Example: Handle subscriber messages
        if (message.IsSubscriber)
        {
            _logger.LogInformation($"Subscriber message from {message.Username}");
        }
    }
}
