namespace Blog;

public static class Configuration
{
    public static string JwtKey = "`0G3bQ2y,hC%0SW*$va`23c;!s$3FE1j";

    public static string ApiKeyName = "api_key";
    
    public static string ApiKey = "Quiporro_temporibus-in-laborumT%architecto==sit";

    public static SmtpConfiguration Smtp = new();
    
    public class SmtpConfiguration
    {
        public string Host { get; set; }

        public int Port { get; set; } = 25;
        
        public string UserName { get; set; }
        
        public string Password { get; set; }
    }
}