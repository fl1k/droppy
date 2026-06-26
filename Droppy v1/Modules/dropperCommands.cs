using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using Droppy_v1.Preconditions;
using Droppy_v1.Handlers;

namespace Droppy_v1.Modules
{
    public class dropperCommands : ModuleBase<SocketCommandContext>
    {
        Declarations dec = new Declarations();

        [Command("openlobby")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task OpenLobby()
        {
            if (!File.Exists(dec.lobbiesPath + Context.User.Id))
            {
                await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetTextChannel(Configurator.GetFromConfig.botLogChannelID).SendMessageAsync($"**{Context.User}** has opened a lobby.");
                await Context.User.SendMessageAsync("You've opened a lobby.");
                File.Create(dec.lobbiesPath + Context.User.Id).Close();
            }
            else
            {
                await Context.User.SendMessageAsync("You already have an open lobby.");
            }
        }

        [Command("accept")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task AcceptUsers(int userCount)
        {
            if (!(userCount > UserDataHandler.GetQueueSize() || userCount <= 0))
            {
                if (File.Exists(dec.lobbiesPath + Context.User.Id))
                {
                    List<string> queue = File.ReadAllLines(dec.queuePath).ToList();
                    List<string> FailedAfkCheck = new List<string>();
                    List<string> SucceededAfkCheck = new List<string>();
                    await Context.User.SendMessageAsync($"I'm doing an afk check on the accepted users, will take {Configurator.GetFromConfig.afkCheckTimeMinutes} minutes at most.");
                    List<string> pulledUsers = queue.GetRange(0, userCount);
                    foreach (string userID in pulledUsers)
                    {
                        UserDataHandler.LoadUser(Convert.ToUInt64(userID));
                        await Context.Client.GetUser(Convert.ToUInt64(userID)).SendMessageAsync($"You've been accepted into a lobby. You have {Configurator.GetFromConfig.afkCheckTimeMinutes} minutes to respond with `!here`.");
                        UserDataHandler.UserDB.inQueue = false;
                        UserDataHandler.UserDB.DropPending = true;
                        UserDataHandler.SaveUser(Convert.ToUInt64(userID));
                    }
                    queue.RemoveRange(0, userCount);
                    File.WriteAllLines(dec.queuePath, queue.ToArray());
                    await Task.Delay(TimeSpan.FromMinutes(Configurator.GetFromConfig.afkCheckTimeMinutes));
                    foreach (string userID in pulledUsers)
                    {
                        UserDataHandler.LoadUser(Convert.ToUInt64(userID));
                        UserDataHandler.UserDB.DropPending = false;
                        UserDataHandler.SaveUser(Convert.ToUInt64(userID));
                        if (UserDataHandler.UserDB.AfkCheckResult == true)
                        {
                            using (StreamWriter sw = new StreamWriter(dec.lobbiesPath + Context.User.Id, true))
                            {
                                sw.WriteLine(userID);
                            }
                            await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(Convert.ToUInt64(userID)).AddRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole));
                            await Context.Client.GetUser(Convert.ToUInt64(userID)).SendMessageAsync($"You've been accepted into {Context.User}'s lobby, please do as they say.\nAdd the Dropper who accepted you, their social club username is `{UserDataHandler.GetSCUsername(Context.User.Id)}`");
                            SucceededAfkCheck.Add($"{Context.Client.GetUser(Convert.ToUInt64(userID))} [`{UserDataHandler.GetSCUsername(Convert.ToUInt64(userID))}`]");
                            UserDataHandler.LoadUser(Convert.ToUInt64(userID));
                            UserDataHandler.UserDB.AfkCheckResult = false;
                            UserDataHandler.UserDB.Lobby = Context.User.Id;
                            UserDataHandler.SaveUser(Convert.ToUInt64(userID));
                        }
                        else
                        {
                            await Context.Client.GetUser(Convert.ToUInt64(userID)).SendMessageAsync("You've failed to respond to the AFK check therefore you have not been accepted into a lobby.");
                            FailedAfkCheck.Add($"{Context.Client.GetUser(Convert.ToUInt64(userID))} [`{UserDataHandler.GetSCUsername(Convert.ToUInt64(userID))}`]");
                        }
                    }
                    string msg = $@"**__Accepted users__**
**Completed the afk check({SucceededAfkCheck.Count()})**
{String.Join("\n", SucceededAfkCheck)}

**Failed the afk check({FailedAfkCheck.Count()})**
{String.Join("\n", FailedAfkCheck)}";
                    await Context.User.SendMessageAsync(msg);

                    await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetTextChannel(Configurator.GetFromConfig.botLogChannelID).SendMessageAsync($"**{Context.User}** has accepted {userCount} user(s) into his lobby (Of which {SucceededAfkCheck.Count()} completed the afk check and {FailedAfkCheck.Count()} failed). Number of users in queue is now {UserDataHandler.GetQueueSize()}");
                }
                else
                {
                    await Context.User.SendMessageAsync("You don't have an open lobby.");
                }
            }
            else
            {
                await Context.User.SendMessageAsync("Queue doesn't have that many users.");
            }
        }

