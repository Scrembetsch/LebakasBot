using System.IO;

namespace LebakasBot
{
    public class TokenManager
    {
        private const string cTokenPathVar = "TOKEN_PATH";
        public string Token { get; }

        public TokenManager(string path = "token.txt")
        {
            string pathToToken = System.Environment.GetEnvironmentVariable(cTokenPathVar);
            if (string.IsNullOrWhiteSpace(pathToToken))
            {
                pathToToken = "";
            }
            else
            {
                if (!pathToToken.EndsWith('/'))
                {
                    pathToToken += '/';
                }
            }
            Token = File.ReadAllText(pathToToken + path);
        }
    }
}
