using System.IdentityModel.Tokens.Jwt;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using AuthDemo.Settings;
using Microsoft.Extensions.Options;
using AWSCredentials = AuthDemo.Settings.AWSCredentials;

namespace AuthDemo.Services;

public class CognitoAuthService
{
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient;

    private readonly AWSCognitoSettings _awsCognitoSettings;
    
    public CognitoAuthService(IOptions<AWSCognitoSettings> cognitoSettings, IOptions<AWSCredentials> awsKeys)
    {
        _awsCognitoSettings = cognitoSettings.Value;

        var awsCredentials = new BasicAWSCredentials(
            accessKey: awsKeys.Value.AccessKey,
            secretKey: awsKeys.Value.SecretKey);
        _cognitoClient = new AmazonCognitoIdentityProviderClient(awsCredentials, RegionEndpoint.GetBySystemName(cognitoSettings.Value.Region));
    }

    // 1️⃣ Register User (Sign-Up)
    public async Task<string> RegisterUserAsync(string email, string password, string givenName, string familyName)
    {
        try
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = _awsCognitoSettings.AppClientId,
                Username = email,
                Password = password,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = email },
                    new AttributeType { Name = "name", Value = givenName },
                    new AttributeType { Name = "birthdate", Value = "2020-01-01" }
                }
            };

            var response = await _cognitoClient.SignUpAsync(signUpRequest);
            return response.UserSub; // Returns Cognito user ID
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            return "Invalid Response";
        }
    }

    // 2️⃣ Confirm User Email (Verification Code Sent by Cognito)
    public async Task<bool> ConfirmUserEmailAsync(string email, string confirmationCode)
    {
        try
        {
            var confirmRequest = new ConfirmSignUpRequest
            {
                ClientId = _awsCognitoSettings.AppClientId,
                Username = email,
                ConfirmationCode = confirmationCode
            };

            await _cognitoClient.ConfirmSignUpAsync(confirmRequest);
            return true; // Confirmation successful
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming email: {ex.Message}");
            return false;
        }
    }

    // 3️⃣ User Login & Get Tokens
    public async Task<AuthResponse> LoginUserAsync(string email, string password)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                ClientId = _awsCognitoSettings.AppClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password }
                }
            };

            var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest);

            var result = new AuthResponse
            {
                AccessToken = authResponse.AuthenticationResult.AccessToken,
                IdToken = authResponse.AuthenticationResult.IdToken,
                RefreshToken = authResponse.AuthenticationResult.RefreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(authResponse.AuthenticationResult.ExpiresIn)
            };

            var handler = new JwtSecurityTokenHandler();
            var accessJWT = handler.ReadJwtToken(result.AccessToken);
            var idJWT = handler.ReadJwtToken(result.IdToken);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging in: {ex.Message}");
            throw;
        }
    }

    public async Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                ClientId = _awsCognitoSettings.AppClientId,
                AuthParameters = new Dictionary<string, string>
                {
                    { "REFRESH_TOKEN", refreshToken }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(request);

            if (response.AuthenticationResult != null)
            {
                return new RefreshTokenResponse 
                {
                    AccessToken = response.AuthenticationResult.AccessToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.AuthenticationResult.ExpiresIn)
                };
            }
            else
            {
                throw new Exception("Failed to refresh access token");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error refreshing token: " + ex.Message);
            throw;
        }
    }
}

// Model for storing authentication response
public class AuthResponse
{
    public required string AccessToken { get; set; }
    public required string IdToken { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
}

public class RefreshTokenResponse
{
    public required string AccessToken { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
}
