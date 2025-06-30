using System.Text;
using InnBot.DataProviders;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnBot.Commands;

public class InnCommand: ICommand
{
    public string Name { get; } = "/inn";
    private readonly IDataProvider _dataProvider;
    private readonly ILogger<InnCommand> _logger;

    public InnCommand(IDataProvider dataProvider, ILogger<InnCommand> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
    }
    public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
    {
        var chatId = update.Message!.Chat.Id;
        var messageText = update.Message.Text;

        // 1. Парсим ИНН из сообщения
        var inns = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Skip(1) // Пропускаем саму команду "/inn"
                              .ToList();

        if (!inns.Any())
        {
            await botClient.SendMessage(chatId, 
                "Пожалуйста, укажите один или несколько ИНН после команды.\nПример: `/inn 7707083893 7706799482`",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            return;
        }

        // 2. Получаем данные через наш провайдер
        var companies = await _dataProvider.GetCompanyInfoByInnAsync(inns);

        // 3. Сортируем по имени компании, как требуется в задании
        var sortedCompanies = companies.OrderBy(c => c.Name).ToList();
        
        // 4. Формируем и отправляем ответ
        if (!sortedCompanies.Any())
        {
            await botClient.SendMessage(chatId, "Не удалось найти информацию по указанным ИНН.");
            return;
        }

        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine($"Найдена информация по {sortedCompanies.Count} из {inns.Count} ИНН:");
        responseBuilder.AppendLine();

        foreach (var company in sortedCompanies)
        {
            responseBuilder.AppendLine($"**✅ {company.Name}**");
            responseBuilder.AppendLine($"   ИНН: `{company.Inn}`"); // `...` для моноширинного шрифта
            responseBuilder.AppendLine($"   Адрес: {company.Address}");
            responseBuilder.AppendLine(); // Пустая строка для разделения
        }

        await botClient.SendMessage(chatId, responseBuilder.ToString(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

        // 5. Сохраняем команду для /last (пока закомментировано)
        // _historyService.SetLastCommand(chatId, this, update);
    }
}