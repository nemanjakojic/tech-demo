namespace AuthDemo.Settings;

public class AWSCognitoSettings
{
    public required string Region {get; set;}
    public required string UserPoolId { get; set; }
    public required string AppClientId { get; set; }
    public required string AppClientSecret { get; set; }

    public static AWSCognitoSettings CreateEmpty() 
    {
        return new AWSCognitoSettings 
        {
            Region = string.Empty,
            AppClientId = string.Empty,
            AppClientSecret = string.Empty,
            UserPoolId = string.Empty
        };
    }
}