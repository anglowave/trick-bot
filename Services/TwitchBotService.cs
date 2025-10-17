using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Trick.Services;

public class TwitchBotService
{
    private readonly TwitchClient _client;
    private readonly ILogger<TwitchBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly CommandHandler _commandHandler;
    private readonly Trick.Events.OnMessageReceived _messageHandler;
    private readonly ChatService _chatService;

    public TwitchBotService(ILogger<TwitchBotService> logger, IConfiguration configuration, CommandHandler commandHandler, Trick.Events.OnMessageReceived messageHandler, ChatService chatService)
    {
        _logger = logger;
        _configuration = configuration;
        _commandHandler = commandHandler;
        _messageHandler = messageHandler;
        _chatService = chatService;

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        
        // Set the TwitchClient in ChatService to use the same instance
        _chatService.SetTwitchClient(_client);

        SetupEventHandlers();
    }

    public async Task StartAsync()
    {
        var botUsername = _configuration["TwitchBot:BotUsername"];
        var oauthToken = _configuration["TwitchBot:OAuthToken"];
        var channelName = _configuration["TwitchBot:ChannelName"];

        if (string.IsNullOrEmpty(botUsername) || string.IsNullOrEmpty(oauthToken) || string.IsNullOrEmpty(channelName))
        {
            _logger.LogError("Missing Twitch bot configuration. Please check appsettings.json");
            return;
        }

        var credentials = new ConnectionCredentials(botUsername, oauthToken);
        _client.Initialize(credentials, channelName);

        _logger.LogInformation("Starting Twitch bot...");
        _client.Connect();

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping Twitch bot...");
        _client.Disconnect();
        await Task.CompletedTask;
    }

    private void SetupEventHandlers()
    {
        _client.OnConnected += OnConnected;
        _client.OnJoinedChannel += OnJoinedChannel;
        _client.OnMessageReceived += OnMessageReceived;
        _client.OnUserJoined += OnUserJoined;
        _client.OnRaidNotification += OnRaidNotification;
        _client.OnError += (sender, e) => _logger.LogError($"Twitch client error: {e.Exception?.Message}");
    }

    private void OnConnected(object? sender, OnConnectedArgs e)
    {
        _logger.LogInformation($"Connected to Twitch as {e.BotUsername}");
    }

    private void OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _logger.LogInformation($"Joined channel: {e.Channel}");
        _client.SendMessage(e.Channel, "Bot is now online!");
    }

    private async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        _logger.LogInformation($"{e.ChatMessage.Username}: {e.ChatMessage.Message}");
        
        // Handle commands
        await _commandHandler.HandleCommand(e.ChatMessage);
        
        // Handle token detection and other message processing
        await _messageHandler.Handle(sender, e);
    }

    private void OnUserJoined(object? sender, OnUserJoinedArgs e)
    {
        _logger.LogInformation($"User joined: {e.Username}");
    }

    private void OnRaidNotification(object? sender, OnRaidNotificationArgs e)
    {
        _logger.LogInformation($"Raid from {e.RaidNotification.MsgParamDisplayName} with {e.RaidNotification.MsgParamViewerCount} viewers!");
    }
}
