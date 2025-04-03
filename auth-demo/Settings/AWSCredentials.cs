namespace AuthDemo.Settings;

public class AWSCredentials 
{
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }

    public static AWSCredentials CreateEmpty() 
    {
        return new AWSCredentials 
        {
            AccessKey = string.Empty,
            SecretKey = string.Empty
        };
    }
}