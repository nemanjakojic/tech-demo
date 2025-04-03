using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.Controllers;

[ApiController]
[Route("api/accounts")]
public class BankAccountsController : ControllerBase
{
    // Sample in-memory data store
    private static readonly List<BankAccount> Accounts = new()
    {
        new BankAccount { Id = 1, AccountHolder = "John Doe", Balance = 1000.50m },
        new BankAccount { Id = 2, AccountHolder = "Jane Smith", Balance = 2500.75m },
        new BankAccount { Id = 3, AccountHolder = "Alice Johnson", Balance = 500.00m }
    };

    /// <summary>
    /// Get all bank accounts.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult GetAccounts()
    {
        return Ok(Accounts);
    }

    /// <summary>
    /// Get a bank account by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public IActionResult GetAccountById(int id)
    {
        var account = Accounts.FirstOrDefault(a => a.Id == id);
        if (account == null)
        {
            return NotFound(new { message = "Account not found" });
        }
        return Ok(account);
    }
}

/// <summary>
/// Represents a bank account.
/// </summary>
public class BankAccount
{
    public int Id { get; set; }
    public string AccountHolder { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
