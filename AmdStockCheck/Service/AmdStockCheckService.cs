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
using AmdStockCheck.DataAccess;

namespace AmdStockCheck.Service
{
    public class AmdStockCheckService
    {
        public enum RegisterReturnState
        {
            Ok,
            AlreadyRegistered,
            UrlCheckFailed,
            CannotMessage,
            InternalError
        }

        public enum UnregisterReturnState
        {
            Ok,
            NotRegistered,
            InternalError
        }

        private float _CheckInterval;
        private string _BaseCheckUrl;
        private string _BaseOpenUrl;
        private string _ErrorUrl;
        private string _ContainString;
        private string _InQueueString;

        private Task _UrlCheckTask = null;
        private CancellationTokenSource _UrlCheckCancelToken;

        public readonly string _Source = "AmdStockSrvc";

        private readonly Web.StockCheckClient _Client;
        private readonly MessageDispatcher _MessageDispatcher;
        private readonly AmdDatabaseService _AmdDbService;

        public AmdStockCheckService(IServiceProvider services)
        {
            _Client = services.GetService<Web.StockCheckClient>();
            _MessageDispatcher = services.GetService<MessageDispatcher>();
            _AmdDbService = services.GetService<AmdDatabaseService>();

            ApplyConfig();

            if(_AmdDbService.GetAllRegisteredProducts().Count > 0)
            {
                _UrlCheckCancelToken = new CancellationTokenSource();
                _UrlCheckTask = Run(_UrlCheckCancelToken.Token);
            }

            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, "Initialized!"));
        }

        private void ApplyConfig()
        {
            _BaseCheckUrl = ConfigurationManager.AppSettings["AmdBaseCheckUrl"];
            _BaseOpenUrl = ConfigurationManager.AppSettings["AmdBaseOpenUrl"];
            _ErrorUrl = ConfigurationManager.AppSettings["AmdErrorUrl"];
            _ContainString = ConfigurationManager.AppSettings["AmdCheckString"];
            _InQueueString = ConfigurationManager.AppSettings["AmdInQueueCheckString"];
            _CheckInterval = float.Parse(ConfigurationManager.AppSettings["AmdCheckInterval"]);
        }

        public async Task<RegisterReturnState> RegisterProductAsync(string productId, ulong userId)
        {
            try
            {
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

                Product product = _AmdDbService.GetProductById(productId);
                if (product == null)
                {
                    product = new Product()
                    {
                        ProductId = productId,
                        Alias = alias,
                        CheckUrl = checkUrl,
                        Users = null
                    };

                    _AmdDbService.AddNewProduct(product);
                }

                // User can default to null, because of db service
                if (product.Users == null
                    || product.Users.FirstOrDefault(x => x.UserId == userId) == null)
                {
                    bool success = _AmdDbService.AddUserToProduct(product, new User()
                    {
                        UserId = userId
                    });
                    if (!success)
                    {
                        return RegisterReturnState.InternalError;
                    }
                }
                else
                {
                    return RegisterReturnState.AlreadyRegistered;
                }

                if (_UrlCheckTask == null)
                {
                    _UrlCheckCancelToken = new CancellationTokenSource();
                    _UrlCheckTask = Run(_UrlCheckCancelToken.Token);
                }
                return RegisterReturnState.Ok;
            }
            catch(Exception e)
            {
                _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
            }
            return RegisterReturnState.InternalError;
        }

        public UnregisterReturnState UnRegisterProduct(string productId, ulong userId)
        {
            try
            {
                Product product = _AmdDbService.GetProductById(productId);
                if (product == null)
                {
                    return UnregisterReturnState.NotRegistered;
                }

                User user = product.Users.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                {
                    return UnregisterReturnState.NotRegistered;
                }

                bool success = _AmdDbService.RemoveUser(product, user);
                if (!success)
                {
                    return UnregisterReturnState.InternalError;
                }

                if (product.Users.Count == 0)
                {
                    _AmdDbService.RemoveProduct(product);
                }

                if (_AmdDbService.GetAllRegisteredProducts().Count == 0)
                {
                    _UrlCheckCancelToken.Cancel();
                }

                return UnregisterReturnState.Ok;
            }
            catch(Exception e)
            {
                _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, _Source, e.Message, e));
            }
            return UnregisterReturnState.InternalError;
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
                    if(e is not TaskCanceledException)
                    {
                        OnError();
                    }
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
            List<Product> products = _AmdDbService.GetAllRegisteredProducts();

            foreach (var product in products)
            {
                responses.Add(client.GetAsync(product.CheckUrl));
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
            Product product = _AmdDbService.GetProductByCheckUrl(response.RequestMessage.RequestUri.ToString());
            if (response.IsSuccessStatusCode && product != null)
            {
                if (await CheckAvailableAsync(response))
                {
                    OnSuccess(product);
                }
                else if(await CheckInQueueAsync(response))
                {
                    OnInQueue(product);
                }
                else
                {
                    _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Debug, _Source, $"{product.Alias}: Not Available!"));
                }
            }
            else
            {
                // Error usually occurs when page enters queue mode -> Products will become available in a few minutes
                OnError();
            }
        }

        public async Task<bool> CheckAvailableAsync(HttpResponseMessage response)
        {
            return (await response.Content.ReadAsStringAsync()).Contains(_ContainString);
        }

        public async Task<bool> CheckInQueueAsync(HttpResponseMessage response)
        {
            return (await response.Content.ReadAsStringAsync()).Contains(_InQueueString);
        }

        private void OnSuccess(Product product)
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"{product.Alias}: Success!"));

            string openUrl = _BaseOpenUrl.Replace("{ProductId}", product.ProductId);
            string message = Data.PredefinedStrings.cService_Available.ReplaceId(0, product.Alias).ReplaceId(1, openUrl);

            foreach (var user in product.Users)
            {
                _ = _MessageDispatcher.SendPrivateMessageAsync(message, user.UserId);
            }
        }

        private void OnInQueue(Product product)
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"{product.Alias}: Success!"));

            string openUrl = _BaseOpenUrl.Replace("{ProductId}", product.ProductId);
            string message = Data.PredefinedStrings.cService_QueueStarted.ReplaceId(0, product.Alias).ReplaceId(1, openUrl);

            foreach (var user in product.Users)
            {
                _ = _MessageDispatcher.SendPrivateMessageAsync(message, user.UserId);
            }
        }

        private void OnError()
        {
            _ = Logger.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Info, _Source, $"Runner error!"));

            string message = Data.PredefinedStrings.cService_RequestError.ReplaceId(0,_ErrorUrl);

            HashSet<ulong> allUniqueUsers = new HashSet<ulong>();
            List<Product> products = _AmdDbService.GetAllRegisteredProducts();
            foreach(var item in products)
            {
                foreach(var user in item.Users)
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
