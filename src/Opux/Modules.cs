using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Opux
{
    // Create a module with no prefix
    public partial class Info : ModuleBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("help", RunMode = RunMode.Async), Summary("Reports help text.")]
        public async Task Help()
        {
            var userInfo = Context.Message.Author;
            await ReplyAsync($"{userInfo.Mention}, Here is a list of plugins available, **!about | !help | !jita | !amarr | !dodixe | !rens | !pc | !evetime**");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("pc", RunMode = RunMode.Async), Summary("Performs Prices Checks Example: !pc Tritanium")]
        public async Task Pc([Remainder] string x)
        {
            var userInfo = Context.Message.Author;
            if (x == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention} please provide an item name");
            }
            else
            {
                await Functions.PriceCheck(Context, x, "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("jita", RunMode = RunMode.Async), Summary("Performs Prices Checks Example: !jita Tritanium")]
        public async Task Jita([Remainder] string x)
        {
            var userInfo = Context.Message.Author;
            if (x == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention} please provide an item name");
            }
            else
            {
                await Functions.PriceCheck(Context, x, "jita");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("amarr", RunMode = RunMode.Async), Summary("Performs Prices Checks Example: !pc Tritanium")]
        public async Task Amarr([Remainder] string x)
        {
            var userInfo = Context.Message.Author;
            if (x == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention} please provide an item name");
            }
            else
            {
                await Functions.PriceCheck(Context, x, "amarr");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("rens", RunMode = RunMode.Async), Summary("Performs Prices Checks Example: !pc Tritanium")]
        public async Task Rens([Remainder] string x)
        {
            var userInfo = Context.Message.Author;
            if (x == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention} please provide an item name");
            }
            else
            {
                await Functions.PriceCheck(Context, x, "rens");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("dodixe", RunMode = RunMode.Async), Summary("Performs Prices Checks Example: !pc Tritanium")]
        public async Task Dodixe([Remainder] string x)
        {
            var userInfo = Context.Message.Author;
            if (x == null)
            {
                await ReplyAsync($"{Context.Message.Author.Mention} please provide an item name");
            }
            else
            {
                await Functions.PriceCheck(Context, x, "dodixe");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("evetime", RunMode = RunMode.Async), Summary("EVE TQ Time")]
        public async Task EveTime()
        {
            if (Convert.ToBoolean(Program.Settings.GetSection("config")["time"]))
            {
                try
                {
                    await Functions.EveTime(Context);
                }
                catch (Exception ex)
                {
                    await Functions.Client_Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Modules", ex.Message, ex));
                    await Task.FromException(ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("about", RunMode = RunMode.Async), Summary("About Opux")]
        public async Task About()
        {
            try
            {
                await Functions.About(Context);
            }
            catch (Exception ex)
            {
                await Functions.Client_Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Modules", ex.Message, ex));
                await Task.FromException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("char", RunMode = RunMode.Async), Summary("Character Details")]
        public async Task Char([Remainder] string x)
        {
            try
            {
                await Functions.Char(Context, x);
            }
            catch (Exception ex)
            {
                await Functions.Client_Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Modules", ex.Message, ex));
                await Task.FromException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Command("corp", RunMode = RunMode.Async), Summary("Corporation Details")]
        public async Task Corp([Remainder] string x)
        {
            try
            {
                await Functions.Corp(Context, x);
            }
            catch (Exception ex)
            {
                await Functions.Client_Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Modules", ex.Message, ex));
                await Task.FromException(ex);
            }
        }
    }

}
