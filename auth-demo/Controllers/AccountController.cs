using AuthDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly CognitoAuthService _cognitoAuthService;
    // private readonly AWSCognitoSettings _awsCognitoSettings;

    /// <summary>
    /// The constructor receives relevant configuration settings via strongly-typed injection.
    /// </summary>
    /// <param name="awsCognitoOptions">Injected AWS Cognito settings.</param>
    // public AccountController(IOptions<AWSCognitoSettings> awsCognitoOptions, IOptions<AWSCredentials> awsCredentialsOptions)
    public AccountController(CognitoAuthService cognitoService)
    {
        // _awsCognitoSettings = awsCognitoOptions.Value;
        // _cognitoAuthService = new CognitoAuthService(awsCognitoOptions, awsCredentialsOptions);
        _cognitoAuthService = cognitoService;
    }

    // 1️⃣ Register User
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserModel model)
    {
        if (model == null) return BadRequest("Invalid request.");

        var userId = await _cognitoAuthService.RegisterUserAsync(model.Email, model.Password, model.GivenName, model.FamilyName);

        if (userId != null)
            return Ok(new { Message = "User registered successfully! Check your email for the confirmation code.", UserId = userId });

        return BadRequest("User registration failed.");
    }

    // 2️⃣ Confirm Email with Verification Code
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmUser([FromBody] ConfirmUserModel model)
    {
        if (model == null) return BadRequest("Invalid request.");

        var isConfirmed = await _cognitoAuthService.ConfirmUserEmailAsync(model.Email, model.ConfirmationCode);

        if (isConfirmed)
            return Ok(new { Message = "Email confirmed successfully. You can now log in." });

        return BadRequest("Invalid confirmation code or user not found.");
    }

    // 3️⃣ Login & Get JWT Token
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginModel model)
    {
        if (model == null) return BadRequest("Invalid request.");

        var authResponse = await _cognitoAuthService.LoginUserAsync(model.Email, model.Password);

        if (authResponse == null) 
        {
            return Unauthorized("Invalid credentials.");
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Ensures it's only sent over HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = authResponse.ExpiresAt
        };

        Response.Cookies.Append("AccessToken", authResponse.AccessToken, cookieOptions);
        Response.Cookies.Append("IdToken", authResponse.IdToken, cookieOptions);
        Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, cookieOptions);

        return Ok(new {Message = "Login successful."});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            var response = await _cognitoAuthService.RefreshAccessTokenAsync(request.RefreshToken);
            
            // Optionally set the new access token in an HTTP-only cookie
            Response.Cookies.Append("AccessToken", response.AccessToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = response.ExpiresAt
            });

            return Ok(new { Message = "Access token refreshed successfully." });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token", error = ex.Message });
        }
    }
}

// Request model for refresh token
public class RefreshTokenRequest
{
    public required string RefreshToken { get; set; }
}

// Request models

public class RegisterUserModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string GivenName { get; set; }
    public required string FamilyName { get; set; }
}

public class ConfirmUserModel
{
    public required string Email { get; set; }
    public required string ConfirmationCode { get; set; }
}

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
