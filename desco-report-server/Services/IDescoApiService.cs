using desco_report_server.Models;

namespace desco_report_server.Services;

public interface IDescoApiService
{
    Task<DescoAccount?> GetAccountBalanceAsync(string accountNo, string meterNo);
    Task<List<DescoLocation>> GetCustomerLocationAsync(string accountNo);
    Task<List<DescoDailyConsumption>> GetDailyConsumptionAsync(string accountNo, string meterNo, DateTime dateFrom, DateTime dateTo);
    Task<List<DescoRecentEvent>> GetRecentEventsAsync(string accountNo);
    Task<List<DescoRechargeHistory>> GetRechargeHistoryAsync(string accountNo, string meterNo, DateTime dateFrom, DateTime dateTo);
    Task<List<DescoMonthlyConsumption>> GetMonthlyConsumptionAsync(string accountNo, string meterNo, DateTime monthFrom, DateTime monthTo);
    Task<bool> ValidateAccountAsync(string accountNo, string meterNo);
    Task SyncAccountDataAsync(string accountNo, string meterNo);
}