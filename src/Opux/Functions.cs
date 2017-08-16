using ByteSizeLib;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LibGit2Sharp;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Opux
{
    internal class Functions
    {
        internal static bool avaliable = false;
        internal static bool running = false;

        //Timer is setup here
        #region Timer stuff
        public async static void RunTick(Object stateInfo)
        {
            try
            {
                if (!running && avaliable)
                {
                    running = true;
                    Async_Tick(stateInfo).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                await Client_Log(new LogMessage(LogSeverity.Error, "Aync_Tick", ex.Message, ex));
            }
        }

        private async static Task Async_Tick(object args)
        {
            try
            {
                running = false;
            }
            catch (Exception ex)
            {
                await Client_Log(new LogMessage(LogSeverity.Error, "Aync_Tick", ex.Message, ex));
                running = false;
            }
        }
        #endregion

        //Needs logging to a file added
        #region Logger
        internal async static Task Client_Log(LogMessage arg)
        {
            try
            {

                var path = Path.Combine(AppContext.BaseDirectory, "logs");
                var file = Path.Combine(path, $"{arg.Source}.log");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(file))
                {
                    File.Create(file);
                }

                var cc = Console.ForegroundColor;

                switch (arg.Severity)
                {
                    case LogSeverity.Critical:
                    case LogSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;

                        break;
                    case LogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogSeverity.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogSeverity.Verbose:
                    case LogSeverity.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                }

                using (StreamWriter logFile = new StreamWriter(File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Write), Encoding.UTF8))
                {
                    await logFile.WriteLineAsync($"{DateTime.Now,-19} [{arg.Severity,8}]: {arg.Message}");
                }

                Console.WriteLine($"{DateTime.Now,-19} [{arg.Severity,8}] [{arg.Source}]: {arg.Message}");
                if (arg.Exception != null)
                {
                    Console.WriteLine(arg.Exception?.StackTrace);
                }
                Console.ForegroundColor = cc;
                await Task.CompletedTask;
            }
            catch { }
        }
        #endregion

        //Events are attached here
        #region EVENTS
        internal async static Task Event_Ready()
        {
            avaliable = true;
            var user = (ISelfUser) Program.Client.CurrentUser;
            await user.ModifyAsync(x => x.Username = Program.Settings.GetSection("config")["name"]);
            await Task.CompletedTask;
        }

        internal static Task Event_Disconnected(Exception arg)
        {
            avaliable = false;
            return Task.CompletedTask;
        }

        internal static Task Event_Connected()
        {
            return Task.CompletedTask;
        }

        internal static Task Event_LoggedIn()
        {
            return Task.CompletedTask;
        }

        internal static Task Event_LoggedOut()
        {
            avaliable = false;
            return Task.CompletedTask;
        }
        #endregion

        //Complete
        #region Pricecheck
        internal async static Task PriceCheck(ICommandContext context, string String, string system)
        {
            var NametoId = "https://www.fuzzwork.co.uk/api/typeid.php?typename=";

            using (HttpClient webClient = new HttpClient())
            {
                JObject jObject = new JObject();
                var channel = (ITextChannel)context.Message.Channel;
                if (String.ToLower() == "short name")
                {
                    String = "Item Name";
                }

                var reply = await webClient.GetStringAsync(NametoId + String);
                jObject = JObject.Parse(reply);
                if ((string)jObject["typeName"] == "bad item")
                {
                    await channel.SendMessageAsync($"{context.Message.Author.Mention} Item {String} does not exist please try again");
                    await Task.CompletedTask;
                }
                else
                {
                    try
                    {
                        if (system == "")
                        {
                            var eveCentralReply = await webClient.GetAsync($"http://api.eve-central.com/api/marketstat/json?typeid={jObject["typeID"]}");
                            var eveCentralReplyString = eveCentralReply.Content;
                            var centralreply = JToken.Parse(await eveCentralReply.Content.ReadAsStringAsync());
                            await Client_Log(new LogMessage(LogSeverity.Info, "PCheck", $"Sending {context.Message.Author}'s Price check to {channel.Name}"));
                            await channel.SendMessageAsync($"{context.Message.Author.Mention}, System: **Universe**{Environment.NewLine}" +
                                $"**Buy:**{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["buy"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["buy"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["buy"]["max"]:n2}```" +
                                $"{Environment.NewLine}" +
                                $"**Sell**:{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["sell"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["sell"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["sell"]["max"]:n2}```");
                        }
                        if (system == "jita")
                        {
                            var eveCentralReply = await webClient.GetAsync($"http://api.eve-central.com/api/marketstat/json?typeid={jObject["typeID"]}&usesystem=30000142");
                            var eveCentralReplyString = eveCentralReply.Content;
                            var centralreply = JToken.Parse(await eveCentralReply.Content.ReadAsStringAsync());
                            await Client_Log(new LogMessage(LogSeverity.Info, "PCheck", $"Sending {context.Message.Author}'s Price check to {channel.Name}"));
                            await channel.SendMessageAsync($"{context.Message.Author.Mention}, System: Jita{Environment.NewLine}" +
                                $"**Buy:**{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["buy"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["buy"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["buy"]["max"]:n2}```" +
                                $"{Environment.NewLine}" +
                                $"**Sell**:{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["sell"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["sell"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["sell"]["max"]:n2}```");
                        }
                        if (system == "amarr")
                        {
                            var eveCentralReply = await webClient.GetAsync($"http://api.eve-central.com/api/marketstat/json?typeid={jObject["typeID"]}&usesystem=30002187");
                            var eveCentralReplyString = eveCentralReply.Content;
                            var centralreply = JToken.Parse(await eveCentralReply.Content.ReadAsStringAsync());
                            await Client_Log(new LogMessage(LogSeverity.Info, "PCheck", $"Sending {context.Message.Author}'s Price check to {channel.Name}"));
                            await channel.SendMessageAsync($"{context.Message.Author.Mention}, System: Amarr{Environment.NewLine}" +
                                $"**Buy:**{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["buy"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["buy"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["buy"]["max"]:n2}```" +
                                $"{Environment.NewLine}" +
                                $"**Sell**:{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["sell"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["sell"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["sell"]["max"]:n2}```");
                        }
                        if (system == "rens")
                        {
                            var eveCentralReply = await webClient.GetAsync($"http://api.eve-central.com/api/marketstat/json?typeid={jObject["typeID"]}&usesystem=30002510");
                            var eveCentralReplyString = eveCentralReply.Content;
                            var centralreply = JToken.Parse(await eveCentralReply.Content.ReadAsStringAsync());
                            await Client_Log(new LogMessage(LogSeverity.Info, "PCheck", $"Sending {context.Message.Author}'s Price check to {channel.Name}"));
                            await channel.SendMessageAsync($"{context.Message.Author.Mention}, System: Rens{Environment.NewLine}" +
                                $"**Buy:**{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["buy"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["buy"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["buy"]["max"]:n2}```" +
                                $"{Environment.NewLine}" +
                                $"**Sell**:{Environment.NewLine}" +
                                $"```Low: {centralreply[0]["sell"]["min"]:n2}{Environment.NewLine}" +
                                $"Avg: {centralreply[0]["sell"]["avg"]:n2}{Environment.NewLine}" +
                                $"High: {centralreply[0]["sell"]["max"]:n2}```");
                        }
                        if (system == "dodixe")
                        {
                            var eveCentralReply = await webClient.GetAsync($"http://api.eve-central.com/api/marketstat/json?typeid={jObject["typeID"]}&usesystem=30002659");
                            var eveCentralReplyString = eveCentralReply.Content;
                            var centralreply = JToken.Parse(await eveCentralReply.Content.ReadAsStringAsync());
                            await Client_Log(new LogMessage(LogSeverity.Info, "PCheck", $"Sending {context.Message.Author}'s Price check to {channel.Name}"));
                            await channel.SendMessageAsync($"{context.Message.Author.Mention}, System: Dodixe{Environment.NewLine}" +
                                $"**Buy:**{Environment.NewLine}" +
                                $"      Low: {centralreply[0]["buy"]["min"]:n}{Environment.NewLine}" +
                                $"      Avg: {centralreply[0]["buy"]["avg"]:n}{Environment.NewLine}" +
                                $"      High: {centralreply[0]["buy"]["max"]:n}{Environment.NewLine}" +
                                $"**Sell**:{Environment.NewLine}" +
                                $"      Low: {centralreply[0]["sell"]["min"]:n}{Environment.NewLine}" +
                                $"      Avg: {centralreply[0]["sell"]["avg"]:n}{Environment.NewLine}" +
                                $"      High: {centralreply[0]["sell"]["max"]:n}{Environment.NewLine}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Client_Log(new LogMessage(LogSeverity.Error, "PC", ex.Message, ex));
                    }
                }
            }
        }
        #endregion

        //About
        #region About
        internal async static Task About(ICommandContext context)
        {
            var directory = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(
                Directory.GetParent(AppContext.BaseDirectory).FullName).FullName).FullName).FullName).FullName);
            //using (var repo = new Repository(directory))
            //{
                var channel = (dynamic)context.Channel;
                var botid = Program.Client.CurrentUser.Id;
                var MemoryUsed = Math.Round(ByteSize.FromBytes(Process.GetCurrentProcess().WorkingSet64).MegaBytes, 2);
                var RunTime = DateTime.Now - Process.GetCurrentProcess().StartTime;
                var Guilds = Program.Client.Guilds.Count;
                var TotalUsers = 0;
                foreach (var guild in Program.Client.Guilds)
                {
                    TotalUsers = guild.Users.Count;
                }

            channel.SendMessageAsync($"{context.User.Mention},{Environment.NewLine}{Environment.NewLine}" +
                $"```Developer: Jimmy06 (In-game Name: Jimmy06){Environment.NewLine}{Environment.NewLine}" +
                $"Bot ID: {botid}{Environment.NewLine}{Environment.NewLine}" +
                //$"Current Version: {repo.Head.Tip.Id}{Environment.NewLine}" +
                //$"Current Branch: {repo.Head.FriendlyName}{Environment.NewLine}" +
                $"Run Time: {RunTime.Days}:{RunTime.Hours}:{RunTime.Minutes}:{RunTime.Seconds}{Environment.NewLine}{Environment.NewLine}" +
                $"Statistics:{Environment.NewLine}" +
                $"Memory Used: {MemoryUsed}MB{Environment.NewLine}" +
                $"Total Connected Guilds: {Guilds}{Environment.NewLine}" +
                $"Total Users Seen: {TotalUsers}```" +
                $"Invite URL: <https://discordapp.com/oauth2/authorize?&client_id=347078401376649216&scope=bot>{Environment.NewLine}");
                    //$"GitHub URL: <{repo.Config.ToList().FirstOrDefault(x => x.Key == "remote.origin.url").Value}>");
            //}

            await Task.CompletedTask;
        }
        #endregion

        //Char
        #region Char
        internal async static Task Char(ICommandContext context, string x)
        {
            var channel = (dynamic)context.Channel;
            using (HttpClient webclient = new HttpClient())
            using (HttpResponseMessage _characterid = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/search/?categories=character&datasource=tranquility&language=en-us&search={x}&strict=false"))
            using (HttpContent _characteridContent = _characterid.Content)
            {
                var id = JObject.Parse(await _characteridContent.ReadAsStringAsync())["character"].FirstOrDefault();
                var _character = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/characters/{id}/?datasource=tranquility");
                var _characterContent = JObject.Parse(await _character.Content.ReadAsStringAsync());
                var _corp = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/corporations/{_characterContent["corporation_id"]}/?datasource=tranquility");
                var _corpContent = JObject.Parse(await _corp.Content.ReadAsStringAsync());
                var _zkill = await webclient.GetAsync($"https://zkillboard.com/api/kills/characterID/{id}/");
                var _zkillContent = JArray.Parse(await _zkill.Content.ReadAsStringAsync())[0];
                var lastSystem = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/universe/systems/{_zkillContent["solarSystemID"]}/?datasource=tranquility&language=en-us");
                var _lastSystem = JObject.Parse(await lastSystem.Content.ReadAsStringAsync());
                var lastShipType = "Unknown";
                if (_zkillContent["victim"]["characterID"] == id)
                {
                    lastShipType = _zkillContent["victim"]["shipTypeID"].ToString();
                }
                else
                {
                    foreach (var attacker in _zkillContent["attackers"])
                    {
                        if ((int)attacker["characterID"] == (int)id)
                        {
                            lastShipType = attacker["shipTypeID"].ToString();
                        }
                    }
                }

                var lastShip = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/universe/types/{lastShipType}/?datasource=tranquility&language=en-us");
                var _lastShip = JObject.Parse(await lastShip.Content.ReadAsStringAsync());
                var _lastSeen = _zkillContent["killTime"];

                var _ally = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/alliances/{_corpContent["alliance_id"]}/?datasource=tranquility");
                var _allyContent = JObject.Parse(await _ally.Content.ReadAsStringAsync());
                await channel.SendMessageAsync($"```Name: {_characterContent["name"]}{Environment.NewLine}" +
                    $"DOB: {_characterContent["birthday"]}{Environment.NewLine}{Environment.NewLine}" +
                    $"Corporation Name: {_corpContent["corporation_name"]}{Environment.NewLine}" +
                    $"Alliance Name: {_allyContent["alliance_name"]}{Environment.NewLine}{Environment.NewLine}" +
                    $"Last System: {_lastSystem["name"]}{Environment.NewLine}" +
                    $"Last Ship: {_lastShip["name"]}{Environment.NewLine}" +
                    $"Last Seen: {_lastSeen}{Environment.NewLine}```" +
                    $"zKill: https://zkillboard.com/character/{id}/");
            }
            await Task.CompletedTask;
        }
        #endregion

        //Corp
        #region Corp
        internal async static Task Corp(ICommandContext context, string x)
        {
            var channel = (dynamic)context.Channel;
            using (HttpClient webclient = new HttpClient())
            using (HttpResponseMessage _characterid = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/search/?categories=corporation&datasource=tranquility&language=en-us&search={x}&strict=false"))
            using (HttpContent _characteridContent = _characterid.Content)
            {
                var _corpContent = JObject.Parse(await _characteridContent.ReadAsStringAsync());
                var _corpDetails = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/corporations/{_corpContent["corporation"][0]}/?datasource=tranquility");
                var _CorpDetailsContent = JObject.Parse(await _corpDetails.Content.ReadAsStringAsync());
                var _CEOName = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/characters/{_CorpDetailsContent["ceo_id"]}/?datasource=tranquility");
                var _CEONameContent = JObject.Parse(await _CEOName.Content.ReadAsStringAsync());
                var _ally = await webclient.GetAsync($"https://esi.tech.ccp.is/latest/alliances/{_CorpDetailsContent["alliance_id"]}/?datasource=tranquility");
                var _allyContent = JObject.Parse(await _ally.Content.ReadAsStringAsync());

                await channel.SendMessageAsync($"```Corp Name: {_CorpDetailsContent["corporation_name"]}{Environment.NewLine}" +
                        $"Corp Ticker: {_CorpDetailsContent["ticker"]}{Environment.NewLine}" +
                        $"CEO: {_CEONameContent["name"]}{Environment.NewLine}" +
                        $"Alliance Name: {_allyContent["alliance_name"]}{Environment.NewLine}" +
                        $"Member Count: {_CorpDetailsContent["member_count"]}{Environment.NewLine}```");
            }
            await Task.CompletedTask;
        }
        #endregion

        //Time
        #region Time
        internal async static Task EveTime(ICommandContext context)
        {
            try
            {
                var format = Program.Settings.GetSection("config")["timeformat"];
                var utcTime = DateTime.UtcNow.ToString(format);
                await context.Message.Channel.SendMessageAsync($"{context.Message.Author.Mention} Current EVE Time is {utcTime}");
            }
            catch (Exception ex)
            {
                await Client_Log(new LogMessage(LogSeverity.Error, "EveTime", ex.Message, ex));
            }
        }
        #endregion

        //Discord Stuff
        #region Discord Modules
        internal static async Task InstallCommands()
        {
            Program.Client.MessageReceived += HandleCommand;
            await Program.Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        internal static async Task HandleCommand(SocketMessage messageParam)
        {

            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix
                    (Program.Client.CurrentUser, ref argPos))) return;

            var context = new CommandContext(Program.Client, message);

            var result = await Program.Commands.ExecuteAsync(context, argPos, Program.ServiceCollection);
            if (!result.IsSuccess && result.ErrorReason == "Unknown command.")
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
        #endregion

        //Complete
        #region MysqlQuery
        internal static async Task<IList<IDictionary<string, object>>> MysqlQuery(string connstring, string query)
        {
            using (MySqlConnection conn = new MySqlConnection(connstring))
            {
                MySqlCommand cmd = conn.CreateCommand();
                List<IDictionary<string, object>> list = new List<IDictionary<string, object>>(); ;
                cmd.CommandText = query;
                try
                {
                    conn.ConnectionString = connstring;
                    conn.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var record = new Dictionary<string, object>();

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var key = reader.GetName(i);
                            var value = reader[i];
                            record.Add(key, value);
                        }

                        list.Add(record);
                    }

                    return list;
                }
                catch (MySqlException ex)
                {
                    await Client_Log(new LogMessage(LogSeverity.Error, "mySQL", query + " " + ex.Message, ex));
                }
                await Task.Yield();
                return list;
            }
        }
        #endregion

        //SQLite Query
        #region SQLiteQuery
        internal async static Task<string> SQLiteDataQuery(string table, string field, string name)
        {
            using (SqliteConnection con = new SqliteConnection("Data Source = Opux.db;"))
            using (SqliteCommand querySQL = new SqliteCommand($"SELECT {field} FROM {table} WHERE name = @name", con))
            {
                await con.OpenAsync();
                querySQL.Parameters.Add(new SqliteParameter("@name", name));
                try
                {
                    using (SqliteDataReader r = await querySQL.ExecuteReaderAsync())
                    {
                        var result = await r.ReadAsync();
                        return r.GetString(0) ?? "";
                    }
                }
                catch (Exception ex)
                {
                    await Client_Log(new LogMessage(LogSeverity.Error, "SQLite", ex.Message, ex));
                    return null;
                }
            }
        }
        internal async static Task<List<int>> SQLiteDataQuery(string table)
        {
            using (SqliteConnection con = new SqliteConnection("Data Source = Opux.db;"))
            using (SqliteCommand querySQL = new SqliteCommand($"SELECT * FROM {table}", con))
            {
                await con.OpenAsync();
                try
                {
                    using (SqliteDataReader r = await querySQL.ExecuteReaderAsync())
                    {
                        var list = new List<int>();
                        while (await r.ReadAsync())
                        {
                            list.Add(Convert.ToInt32(r["Id"]));
                        }

                        return list;
                    }
                }
                catch (Exception ex)
                {
                    await Client_Log(new LogMessage(LogSeverity.Error, "SQLite", ex.Message, ex));
                    return null;
                }
            }
        }
        #endregion

        //SQLite Update
        #region SQLiteQuery
        internal async static Task SQLiteDataUpdate(string table, string field, string name, string data)
        {
            using (SqliteConnection con = new SqliteConnection("Data Source = Opux.db;"))
            using (SqliteCommand insertSQL = new SqliteCommand($"UPDATE {table} SET {field} = @data WHERE name = @name", con))
            {
                await con.OpenAsync();
                insertSQL.Parameters.Add(new SqliteParameter("@name", name));
                insertSQL.Parameters.Add(new SqliteParameter("@data", data));
                try
                {
                    insertSQL.ExecuteNonQuery();
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    await Client_Log(new LogMessage(LogSeverity.Error, "SQLite", ex.Message, ex));
                }
            }
        }
        #endregion

        //SQLite Delete
        #region SQLiteDelete
        internal async static Task SQLiteDataDelete(string table, string name)
        {
            using (SqliteConnection con = new SqliteConnection("Data Source = Opux.db;"))
            using (SqliteCommand insertSQL = new SqliteCommand($"REMOVE FROM {table} WHERE name = @name", con))
            {
                await con.OpenAsync();
                insertSQL.Parameters.Add(new SqliteParameter("@name", name));
                try
                {
                    insertSQL.ExecuteNonQuery();
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    await Client_Log(new LogMessage(LogSeverity.Error, "SQLite", ex.Message, ex));
                }
            }
        }
        #endregion

        //StripHTML Tags From string
        #region StripHTML
        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
        #endregion

        }

    #region JToken null/empty check
    internal static class JsonExtensions
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
    #endregion

}
