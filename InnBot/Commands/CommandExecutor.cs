using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class CommandExecutor
{
    private readonly IEnumerable<ICommand> _commands;

    // DI передаст сюда список всех зарегистрированных ICommand
    public CommandExecutor(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public async Task ExecuteCommandAsync(string commandName, Update update, ITelegramBotClient botClient)
    {
        var command = _commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command != null)
        {
            await command.ExecuteAsync(update, botClient);
        }
        else
        {
            // Команда не найдена
            await botClient.SendMessage(update.Message.Chat.Id, "Неизвестная команда. Используйте /help для списка команд.");
        }
    }
}