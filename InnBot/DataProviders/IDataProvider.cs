using InnBot.Models;

namespace InnBot.DataProviders;

public interface IDataProvider
{
    Task<IEnumerable<CompanyInfo>> GetCompanyInfoByInnAsync(IEnumerable<string> inns); 
}