using System.Text;
using InnBot.DataProviders;
using InnBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnBot.Commands;

/// <summary>
/// Представляет результат валидации одного кандидата в ИНН.
/// </summary>
/// <param name="OriginalInput">Исходная строка, введенная пользователем.</param>
/// <param name="IsValid">Является ли строка валидным ИНН по формату.</param>
/// <param name="ErrorMessage">Сообщение об ошибке, если IsValid = false.</param>
public record InnValidationResult(string OriginalInput, bool IsValid, string? ErrorMessage);

public class InnCommand : ICommand
{
    public string Name => "/inn";

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
        
        
        // Отделяем саму команду от аргументов
        var argsString = messageText.Length > Name.Length ? messageText.Substring(Name.Length) : string.Empty;
        
        // набор разделителей
        char[] delimiters = { ' ', ',', ';', '\n', '\r' };

        // Разбиваем строку с аргументами, используя все разделители, и удаляем пустые записи
        var innCandidates = argsString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

        if (!innCandidates.Any())
        {
            await botClient.SendMessage(chatId,
                "Пожалуйста, укажите один или несколько ИНН после команды.\n" +
                "Их можно разделять пробелами, запятыми или точкой с запятой.\n" +
                "Пример: `/inn 7707083893, 7706799482`",
                parseMode: ParseMode.Markdown);
            return;
        }

        _logger.LogInformation("Запрос на обработку {Count} кандидатов в ИНН для чата {ChatId}.", innCandidates.Count, chatId);

        // 2. Валидируем каждый кандидат
        var validationResults = innCandidates.Select(ValidateInn).ToList();

        // 3. Отбираем только валидные ИНН для запроса к API
        var validInns = validationResults
            .Where(r => r.IsValid)
            .Select(r => r.OriginalInput)
            .Distinct()
            .ToList();

        // 4. Получаем информацию о компаниях (если есть что запрашивать)
        List<CompanyInfo> foundCompanies = new();
        if (validInns.Any())
        {
            try
            {
                var companies = await _dataProvider.GetCompanyInfoByInnAsync(validInns);
                foundCompanies.AddRange(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных от провайдера для чата {ChatId}", chatId);
                await botClient.SendMessage(chatId, "Произошла ошибка при получении данных от внешнего сервиса. Попробуйте позже.");
                return;
            }
        }
        
        // 5. Формируем детальный отчет
        var responseText = BuildResponse(validationResults, foundCompanies);
        

        await botClient.SendMessage(chatId, responseText, parseMode: ParseMode.Markdown);
    }

    private static InnValidationResult ValidateInn(string innCandidate)
    {
        if (string.IsNullOrWhiteSpace(innCandidate))
        {
            return new InnValidationResult(innCandidate, false, "Пустое значение.");
        }
        if (!innCandidate.All(char.IsDigit))
        {
            return new InnValidationResult(innCandidate, false, "Содержит недопустимые символы (не цифры).");
        }
        if (innCandidate.Length != 10 && innCandidate.Length != 12)
        {
            return new InnValidationResult(innCandidate, false, $"Неверная длина ({innCandidate.Length} цифр), должно быть 10 или 12.");
        }

        return new InnValidationResult(innCandidate, true, null);
    }
    
    private string BuildResponse(List<InnValidationResult> validationResults, List<CompanyInfo> foundCompanies)
    {
        var responseBuilder = new StringBuilder();
        
        var sortedCompanies = foundCompanies.OrderBy(c => c.Name).ToList();
        var foundInns = sortedCompanies.Select(c => c.Inn).ToHashSet();
        
        if (sortedCompanies.Any())
        {
            responseBuilder.AppendLine("✅ **Найденные компании:**");
            responseBuilder.AppendLine();
            foreach (var company in sortedCompanies)
            {
                responseBuilder.AppendLine($"**{company.Name}**");
                responseBuilder.AppendLine($"   ИНН: `{company.Inn}`");
                responseBuilder.AppendLine($"   Адрес: {company.Address ?? "не указан"}");
                responseBuilder.AppendLine();
            }
        }

        var notFoundInns = validationResults
            .Where(r => r.IsValid && !foundInns.Contains(r.OriginalInput))
            .ToList();
        
        if (notFoundInns.Any())
        {
            responseBuilder.AppendLine("🟡 **Не найдены в базе:**");
            foreach (var result in notFoundInns)
            {
                responseBuilder.AppendLine($"`{result.OriginalInput}`");
            }
            responseBuilder.AppendLine();
        }

        var invalidResults = validationResults.Where(r => !r.IsValid).ToList();
        if (invalidResults.Any())
        {
            responseBuilder.AppendLine("❌ **Некорректный формат:**");
            foreach (var result in invalidResults)
            {
                responseBuilder.AppendLine($"`{result.OriginalInput}` - {result.ErrorMessage}");
            }
            responseBuilder.AppendLine();
        }

        if (responseBuilder.Length == 0)
        {
            return "Не удалось обработать ваш запрос. Проверьте введенные данные.";
        }
        
        return responseBuilder.ToString();
    }
}