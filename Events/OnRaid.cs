using Microsoft.Extensions.Logging;
using TwitchLib.Client.Events;

namespace Trick.Events;

public class OnRaid
{
    private readonly ILogger<OnRaid> _logger;

    public OnRaid(ILogger<OnRaid> logger)
    {
        _logger = logger;
    }

    public void Handle(object? sender, OnRaidNotificationArgs e)
    {
        var raidInfo = e.RaidNotification;
        
        _logger.LogInformation($"Raid from {raidInfo.MsgParamDisplayName} with {raidInfo.MsgParamViewerCount} viewers!");
        
        // Add custom raid handling logic here
        // For example: raid alerts, special responses, etc.
        
        // Example: Send thank you message
        var viewerCountStr = raidInfo.MsgParamViewerCount;
        var raiderName = raidInfo.MsgParamDisplayName;
        
        if (int.TryParse(viewerCountStr, out var viewerCount) && viewerCount > 0)
        {
            _logger.LogInformation($"Thanking {raiderName} for raiding with {viewerCount} viewers!");
            
            // In a real implementation, you would send this message to chat
            // This would typically be done through the TwitchBotService
            Console.WriteLine($"Bot: Thank you {raiderName} for raiding with {viewerCountStr} viewers! Welcome everyone!");
        }
        
        // Example: Track raid statistics
        // TrackRaidStatistics(raiderName, viewerCount);
    }
}
