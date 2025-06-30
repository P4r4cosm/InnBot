// InnBot/DataProviders/DadataProvider.cs
using Dadata;
using InnBot.Configuration;
using InnBot.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace InnBot.DataProviders;

public class DadataProvider : IDataProvider
{
    private readonly SuggestClientAsync _api;
    private readonly ILogger<DadataProvider> _logger;

    public DadataProvider(IOptions<DadataConfiguration> config, ILogger<DadataProvider> logger)
    {
        var dadataConfig = config.Value;
        if (string.IsNullOrEmpty(dadataConfig.ApiKey))
        {
            throw new ArgumentNullException(nameof(dadataConfig.ApiKey), "API-ключ для Dadata не установлен в конфигурации.");
        }
        _api = new SuggestClientAsync(dadataConfig.ApiKey);
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyInfo>> GetCompanyInfoByInnAsync(IEnumerable<string> inns)
    {
        // Используем потокобезопасную коллекцию, так как будем добавлять в нее из разных потоков
        var results = new ConcurrentBag<CompanyInfo>();
        var uniqueInns = inns.Distinct().ToList();

        _logger.LogInformation("Запрос информации для {Inn_count} уникальных ИНН от Dadata.", uniqueInns.Count);

        // Создаем список задач для параллельного выполнения
        var tasks = uniqueInns.Select(async inn =>
        {
            try
            {
                var response = await _api.FindParty(inn);
                var party = response.suggestions.FirstOrDefault()?.data;

                if (party != null)
                {
                    results.Add(new CompanyInfo
                    {
                        Inn = inn,
                        Name = party.name.full_with_opf ?? party.name.full,
                        Address = party.address?.unrestricted_value ?? "Адрес не найден"
                    });
                }
                else
                {
                     _logger.LogWarning("По ИНН {Inn} не найдена информация в Dadata.", inn);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запросе информации по ИНН {Inn} от Dadata.", inn);
            }
        });

        // Ожидаем завершения всех запросов
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("Получено {Results_count} записей от Dadata.", results.Count);

        return results;
    }
}