using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trick.Commands;
using Trick.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Logging.AddConsole();

builder.Services.AddSingleton<TwitchBotService>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<CallService>();
builder.Services.AddSingleton<DexScreenerService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddHttpClient<DexScreenerService>();

// Register commands
builder.Services.AddTransient<ICommand, DexPaidCommand>();

builder.Services.AddTransient<Trick.Events.OnMessageReceived>();
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var twitchBotService = host.Services.GetRequiredService<TwitchBotService>();

logger.LogInformation("Starting Trick Twitch Bot...");

try
{
    await twitchBotService.StartAsync();
    logger.LogInformation("Twitch Bot started successfully!");
    
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
