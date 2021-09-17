using AmdStockCheck.Models;
using AmdStockCheck.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace AmdStockCheck.Service
{
    public class AmdStockCheckService
    {
        HttpClient _Client = Web.CreateClient();

        public enum RegisterReturnState
        {
            Ok,
            AlreadyRegistered,
            UrlCheckFailed
        }

        public enum UnregisterReturnState
        {
            Ok,
            NotRegistered
        }

        private readonly List<string> _RegisteredProductIds = new List<string>();
        private readonly Dictionary<string, string> _CheckUrls = new Dictionary<string, string>();
        private readonly Dictionary<string, List<RegisteredUser>> _ProductUserMap = new Dictionary<string, List<RegisteredUser>>();

        private readonly float _CheckInterval = 15.0f;
        private readonly string _BaseCheckUrl = "https://www.amd.com/en/direct-buy/products/{ProductId}/at";

        public AmdStockCheckService()
        {
            // Load lists from config or db
        }

        public async Task<RegisterReturnState> RegisterProductAsync(string productId, ulong guildId, ulong channelId, ulong userId)
        {
            string checkUrl = _BaseCheckUrl.Replace("{productId}", productId);
            if (!await ValidateUrlAsync(checkUrl))
            {
                return RegisterReturnState.UrlCheckFailed;
            }
            if(_RegisteredProductIds.FirstOrDefault(x => x == productId) == null)
            {
                _RegisteredProductIds.Add(productId);
                _ProductUserMap.Add(productId, new List<RegisteredUser>());
                _CheckUrls.Add(productId,checkUrl);
            }
            if(_ProductUserMap[productId].FirstOrDefault(x => x.Equals(guildId, channelId, userId)) == null)
            {
                _ProductUserMap[productId].Add(new RegisteredUser()
                {
                    GuildId = guildId,
                    ChannelId = channelId,
                    UserId = userId
                });
                return RegisterReturnState.Ok;
            }
            else
            {
                return RegisterReturnState.AlreadyRegistered;
            }
        }

        public UnregisterReturnState UnRegisterProduct(string productId, ulong guildId, ulong channelId, ulong userId)
        {
            int index = _RegisteredProductIds.FindIndex(x => x == productId);
            if(index == -1)
            {
                return UnregisterReturnState.NotRegistered;
            }
            int userIndex = _ProductUserMap[productId].FindIndex(x => x.Equals(guildId, channelId, userId));
            if(userIndex == -1)
            {
                return UnregisterReturnState.NotRegistered;
            }
            _ProductUserMap[productId].RemoveAt(userIndex);
            if(_ProductUserMap[productId].Count == 0)
            {
                _ProductUserMap.Remove(productId);
                _CheckUrls.Remove(productId);
                _RegisteredProductIds.RemoveAt(index);
            }
            return UnregisterReturnState.Ok;
        }

        public void Run()
        {
            while (true)
            {
                DateTime begin = DateTime.Now;
                ConsoleWrapper.WriteLine(begin, ConsoleColor.Blue);
                // Send all requests to get product info
                var responseTasks = SendAllRequests(_Client);

                CheckAllRequests(responseTasks);

                DateTime end = DateTime.Now;
                float msSinceStart = (float)(end - begin).TotalMilliseconds;
                int waitTime = (int)(_CheckInterval * 1000.0f - msSinceStart);
                waitTime = Math.Max(0, waitTime);
                Thread.Sleep(waitTime);
            }
        }

        private async Task<bool> ValidateUrlAsync(string url)
        {
            HttpResponseMessage message = await _Client.GetAsync(url);
            return message.IsSuccessStatusCode;
        }

        private List<Task<HttpResponseMessage>> SendAllRequests(HttpClient client)
        {
            List<Task<HttpResponseMessage>> responses = new List<Task<HttpResponseMessage>>();
            foreach (var urlPair in _CheckUrls)
            {
                responses.Add(client.GetAsync(urlPair.Value));
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
                        CheckResponse(responseTasks[i].Result);
                        responseTasks.RemoveAt(i--);
                    }
                }
                // Wait small amount of time to save computation time
                Thread.Sleep(50);
            }
        }

        private void CheckResponse(HttpResponseMessage response)
        {
            int index = FindUrl(response.RequestMessage.RequestUri.ToString());
            if (response.IsSuccessStatusCode)
            {
                if (Check(response))
                {
                    ConsoleWrapper.WriteLine($"{_Alias[index]}: SUCCESS", SuccessColor);
                    OnSuccess(index);
                }
                else
                {
                    ConsoleWrapper.WriteLine($"{_Alias[index]}: Not available", InfoColor);
                    PlaySound(IStockChecker.CheckState.NotAvailable);
                }
            }
            else
            {
                ConsoleWrapper.WriteLine($"{_Alias[index]}: ERROR", ErrorColor);
                PlaySound(IStockChecker.CheckState.RequestError);
            }
        }

        private void OnSuccess(int index)
        {
            if ((DateTime.Now - _LastOpened[index]).TotalSeconds > _SiteOpenThreshold)
            {
                Web.OpenUrl(_OpenUrls[index]);
                PlaySound(IStockChecker.CheckState.InStock);
                _LastOpened[index] = DateTime.Now;
            }
            else
            {
                ConsoleWrapper.WriteLine("Opening site skipped, due to cooldown", NoticeColor);
            }
        }

        private int FindUrl(string url)
        {
            for (int i = 0; i < _CheckUrls.Count; i++)
            {
                if (_CheckUrls[i] == url)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
