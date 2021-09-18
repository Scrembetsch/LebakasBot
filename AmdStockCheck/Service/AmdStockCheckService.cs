using AmdStockCheck.Models;
using AmdStockCheck.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using GenericUtil.Extensions;
using BackgroundMessageDispatcher;
using System.Configuration;
using Interfaces.Services;

namespace AmdStockCheck.Service
{
    // Todo: Comments
    // Todo: Logs
    // Todo: Cleanup
    public class AmdStockCheckService : IService
    {
        public enum RegisterReturnState
        {
            Ok,
            AlreadyRegistered,
            UrlCheckFailed,
            CannotMessage
        }

        public enum UnregisterReturnState
        {
            Ok,
            NotRegistered
        }

        private float _CheckInterval;
        private string _BaseCheckUrl;
        private string _BaseOpenUrl;
        private string _ErrorUrl;
        private string _ContainString;

        private Task _UrlCheckTask = null;
        private CancellationTokenSource _UrlCheckCancelToken;

        public readonly string _Source = "AmdStockSrvc";
        private readonly Dictionary<string, ProductEntry> _RegisteredProducts = new Dictionary<string, ProductEntry>();


        private Web.StockCheckClient _Client;
        private MessageDispatcher _MessageDispatcher;
        public AmdStockCheckService(IServiceProvider services)
        {
            _Client = services.GetService<Web.StockCheckClient>();
            _MessageDispatcher = services.GetService<MessageDispatcher>();

            // Load lists from config or db

            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Initialized!"));
        }

        public void ApplyConfig()
        {
            _BaseCheckUrl = ConfigurationManager.AppSettings["AmdBaseCheckUrl"];
            _BaseOpenUrl = ConfigurationManager.AppSettings["AmdBaseOpenUrl"];
            _ErrorUrl = ConfigurationManager.AppSettings["AmdErrorUrl"];
            _ContainString = ConfigurationManager.AppSettings["Add to cart"];
            _CheckInterval = float.Parse(ConfigurationManager.AppSettings["AmdCheckInterval"]);

            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Configured!"));
        }

        public async Task<RegisterReturnState> RegisterProductAsync(string productId, ulong userId)
        {
            await Task.Delay(5000);
            bool messageSuccessful = await _MessageDispatcher.SendPrivateMessageAsync("Just checking if I can reach you ԅ( ͒ ۝ ͒ )ᕤ", userId);
            if (!messageSuccessful)
            {
                return RegisterReturnState.CannotMessage;
            }

            string checkUrl = _BaseCheckUrl.Replace("{ProductId}", productId);
            string alias = await ValidateUrlAsync(checkUrl);

            if (string.IsNullOrWhiteSpace(alias))
            {
                return RegisterReturnState.UrlCheckFailed;
            }

            if(!_RegisteredProducts.ContainsKey(productId))
            {
                _RegisteredProducts.Add(productId, new ProductEntry()
                {
                    ProductId = productId,
                    Alias = alias,
                    CheckUrl = checkUrl,
                    Users = new List<RegisteredUser>()
                });
            }

            if(_RegisteredProducts[productId].Users.FirstOrDefault(x => x.Equals(userId)) == null)
            {
                _RegisteredProducts[productId].Users.Add(new RegisteredUser()
                {
                    UserId = userId
                });
            }
            else
            {
                return RegisterReturnState.AlreadyRegistered;
            }

            if(_UrlCheckTask == null)
            {
                _UrlCheckCancelToken = new CancellationTokenSource();
                _UrlCheckTask = Run(_UrlCheckCancelToken.Token);
            }
            return RegisterReturnState.Ok;
        }

        public UnregisterReturnState UnRegisterProduct(string productId, ulong userId)
        {
            if(!_RegisteredProducts.ContainsKey(productId))
            {
                return UnregisterReturnState.NotRegistered;
            }

            int userIndex = _RegisteredProducts[productId].Users.FindIndex(x => x.Equals(userId));
            if(userIndex == -1)
            {
                return UnregisterReturnState.NotRegistered;
            }

            _RegisteredProducts[productId].Users.RemoveAt(userIndex);
            if(_RegisteredProducts[productId].Users.Count == 0)
            {
                _RegisteredProducts.Remove(productId);
            }

            if(_RegisteredProducts.Count == 0)
            {
                _UrlCheckCancelToken.Cancel();
            }

            return UnregisterReturnState.Ok;
        }

