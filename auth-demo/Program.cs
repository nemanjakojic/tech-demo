using AuthDemo.Middleware;
using AuthDemo.Services;
using AuthDemo.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load and bind `JwtSettings` before building `app`
var awsCognitoSection = builder.Configuration.GetSection("AWS:Cognito");
var awsCognitoSettings = AWSCognitoSettings.CreateEmpty();
awsCognitoSection.Bind(awsCognitoSettings);

var awsCredentialsSection = builder.Configuration.GetSection("AWS:Credentials");
var awsCredentials = AWSCredentials.CreateEmpty();
awsCredentialsSection.Bind(awsCredentials);

// Make AWS Congito settings injectable
builder.Services.Configure<AWSCognitoSettings>(awsCognitoSection);
builder.Services.Configure<AWSCredentials>(awsCredentialsSection);
builder.Services.AddTransient<CognitoAuthService>();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200", // Angular app's URL
            "http://localhost:5173") // React app's URL
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services        
        .AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://cognito-idp.{awsCognitoSettings.Region}.amazonaws.com/{awsCognitoSettings.UserPoolId}";
            options.Audience = awsCognitoSettings.AppClientId;

            options.RequireHttpsMetadata = true; // Always use HTTPS
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        });

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireClaim("cognito:groups", "Admin"));  // Check the cognito:groups claim
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAngularApp");
}

app.UseHttpsRedirection();
app.UseRouting();

// The solution delivers JWT tokens in HTTP-Only cookies to prevent access by client-side scripts.
// Because of this, the client-side code has no way to populate the Authorization header with these tokens.
// On the other hand, ASP.NET middleware expects to find JWTs in the Authorization header. 
// A custom middleware component, TokenForwardingMiddleware, is designed to extract AccessToken from the Cookie and place into the Authorization header.
// Therefore, TokenForwardingMiddleware is placed before Authentication step to ensure correct AccessToken handling.
app.UseMiddleware<TokenForwardingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
    {
        _ = endpoints.MapControllers();
    });

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