        [Command("closelobby")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task CloseLobby()
        {
            if (File.Exists(dec.lobbiesPath + Context.User.Id))
            {
                List<string> users = File.ReadAllLines(dec.lobbiesPath + Context.User.Id).ToList();
                foreach (string userString in users)
                {
                    ulong userID = Convert.ToUInt64(userString);
                    UserDataHandler.SetUserLobby(userID, 0);
                    await Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has closed their lobby so you were automatically removed.");
                    await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(userID).RemoveRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole));
                }
                await Context.User.SendMessageAsync("You've closed your lobby.");
                await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetTextChannel(Configurator.GetFromConfig.botLogChannelID).SendMessageAsync($"**{Context.User}** has closed their lobby.");
                File.Delete(dec.lobbiesPath + Context.User.Id);
            }
            else
            {
                await Context.User.SendMessageAsync("You don't have an open lobby.");
            }
        }

        [Command("qinfo")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task ShowQueue()
        {
            string[] queue = File.ReadAllLines(dec.queuePath);
            List<string> qList = new List<string>();
            qList.Add($"**Users in queue ({queue.Count()}), showing only first 20:**");
            int n = 20;
            if (queue.Length < n)
                n = queue.Length;

            for (int i = 0; i < n; i++)
            {
                qList.Add($"\n**{i + 1}.** {Context.Client.GetUser(Convert.ToUInt64(queue[i]))} [`{UserDataHandler.GetSCUsername(Convert.ToUInt64(queue[i]))}`]");
            }
            await Context.User.SendMessageAsync(String.Join("", qList));

        }

        [Command("lobbyinfo")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task LobbyInfo()
        {
            if (File.Exists(dec.lobbiesPath + Context.User.Id))
            {
                string[] lobby = File.ReadAllLines(dec.lobbiesPath + Context.User.Id);
                List<string> msg = new List<string>();
                msg.Add($"**Users in your lobby:** ({lobby.Length})");
                for (int i = 0; i < lobby.Length; i++)
                {
                    ulong userID = Convert.ToUInt64(lobby[i]);
                    msg.Add($"{i + 1}. {Context.Client.GetUser(userID)} [`{UserDataHandler.GetSCUsername(userID)}`]");
                }
                await Context.User.SendMessageAsync(String.Join("\n", msg));
            }
            else
            {
                await Context.User.SendMessageAsync("You don't have an open lobby.");
            }
        }

        [Command("userinfo")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task UserInfo(ulong userID)
        {
            if (UserDataHandler.IsRegistered(userID))
            {
                var builder = new EmbedBuilder();
                builder.WithTitle($"**{Context.Client.GetUser(userID)}**");
                builder.WithThumbnailUrl(Context.Client.GetUser(userID).GetAvatarUrl());
                builder.WithFooter(Context.Client.GetUser(userID).Id.ToString());
                builder.AddField($"Register date", $"`{UserDataHandler.GetRegisterDate(userID)}`");
                builder.AddField($"Social Club Username", $"`{UserDataHandler.GetSCUsername(Context.Client.GetUser(userID).Id)}`");
                builder.AddField($"Is in Queue?", $"`{UserDataHandler.IsInQueue(Context.Client.GetUser(userID).Id)}`");
                builder.AddField($"Position in queue?", $"`{UserDataHandler.GetQueuePosition(Context.Client.GetUser(userID).Id)}/{UserDataHandler.GetQueueSize()}`");
                builder.WithColor(Color.Green);
                await Context.User.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                await Context.User.SendMessageAsync("User isn't registered.");
            }
        }

        [Command("emptylobby")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task EmptyLobby()
        {
            if (File.Exists(dec.lobbiesPath + Context.User.Id))
            {
                string[] users = File.ReadAllLines(dec.lobbiesPath + Context.User.Id);
                foreach (string userString in users)
                {
                    ulong userID = Convert.ToUInt64(userString);
                    UserDataHandler.SetUserLobby(Convert.ToUInt64(userID), 0);
                    await Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has emptied the lobby you were in therefore you were removed.");
                    await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(userID).RemoveRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole));
                }
                File.Delete(dec.lobbiesPath + Context.User.Id);
                File.Create(dec.lobbiesPath + Context.User.Id).Close();
                await Context.User.SendMessageAsync("You've emptied your lobby.");
                await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetTextChannel(Configurator.GetFromConfig.botLogChannelID).SendMessageAsync($"**{Context.User}** has emptied their lobby.");
            }
            else
            {
                await Context.User.SendMessageAsync("You don't have an open lobby.");
            }
        }

        [Command("lobbykick")]
        [RequireRoleDropper]
        [UserRegistered]
        public async Task LeaveLobby(ulong userID, [Remainder]string reason)
        {
            if (UserDataHandler.IsRegistered(userID))
            {
                UserDataHandler.LoadUser(userID);
                if (UserDataHandler.UserDB.Lobby == Context.User.Id)
                {
                    await Context.User.SendMessageAsync($"You've kicked {Context.Client.GetUser(userID)} from your lobby for **{reason}**.");
                    await Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has kicked you from his lobby for **{reason}**, if you think this wasn't right report him in #helpdesk.");
                    await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(userID).RemoveRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole));
                    await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetTextChannel(Configurator.GetFromConfig.botLogChannelID).SendMessageAsync($"**{Context.User}** has kicked {Context.Client.GetUser(userID)} from their lobby for **{reason}**.");
                    UserDataHandler.RemoveFromLobby(userID);
                }
                else
                {
                    await Context.User.SendMessageAsync($"{Context.Client.GetUser(userID)} doesn't belong in your lobby.");
                }
            }
            else
            {
                await Context.User.SendMessageAsync($"{Context.Client.GetUser(userID)} isn't even registered.");
            }
        }
    }
}