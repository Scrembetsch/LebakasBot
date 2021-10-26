using AmdStockCheck.Service;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Module
{
    [Group("amd")]
    public class AmdStockCheckModule : ModuleBase<ShardedCommandContext>
    {
        private readonly string _Source = "AmdStockMdl";
        public AmdStockCheckService CheckService { get; set; }

        [Command]
        public async Task HelpCommand()
        {
            await ReplyAsync("Usage:\nadd <ProductId>\nremove <ProductId>");
        }

        [Command("add")]
        public Task AddCommand(string productId)
        {
            _ = Logger.LogAsync(new LogMessage(LogSeverity.Info, _Source, $"{Context.Message.Author.Id} add {productId}"));
            _ = Task.Run(async () =>
            {
                AmdStockCheckService.RegisterReturnState ret = await CheckService.RegisterProductAsync(productId, Context.Message.Author.Id);
                await ReplyAsync(Data.PredefinedStrings.GetString(ret));
            });
            return Task.CompletedTask;
        }

        [Command("remove")]
        public Task RemoveCommand(string productId)
        {
            _ = Logger.LogAsync(new LogMessage(LogSeverity.Info, _Source, $"{Context.Message.Author.Id} remove {productId}"));
            _ = Task.Run(async () =>
            {
                AmdStockCheckService.UnregisterReturnState ret = CheckService.UnRegisterProduct(productId, Context.Message.Author.Id);
                await ReplyAsync(Data.PredefinedStrings.GetString(ret));
            });
            return Task.CompletedTask;
        }
    }
}
