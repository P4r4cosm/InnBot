using Dadata;
using InnBot.Configuration;
using InnBot.DataProviders;
using InnBot.Models;
using Microsoft.Extensions.Options;


public class DadataProvider: IDataProvider
{
    private readonly SuggestClientAsync _api;

    public DadataProvider(IOptions<DadataConfiguration> config)
    {
        var dadataConfig = config.Value;
        _api = new SuggestClientAsync(dadataConfig.ApiKey);
    }
    public async Task<IEnumerable<CompanyInfo>> GetCompanyInfoByInnAsync(IEnumerable<string> inns)
    {
        var results = new List<CompanyInfo>();
        foreach (var inn in inns)
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
        }
        return results;
    }
}