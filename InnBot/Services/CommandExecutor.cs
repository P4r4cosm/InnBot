// InnBot/Commands/CommandExecutor.cs
using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class CommandExecutor
{
    private readonly IEnumerable<ICommand> _commands;
    private readonly IUserHistoryService _historyService;
    private readonly ILogger<CommandExecutor> _logger;

    public CommandExecutor(IEnumerable<ICommand> commands, IUserHistoryService historyService, ILogger<CommandExecutor> logger)
    {
        _commands = commands;
        _historyService = historyService;
        _logger = logger;
    }

    public async Task ExecuteCommandAsync(string commandName, Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;
        var command = _commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command != null)
        {
            _logger.LogInformation("Выполнение команды '{CommandName}' для чата {ChatId}", command.Name, chatId);
            try
            {
                await command.ExecuteAsync(update, botClient);
                
                // После успешного выполнения сохраняем команду в историю
                _historyService.SetLastCommand(chatId, command, update);
            }
            catch (Exception ex)
            {
                // Логируем с указанием контекста
                _logger.LogError(ex, "Ошибка при выполнении команды {CommandName} для чата {ChatId}", commandName, chatId);
                // Отправляем пользователю более общее, но понятное сообщение
                await botClient.SendMessage(chatId, "Произошла внутренняя ошибка при выполнении команды. Попробуйте позже.");
            }
        }
        else
        {
            _logger.LogWarning("Получена неизвестная команда '{CommandName}' от чата {ChatId}", commandName, chatId);
            await botClient.SendMessage(chatId, "Неизвестная команда. Используйте /help для просмотра списка доступных команд.");
        }
    }
}