using Microsoft.Extensions.Logging;
using TwitchLib.Client;

namespace Trick.Services;

public class ChatService
{
    private TwitchClient? _twitchClient;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ILogger<ChatService> logger)
    {
        _logger = logger;
    }

    public void SetTwitchClient(TwitchClient twitchClient)
    {
        _twitchClient = twitchClient;
    }

    public void SendMessage(string channel, string message)
    {
        try
        {
            if (_twitchClient == null)
            {
                _logger.LogError("TwitchClient not initialized in ChatService");
                return;
            }

            _twitchClient.SendMessage(channel, message);
            _logger.LogDebug($"Sent message to {channel}: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send message to {channel}");
        }
    }
}
