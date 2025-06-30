using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnBot.Commands;

public class HelpCommand: ICommand
{
    public string Name { get; } = "/help";
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;

        var helpText = new StringBuilder();
        helpText.AppendLine("**Доступные команды:**");
        helpText.AppendLine("`/start` - Начало работы и приветствие");
        helpText.AppendLine("`/help` - Показать это сообщение со списком команд");
        helpText.AppendLine("`/hello` - Информация об авторе бота");
        helpText.AppendLine("`/inn <ИНН1> <ИНН2>...` - Найти компании по ИНН. Разделяйте несколько ИНН пробелом, запятой или точкой с запятой.");
        helpText.AppendLine("`/last` - Повторить последнюю выполненную команду (кроме `/start` и `/help`)");

        await botClient.SendMessage(
            chatId: chatId,
            text: helpText.ToString(),
            parseMode: ParseMode.Markdown
        );
    }
}