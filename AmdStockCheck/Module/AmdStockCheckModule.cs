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
                switch (ret)
                {
                    case AmdStockCheckService.RegisterReturnState.CannotMessage:
                        await ReplyAsync(Data.PredefinedStrings.cAdd_CannotMessage);
                        break;

                    case AmdStockCheckService.RegisterReturnState.UrlCheckFailed:
                        await ReplyAsync(Data.PredefinedStrings.cAdd_UrlCheckFailed);
                        break;

                    case AmdStockCheckService.RegisterReturnState.AlreadyRegistered:
                        await ReplyAsync(Data.PredefinedStrings.cAdd_AlreadyRegisterd);
                        break;

                    case AmdStockCheckService.RegisterReturnState.Ok:
                        await ReplyAsync(Data.PredefinedStrings.cAdd_Ok);
                        break;

                    case AmdStockCheckService.RegisterReturnState.InternalError:
                        await ReplyAsync(Data.PredefinedStrings.cGeneral_InternalError);
                        break;
                }
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
                switch (ret)
                {
                    case AmdStockCheckService.UnregisterReturnState.NotRegistered:
                        await ReplyAsync(Data.PredefinedStrings.cRemove_NotRegistered);
                        break;

                    case AmdStockCheckService.UnregisterReturnState.Ok:
                        await ReplyAsync(Data.PredefinedStrings.cRemove_Ok);
                        break;

                    case AmdStockCheckService.UnregisterReturnState.InternalError:
                        await ReplyAsync(Data.PredefinedStrings.cGeneral_InternalError);
                        break;
                }
            });
            return Task.CompletedTask;
        }
    }
}
