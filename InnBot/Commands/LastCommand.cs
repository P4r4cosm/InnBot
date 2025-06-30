// InnBot/Commands/LastCommand.cs
using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class LastCommand : ICommand
{
    public string Name => "/last";
    
    private readonly IUserHistoryService _historyService;
    private readonly ILogger<LastCommand> _logger;

    public LastCommand(IUserHistoryService historyService, ILogger<LastCommand> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;
        
        // Получаем последнюю команду из истории
        var (lastCommand, lastUpdate) = _historyService.GetLastCommand(chatId);

        if (lastCommand != null && lastUpdate != null)
        {
            _logger.LogInformation("Повторяем команду '{CommandName}' для чата {ChatId}", lastCommand.Name, chatId);
            // Повторно выполняем найденную команду с ее оригинальными данными
            await lastCommand.ExecuteAsync(lastUpdate, botClient);
        }
        else
        {
            _logger.LogWarning("История команд для чата {ChatId} пуста. Нечего повторять.", chatId);
            await botClient.SendMessage(
                chatId: chatId,
                text: "Я не помню вашу последнюю команду. Попробуйте выполнить что-нибудь сначала."
            );
        }
    }
}