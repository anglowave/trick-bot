using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trick.Commands;
using TwitchLib.Client.Models;

namespace Trick.Services;

public class CommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly Dictionary<string, ICommand> _commands;
    private readonly string _commandPrefix;

    public CommandHandler(ILogger<CommandHandler> logger, IEnumerable<ICommand> commands, IConfiguration configuration)
    {
        _logger = logger;
        _commandPrefix = configuration["TwitchBot:CommandPrefix"] ?? "!";
        
        _commands = commands.ToDictionary(cmd => cmd.Name.ToLower(), cmd => cmd);
        _logger.LogInformation($"Loaded {_commands.Count} commands");
    }

    public async Task HandleCommand(ChatMessage message)
    {
        if (!message.Message.StartsWith(_commandPrefix))
            return;

        var parts = message.Message.Substring(_commandPrefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var command))
        {
            try
            {
                await command.Execute(message, args);
                _logger.LogInformation($"Executed command '{commandName}' for user '{message.Username}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing command '{commandName}' for user '{message.Username}'");
            }
        }
    }
}