        public async Task Run(CancellationToken token)
        {
            // just here to get this method running on background task
            await Task.Delay(100, token);

            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Background Task started!"));

            while (!token.IsCancellationRequested)
            {
                try
                {
                    DateTime begin = DateTime.Now;
                    // Send all requests to get product info
                    var responseTasks = SendAllRequests(_Client);

                    CheckAllRequests(responseTasks);

                    DateTime end = DateTime.Now;
                    float msSinceStart = (float)(end - begin).TotalMilliseconds;
                    int waitTime = (int)(_CheckInterval * 1000.0f - msSinceStart);
                    waitTime = Math.Max(0, waitTime);
                    Thread.Sleep(waitTime);
                }
                catch(Exception e)
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, $"Error while running background task: {e}"));
                    OnError();
                }

            }

            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Background Task stopped!"));
        }

        private async Task<string> ValidateUrlAsync(string url)
        {
            // log and send that something is happening
            HttpResponseMessage message = await _Client.GetAsync(url);
            if (message.IsSuccessStatusCode)
            {
                string content = await message.Content.ReadAsStringAsync();
                string beginString = "<h2>";
                int begin = content.IndexOf(beginString) + beginString.Length;
                int end = content.IndexOf("</h2>", begin);
                return content[begin..end];
            }
            else
            {
                return null;
            }
        }

        private List<Task<HttpResponseMessage>> SendAllRequests(HttpClient client)
        {
            List<Task<HttpResponseMessage>> responses = new List<Task<HttpResponseMessage>>();
            foreach (var products in _RegisteredProducts)
            {
                responses.Add(client.GetAsync(products.Value.CheckUrl));
            }
            return responses;
        }

        private void CheckAllRequests(List<Task<HttpResponseMessage>> responseTasks)
        {
            // Do until all responses are checked
            while (responseTasks.Count > 0)
            {
                for (int i = 0; i < responseTasks.Count; i++)
                {
                    if (responseTasks[i].IsCompleted)
                    {
                        _ = CheckResponse(responseTasks[i].Result);
                        responseTasks.RemoveAt(i--);
                    }
                }
                // Wait small amount of time to save computation time
                Thread.Sleep(50);
            }
        }

        private async Task CheckResponse(HttpResponseMessage response)
        {
            var product = _RegisteredProducts.FirstOrDefault(x => x.Value.CheckUrl == response.RequestMessage.RequestUri.ToString());
            if (response.IsSuccessStatusCode)
            {
                if (await CheckAsync(response))
                {
                    OnSuccess(product.Value);
                }
                else
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Debug, _Source, $"{product.Value.Alias}: Not Available!"));
                }
            }
            else
            {
                // Error usually occurs when page enters queue mode -> Products will become available in a few minutes
                OnError(product.Value);
            }
        }

        public async Task<bool> CheckAsync(HttpResponseMessage response)
        {
            return (await response.Content.ReadAsStringAsync()).Contains(_ContainString);
        }

        private void OnSuccess(ProductEntry product)
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"{product.Alias}: Success!"));

            string openUrl = _BaseOpenUrl.Replace("{ProductId}", product.ProductId);
            string message = $"Available: {product.Alias}\n{openUrl}";
            foreach (var user in product.Users)
            {
                _ = _MessageDispatcher.SendPrivateMessageAsync(message, user.UserId);
            }

        }

        private void OnError(ProductEntry product)
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"{product.Alias}: Page error!"));

            string message = $"Something's Not Quite Right...\n{_ErrorUrl}";
            foreach (var user in product.Users)
            {
                _ = _MessageDispatcher.SendPrivateMessageAsync(message, user.UserId);
            }
        }

        private void OnError()
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"Runner errror!"));

            string message = $"Something's Not Quite Right...\n{_ErrorUrl}";
            HashSet<ulong> allUniqueUsers = new HashSet<ulong>();
            foreach(var item in _RegisteredProducts)
            {
                foreach(var user in item.Value.Users)
                {
                    allUniqueUsers.Add(user.UserId);
                }
            }
            foreach(var userId in allUniqueUsers)
            {
                _ = _MessageDispatcher.SendPrivateMessageAsync(message, userId);
            }
        }
    }
}
