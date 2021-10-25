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
                        await ReplyAsync("Cannot send you a private message! ADMIN PLZ FIX ┻━┻ ヘ╰( •̀ε•́ ╰)");
                        break;

                    case AmdStockCheckService.RegisterReturnState.UrlCheckFailed:
                        await ReplyAsync("Failed to validate URL! Product Id is probably wrong. 乁| ･ 〰 ･ |ㄏ");
                        break;

                    case AmdStockCheckService.RegisterReturnState.AlreadyRegistered:
                        await ReplyAsync("Product already registered for this User! ། – _ – །");
                        break;

                    case AmdStockCheckService.RegisterReturnState.Ok:
                        await ReplyAsync("Added successfully! ୧༼ ヘ ᗜ ヘ ༽୨");
                        break;

                    case AmdStockCheckService.RegisterReturnState.InternalError:
                        await ReplyAsync("Internal Error! (ʘᗩʘ’)");
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
                AmdStockCheckService.UnregisterReturnState ret = await CheckService.UnRegisterProduct(productId, Context.Message.Author.Id);
                switch (ret)
                {
                    case AmdStockCheckService.UnregisterReturnState.NotRegistered:
                        await ReplyAsync("You are not registered for this product! ୧( ಠ Д ಠ )୨");
                        break;

                    case AmdStockCheckService.UnregisterReturnState.Ok:
                        await ReplyAsync("Removed you from mention list! ᕕ( ཀ ʖ̯ ཀ)ᕗ");
                        break;

                    case AmdStockCheckService.UnregisterReturnState.InternalError:
                        await ReplyAsync("Internal Error! (ʘᗩʘ’)");
                        break;
                }
            });
            return Task.CompletedTask;
        }
    }
}
