using TwitchLib.Client.Models;

namespace Trick.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    void Execute(ChatMessage message, string[] args);
}
