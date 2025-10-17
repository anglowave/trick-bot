using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trick.Commands;
using Trick.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add logging
builder.Logging.AddConsole();

// Register services
builder.Services.AddSingleton<TwitchBotService>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<CallService>();
builder.Services.AddSingleton<TokenCallDetector>();

// Register commands
builder.Services.AddTransient<ICommand, HelloCommand>();
builder.Services.AddTransient<ICommand, PingCommand>();
builder.Services.AddTransient<ICommand, PnlCommand>();

// Register event handlers
builder.Services.AddTransient<Trick.Events.OnMessageReceived>();
builder.Services.AddTransient<Trick.Events.OnUserJoined>();
builder.Services.AddTransient<Trick.Events.OnRaid>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var twitchBotService = host.Services.GetRequiredService<TwitchBotService>();

logger.LogInformation("Starting Trick Twitch Bot...");

try
{
    await twitchBotService.StartAsync();
    logger.LogInformation("Twitch Bot started successfully!");
    
    // Keep the application running
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while starting the Twitch Bot");
}
finally
{
    await twitchBotService.StopAsync();
    logger.LogInformation("Twitch Bot stopped.");
}
