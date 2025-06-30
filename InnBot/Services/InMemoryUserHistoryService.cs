using System.Collections.Concurrent;
using InnBot.Commands;
using Telegram.Bot.Types;

namespace InnBot.Services;

public class InMemoryUserHistoryService : IUserHistoryService
{
    // Потокобезопасный словарь для хранения данных в формате: { Id чата -> (Команда, Обновление) }
    private readonly ConcurrentDictionary<long, (ICommand Command, Update Update)> _history = new();

    public (ICommand? Command, Update? Update) GetLastCommand(long chatId)
    {
        _history.TryGetValue(chatId, out var lastAction);
        return lastAction;
    }

    public void SetLastCommand(long chatId, ICommand command, Update update)
    {
        // Не сохраняем "бесполезные" для повторения команды
        var commandNameToIgnore = new[] {  "/last" };
        if (commandNameToIgnore.Contains(command.Name.ToLower()))
        {
            return;
        }

        _history[chatId] = (command, update);
    }
}