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
        public class StockCheckClient : HttpClient
        {
            public StockCheckClient() : base()
            {
            }

            public StockCheckClient(HttpMessageHandler handler) : base(handler)
            {
            }

            public StockCheckClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
            {
            }
        }

        public static StockCheckClient CreateClient()
        {
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            };
            StockCheckClient client = new StockCheckClient(clientHandler);

            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            return client;
        }
    }
}
