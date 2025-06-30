// InnBot/Services/UpdateHandlerService.cs
using InnBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnBot.Services;

public class UpdateHandlerService
{
    private readonly CommandExecutor _commandExecutor;
    private readonly ILogger<UpdateHandlerService> _logger;

    public UpdateHandlerService(CommandExecutor commandExecutor, ILogger<UpdateHandlerService> logger)
    {
        _commandExecutor = commandExecutor;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Используем современный switch для обработки разных типов обновлений
        var handler = update.Type switch
        {
            UpdateType.Message => HandleMessageAsync(botClient, update.Message!),
            // Здесь можно добавить обработчики для других типов (CallbackQuery, InlineQuery и т.д.)
            _ => HandleUnknownUpdateAsync(update)
        };

        await handler;
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
    {
        // Проверяем, что в сообщении есть текст
        if (message.Text is not { } messageText)
        {
            _logger.LogInformation("Получено сообщение без текста в чате {ChatId}", message.Chat.Id);
            return;
        }

        var chatId = message.Chat.Id;
        var userId = message.From?.Id;

        _logger.LogInformation("Получено сообщение '{MessageText}' от пользователя {UserId} в чате {ChatId}", 
            messageText, userId, chatId);

        // Команда должна начинаться с '/'
        if (!messageText.StartsWith("/"))
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Я понимаю только команды. Для списка команд используйте /help."
            );
            return;
        }
        
        // Извлекаем имя команды (первое слово)
        var commandName = messageText.Split(' ')[0].ToLower();
        
        // Создаем "фальшивый" Update, чтобы передать его в executor.
        // Это необходимо, т.к. мы уже разделили логику.
        var update = new Update { Message = message };
        
        await _commandExecutor.ExecuteCommandAsync(commandName, update, botClient);
    }

    private Task HandleUnknownUpdateAsync(Update update)
    {
        _logger.LogInformation("Получено обновление неизвестного типа: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}