using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace LebakasBot
{
    public class TokenManager
    {
        public string Token { get; }

        public TokenManager(string path = "token.txt")
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Token = reader.ReadLine();
            }
        }
    }
}
