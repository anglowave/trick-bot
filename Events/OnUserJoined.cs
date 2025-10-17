using Microsoft.Extensions.Logging;
using TwitchLib.Client.Events;

namespace Trick.Events;

public class OnUserJoined
{
    private readonly ILogger<OnUserJoined> _logger;

    public OnUserJoined(ILogger<OnUserJoined> logger)
    {
        _logger = logger;
    }

    public void Handle(object? sender, OnUserJoinedArgs e)
    {
        _logger.LogInformation($"User joined: {e.Username}");
        
        // Add custom user join handling logic here
        // For example: welcome messages, user tracking, etc.
        
        // Example: Track active viewers
        // You could maintain a list of active users here
        
        // Example: Send welcome message for new followers
        // if (IsNewFollower(e.Username))
        // {
        //     SendWelcomeMessage(e.Username);
        // }
    }
}
