// InnBot/Services/InMemoryUserHistoryService.cs
using System.Collections.Concurrent;
using InnBot.Commands;
using Telegram.Bot.Types;

namespace InnBot.Services;

public class InMemoryUserHistoryService : IUserHistoryService
{
    private readonly ConcurrentDictionary<long, (ICommand Command, Update Update)> _history = new();
    private readonly ILogger<InMemoryUserHistoryService> _logger;
    
    // Список команд, которые не нужно сохранять для повторения
    private static readonly string[] CommandsToIgnore = { "/last", "/start", "/help" };

    public InMemoryUserHistoryService(ILogger<InMemoryUserHistoryService> logger)
    {
        _logger = logger;
    }

    public (ICommand? Command, Update? Update) GetLastCommand(long chatId)
    {
        _history.TryGetValue(chatId, out var lastAction);
        return lastAction;
    }

    public void SetLastCommand(long chatId, ICommand command, Update update)
    {
        // Не сохраняем "бесполезные" для повторения команды
        if (CommandsToIgnore.Contains(command.Name.ToLower()))
        {
            _logger.LogInformation("Команда {CommandName} не будет сохранена в историю для чата {ChatId}", command.Name, chatId);
            return;
        }

        _history[chatId] = (command, update);
        _logger.LogInformation("Команда {CommandName} сохранена в историю для чата {ChatId}", command.Name, chatId);
    }
}