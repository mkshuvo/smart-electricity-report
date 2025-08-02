using System.Text.Json;
using desco_report_server.Data;
using desco_report_server.Models;
using Microsoft.EntityFrameworkCore;

namespace desco_report_server.Services;

public class DescoApiService : IDescoApiService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DescoApiService> _logger;

    public DescoApiService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<DescoApiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<DescoAccount?> GetAccountBalanceAsync(string accountNo, string meterNo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/tkdes/customer/getBalance?accountNo={accountNo}&meterNo={meterNo}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch balance for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<DescoBalanceData>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Data == null)
            {
                _logger.LogWarning("Invalid response format for account {AccountNo}", accountNo);
                return null;
            }

            var account = await _context.DescoAccounts
                .FirstOrDefaultAsync(da => da.AccountNumber == accountNo && da.MeterNumber == meterNo);

            if (account == null)
            {
                account = new DescoAccount
                {
                    AccountNumber = apiResponse.Data.AccountNo,
                    MeterNumber = apiResponse.Data.MeterNo,
                    CurrentBalance = apiResponse.Data.Balance,
                    CurrentMonthConsumption = apiResponse.Data.CurrentMonthConsumption,
                    LastReadingTime = DateTime.Parse(apiResponse.Data.ReadingTime),
                    LastSyncAt = DateTime.UtcNow
                };
                _context.DescoAccounts.Add(account);
            }
            else
            {
                account.CurrentBalance = apiResponse.Data.Balance;
                account.CurrentMonthConsumption = apiResponse.Data.CurrentMonthConsumption;
                account.LastReadingTime = DateTime.Parse(apiResponse.Data.ReadingTime);
                account.LastSyncAt = DateTime.UtcNow;
                _context.DescoAccounts.Update(account);
            }

            await _context.SaveChangesAsync();
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching balance for account {AccountNo}", accountNo);
            return null;
        }
    }

    public async Task<List<DescoLocation>> GetCustomerLocationAsync(string accountNo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/common/getCustomerLocation?accountNo={accountNo}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch location for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return new List<DescoLocation>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<List<DescoLocationData>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var locations = new List<DescoLocation>();
            if (apiResponse?.Data != null)
            {
                foreach (var locationData in apiResponse.Data)
                {
                    var location = new DescoLocation
                    {
                        Division = locationData.Division,
                        District = locationData.District,
                        Thana = locationData.Thana,
                        Area = locationData.Area,
                        PostCode = locationData.PostCode,
                        FullAddress = locationData.FullAddress,
                        Latitude = locationData.Latitude,
                        Longitude = locationData.Longitude,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    locations.Add(location);
                }
            }

            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching location for account {AccountNo}", accountNo);
            return new List<DescoLocation>();
        }
    }

    public async Task<List<DescoDailyConsumption>> GetDailyConsumptionAsync(string accountNo, string meterNo, DateTime dateFrom, DateTime dateTo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/tkdes/customer/getCustomerDailyConsumption?accountNo={accountNo}&meterNo={meterNo}&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch daily consumption for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return new List<DescoDailyConsumption>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<List<DescoDailyConsumptionData>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var consumptions = new List<DescoDailyConsumption>();
            if (apiResponse?.Data != null)
            {
                foreach (var consumptionData in apiResponse.Data)
                {
                    var consumption = new DescoDailyConsumption
                    {
                        Date = DateTime.Parse(consumptionData.Date),
                        ConsumptionValue = consumptionData.Consumption,
                        Unit = consumptionData.Unit ?? "kWh",
                        Cost = consumptionData.Cost,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    consumptions.Add(consumption);
                }
            }

            return consumptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily consumption for account {AccountNo}", accountNo);
            return new List<DescoDailyConsumption>();
        }
    }

    public async Task<List<DescoRecentEvent>> GetRecentEventsAsync(string accountNo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/complaint/push-notification/getRecentEvent?accountNo={accountNo}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch recent events for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return new List<DescoRecentEvent>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<List<DescoEventData>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var events = new List<DescoRecentEvent>();
            if (apiResponse?.Data != null)
            {
                foreach (var eventData in apiResponse.Data)
                {
                    var recentEvent = new DescoRecentEvent
                    {
                        EventDate = DateTime.Parse(eventData.EventDate),
                        EventType = eventData.EventType,
                        Message = eventData.Message,
                        Category = eventData.Category,
                        Priority = eventData.Priority,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    events.Add(recentEvent);
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent events for account {AccountNo}", accountNo);
            return new List<DescoRecentEvent>();
        }
    }

    public async Task<List<DescoRechargeHistory>> GetRechargeHistoryAsync(string accountNo, string meterNo, DateTime dateFrom, DateTime dateTo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/tkdes/customer/getRechargeHistory?accountNo={accountNo}&meterNo={meterNo}&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch recharge history for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return new List<DescoRechargeHistory>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<List<DescoRechargeData>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var recharges = new List<DescoRechargeHistory>();
            if (apiResponse?.Data != null)
            {
                foreach (var rechargeData in apiResponse.Data)
                {
                    var recharge = new DescoRechargeHistory
                    {
                        RechargeDate = DateTime.Parse(rechargeData.RechargeDate),
                        Amount = rechargeData.Amount,
                        TransactionId = rechargeData.TransactionId,
                        PaymentMethod = rechargeData.PaymentMethod,
                        Notes = rechargeData.Notes,
                        Status = rechargeData.Status,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    recharges.Add(recharge);
                }
            }

            return recharges;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recharge history for account {AccountNo}", accountNo);
            return new List<DescoRechargeHistory>();
        }
    }

    public async Task<List<DescoMonthlyConsumption>> GetMonthlyConsumptionAsync(string accountNo, string meterNo, DateTime monthFrom, DateTime monthTo)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DescoApi");
            var response = await httpClient.GetAsync($"/api/tkdes/customer/getCustomerMonthlyConsumption?accountNo={accountNo}&meterNo={meterNo}&monthFrom={monthFrom:yyyy-MM}&monthTo={monthTo:yyyy-MM}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch monthly consumption for account {AccountNo}: {StatusCode}", accountNo, response.StatusCode);
                return new List<DescoMonthlyConsumption>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<DescoApiResponse<List<DescoMonthlyConsumptionData>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var consumptions = new List<DescoMonthlyConsumption>();
            if (apiResponse?.Data != null)
            {
                foreach (var consumptionData in apiResponse.Data)
                {
                    var consumption = new DescoMonthlyConsumption
                    {
                        Year = consumptionData.Year,
                        Month = consumptionData.Month,
                        ConsumptionValue = consumptionData.Consumption,
                        Unit = consumptionData.Unit ?? "kWh",
                        Cost = consumptionData.Cost,
                        AverageDailyConsumption = consumptionData.AverageDailyConsumption,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    consumptions.Add(consumption);
                }
            }

            return consumptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly consumption for account {AccountNo}", accountNo);
            return new List<DescoMonthlyConsumption>();
        }
    }

    public async Task<bool> ValidateAccountAsync(string accountNo, string meterNo)
    {
        try
        {
            var account = await GetAccountBalanceAsync(accountNo, meterNo);
            return account != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating account {AccountNo}", accountNo);
            return false;
        }
    }

    public async Task SyncAccountDataAsync(string accountNo, string meterNo)
    {
        try
        {
            var account = await GetAccountBalanceAsync(accountNo, meterNo);
            if (account == null)
            {
                _logger.LogWarning("Account validation failed for {AccountNo}", accountNo);
                return;
            }

            // Sync daily consumption for last 30 days
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-30);
            var dailyConsumptions = await GetDailyConsumptionAsync(accountNo, meterNo, startDate, endDate);

            // Sync monthly consumption for last 12 months
            var monthlyConsumptions = await GetMonthlyConsumptionAsync(accountNo, meterNo, 
                DateTime.Now.AddMonths(-12), DateTime.Now);

            // Sync recharge history for last 6 months
            var rechargeHistory = await GetRechargeHistoryAsync(accountNo, meterNo,
                DateTime.Now.AddMonths(-6), DateTime.Now);

            // Sync recent events
            var recentEvents = await GetRecentEventsAsync(accountNo);

            // Sync location data
            var locations = await GetCustomerLocationAsync(accountNo);

            _logger.LogInformation("Successfully synced data for account {AccountNo}", accountNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing account data for {AccountNo}", accountNo);
        }
    }
}

// Helper classes for API response deserialization
public class DescoApiResponse<T>
{
    public int Code { get; set; }
    public string Desc { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class DescoBalanceData
{
    public string AccountNo { get; set; } = string.Empty;
    public string MeterNo { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal CurrentMonthConsumption { get; set; }
    public string ReadingTime { get; set; } = string.Empty;
}

public class DescoLocationData
{
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? Thana { get; set; }
    public string? Area { get; set; }
    public string? PostCode { get; set; }
    public string? FullAddress { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class DescoDailyConsumptionData
{
    public string Date { get; set; } = string.Empty;
    public decimal Consumption { get; set; }
    public string? Unit { get; set; }
    public decimal? Cost { get; set; }
}

public class DescoMonthlyConsumptionData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Consumption { get; set; }
    public string? Unit { get; set; }
    public decimal? Cost { get; set; }
    public decimal? AverageDailyConsumption { get; set; }
}

public class DescoRechargeData
{
    public string RechargeDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
}

public class DescoEventData
{
    public string EventDate { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Priority { get; set; }
}