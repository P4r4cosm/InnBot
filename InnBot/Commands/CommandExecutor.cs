using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class CommandExecutor
{
    private readonly IEnumerable<ICommand> _commands;
    private readonly IUserHistoryService _historyService; // Добавляем зависимость
    private readonly ILogger<CommandExecutor> _logger;

    // DI передаст сюда список всех зарегистрированных ICommand
    public CommandExecutor(IEnumerable<ICommand> commands, IUserHistoryService historyService, ILogger<CommandExecutor> logger)
    {
        _commands = commands;
        _historyService = historyService;
        _logger = logger;
    }

    public async Task ExecuteCommandAsync(string commandName, Update update, ITelegramBotClient botClient)
    {
        var command = _commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command != null)
        {
            try
            {
                await command.ExecuteAsync(update, botClient);
                
                // После успешного выполнения сохраняем команду в историю
                var chatId = update.Message!.Chat.Id;
                _historyService.SetLastCommand(chatId, command, update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении команды {CommandName}", commandName);
                await botClient.SendMessage(update.Message!.Chat.Id, "Произошла ошибка при выполнении команды.");
            }
        }
        else
        {
            await botClient.SendMessage(update.Message!.Chat.Id, "Неизвестная команда. Используйте /help для списка команд.");
        }
    }
}