using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class HelloCommand: ICommand
{
    public string Name { get; } = "/hello";
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        await botClient.SendMessage(update.Message.Chat.Id, "Hello there!");
    }
}