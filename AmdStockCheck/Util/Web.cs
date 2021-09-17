using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Util
{
    public static class Web
    {
        public static HttpClient CreateClient()
        {
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            };
            HttpClient client = new HttpClient(clientHandler);

            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            return client;
        }

        public static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = url
                }
            );
        }
    }
}
