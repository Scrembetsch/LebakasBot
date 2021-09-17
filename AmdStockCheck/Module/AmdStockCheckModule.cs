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
        public AmdStockCheckService CheckService { get; set; }

        [Command]
        public async Task HelpCommand()
        {
            await ReplyAsync("Usage: ");
        }

        [Command("add")]
        public async Task AddCommand(string productId)
        {
            AmdStockCheckService.RegisterReturnState ret = await CheckService.RegisterProductAsync(productId, Context.Guild.Id, Context.Channel.Id, Context.Message.Author.Id);
            switch (ret)
            {
                case AmdStockCheckService.RegisterReturnState.UrlCheckFailed:
                    await ReplyAsync("Failed to validate URL!");
                    return;

                case AmdStockCheckService.RegisterReturnState.AlreadyRegistered:
                    await ReplyAsync("Product already registered for this User in this Guild & Channel!");
                    return;

                case AmdStockCheckService.RegisterReturnState.Ok:
                    await ReplyAsync("Added successfully!");
                    return;
            }
        }

        [Command("remove")]
        public async Task RemoveCommand(string productId)
        {

        }
    }
}
