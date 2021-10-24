using System.IO;

namespace LebakasBot
{
    public class TokenManager
    {
        public string Token { get; }

        public TokenManager(string path = "token.txt")
        {
            string currentDir = System.Environment.CurrentDirectory;
            if(!currentDir.EndsWith('/'))
            {
                currentDir += '/';
            }
            Token = File.ReadAllText(currentDir + path);
        }
    }
}
