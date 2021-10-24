using System.IO;

namespace LebakasBot
{
    public class TokenManager
    {
        public string Token { get; }

        public TokenManager(string path = "token.txt")
        {
            Token = File.ReadAllText(path);
        }
    }
}
