using InnBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class LastCommand: ICommand
{
    public string Name { get; } = "/last";
    private readonly IUserHistoryService _historyService;

    public LastCommand(IUserHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;

        // Получаем последнюю команду из истории
        var (lastCommand, lastUpdate) = _historyService.GetLastCommand(chatId);

        if (lastCommand != null && lastUpdate != null)
        {
            // Если нашли - просто выполняем ее снова с теми же данными
            await botClient.SendMessage(chatId, $"Повторяю последнюю команду: `{lastUpdate.Message.Text}`",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            await lastCommand.ExecuteAsync(lastUpdate, botClient);
        }
        else
        {
            await botClient.SendMessage(chatId, "Нет команд для повторения.");
        }
    }
}