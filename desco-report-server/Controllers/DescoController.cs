using desco_report_server.Data;
using desco_report_server.Models;
using desco_report_server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace desco_report_server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DescoController : ControllerBase
{
    private readonly IDescoApiService _descoApiService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DescoController> _logger;

    public DescoController(
        IDescoApiService descoApiService,
        ApplicationDbContext context,
        ILogger<DescoController> logger)
    {
        _descoApiService = descoApiService;
        _context = context;
        _logger = logger;
    }

    [HttpGet("balance/{accountNo}/{meterNo}")]
    public async Task<IActionResult> GetBalance(string accountNo, string meterNo)
    {
        try
        {
            var balance = await _descoApiService.GetAccountBalanceAsync(accountNo, meterNo);
            if (balance == null)
                return NotFound("Account not found or unable to fetch balance");

            return Ok(new
            {
                balance.AccountNumber,
                balance.MeterNumber,
                balance.CurrentBalance,
                balance.CurrentMonthConsumption,
                balance.LastReadingTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching balance for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching balance");
        }
    }

    [HttpGet("daily-consumption/{accountNo}/{meterNo}")]
    public async Task<IActionResult> GetDailyConsumption(
        string accountNo, 
        string meterNo, 
        [FromQuery] DateTime dateFrom, 
        [FromQuery] DateTime dateTo)
    {
        try
        {
            var consumptions = await _descoApiService.GetDailyConsumptionAsync(accountNo, meterNo, dateFrom, dateTo);
            return Ok(consumptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily consumption for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching daily consumption");
        }
    }

    [HttpGet("monthly-consumption/{accountNo}/{meterNo}")]
    public async Task<IActionResult> GetMonthlyConsumption(
        string accountNo, 
        string meterNo, 
        [FromQuery] DateTime monthFrom, 
        [FromQuery] DateTime monthTo)
    {
        try
        {
            var consumptions = await _descoApiService.GetMonthlyConsumptionAsync(accountNo, meterNo, monthFrom, monthTo);
            return Ok(consumptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly consumption for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching monthly consumption");
        }
    }

    [HttpGet("recharge-history/{accountNo}/{meterNo}")]
    public async Task<IActionResult> GetRechargeHistory(
        string accountNo, 
        string meterNo, 
        [FromQuery] DateTime dateFrom, 
        [FromQuery] DateTime dateTo)
    {
        try
        {
            var history = await _descoApiService.GetRechargeHistoryAsync(accountNo, meterNo, dateFrom, dateTo);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recharge history for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching recharge history");
        }
    }

    [HttpGet("recent-events/{accountNo}")]
    public async Task<IActionResult> GetRecentEvents(string accountNo)
    {
        try
        {
            var events = await _descoApiService.GetRecentEventsAsync(accountNo);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent events for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching recent events");
        }
    }

    [HttpGet("location/{accountNo}")]
    public async Task<IActionResult> GetLocation(string accountNo)
    {
        try
        {
            var locations = await _descoApiService.GetCustomerLocationAsync(accountNo);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching location for {AccountNo}", accountNo);
            return StatusCode(500, "An error occurred while fetching location");
        }
    }

    [HttpPost("sync-account")]
    public async Task<IActionResult> SyncAccount([FromBody] SyncAccountRequest request)
    {
        try
        {
            if (!await _descoApiService.ValidateAccountAsync(request.AccountNo, request.MeterNo))
                return BadRequest("Invalid account or meter number");

            await _descoApiService.SyncAccountDataAsync(request.AccountNo, request.MeterNo);
            return Ok("Account data synced successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing account data for {AccountNo}", request.AccountNo);
            return StatusCode(500, "An error occurred while syncing account data");
        }
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetUserAccounts()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value ?? string.Empty;
            var accounts = await _context.DescoAccounts
                .Where(da => da.UserId == userId)
                .ToListAsync();

            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user accounts");
            return StatusCode(500, "An error occurred while fetching accounts");
        }
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> AddAccount([FromBody] AddAccountRequest request)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value ?? string.Empty;
            
            if (!await _descoApiService.ValidateAccountAsync(request.AccountNo, request.MeterNo))
                return BadRequest("Invalid account or meter number");

            var existingAccount = await _context.DescoAccounts
                .FirstOrDefaultAsync(da => da.AccountNumber == request.AccountNo && da.MeterNumber == request.MeterNo);

            if (existingAccount != null)
                return BadRequest("Account already exists");

            var account = new DescoAccount
            {
                UserId = userId,
                AccountNumber = request.AccountNo,
                MeterNumber = request.MeterNo,
                CustomerName = request.CustomerName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DescoAccounts.Add(account);
            await _context.SaveChangesAsync();

            await _descoApiService.SyncAccountDataAsync(request.AccountNo, request.MeterNo);

            return CreatedAtAction(nameof(GetBalance), new { accountNo = request.AccountNo, meterNo = request.MeterNo }, account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding account");
            return StatusCode(500, "An error occurred while adding the account");
        }
    }
}

public class SyncAccountRequest
{
    public string AccountNo { get; set; } = string.Empty;
    public string MeterNo { get; set; } = string.Empty;
}

public class AddAccountRequest
{
    public string AccountNo { get; set; } = string.Empty;
    public string MeterNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}