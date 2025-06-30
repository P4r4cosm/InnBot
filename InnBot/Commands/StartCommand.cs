using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class StartCommand: ICommand
{
    public string Name { get; } = "/start";
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;
        var userName = update.Message.From?.FirstName ?? "пользователь";

        var message = new StringBuilder();
        message.AppendLine($"👋 Привет, {userName}!");
        message.AppendLine();
        message.AppendLine("Я бот для получения информации о компаниях по их ИНН.");
        message.AppendLine("Чтобы увидеть список всех доступных команд - отправьте /help.");

        await botClient.SendMessage(
            chatId: chatId,
            text: message.ToString()
        );
    }
}