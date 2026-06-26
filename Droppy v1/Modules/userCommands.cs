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
    public class userCommands : ModuleBase<SocketCommandContext>
    {
        Declarations dec = new Declarations();

        [Command("register")]
        [RequireRoleMember]
        public async Task Register(string SCUsername)
        {
            await Context.User.SendMessageAsync(UserDataHandler.userRegister(Context.User.Id, SCUsername));
        }

        [Command("qjoin")]
        [NotInDrop]
        [UserRegistered]
        [DropNotPending]
        public async Task JoinQueue()
        {
            if (!UserDataHandler.IsInQueue(Context.User.Id))
            {
                if (UserDataHandler.GetQueueSize() < Configurator.GetFromConfig.queueMax)
                {
                    using (StreamWriter sw = new StreamWriter(dec.queuePath, true))
                    {
                        sw.WriteLine(Context.User.Id);
                    }
                    UserDataHandler.LoadUser(Context.User.Id);
                    UserDataHandler.UserDB.inQueue = true;
                    UserDataHandler.SaveUser(Context.User.Id);
                    await Context.User.SendMessageAsync($"You've been added into the queue, your position is **[{UserDataHandler.GetQueueSize()}/{UserDataHandler.GetQueueSize()}]**, (`!qpos` to update)");
                }
                else
                {
                    await Context.User.SendMessageAsync("Queue is already full, please wait until a Dropper accepts some people.");
                }
            }
            else
            {
                await Context.User.SendMessageAsync("You're already in the queue.");
            }
        }

        [Command("qpos")]
        [UserRegistered]
        [DropNotPending]
        [NotInDrop]
        public async Task QueuePosition()
        {
            if (UserDataHandler.IsInQueue(Context.User.Id))
            {
                await Context.User.SendMessageAsync($"Your position in the queue is **[{UserDataHandler.GetQueuePosition(Context.User.Id)}/{UserDataHandler.GetQueueSize()}]**");
            }
            else
            {
                await Context.User.SendMessageAsync("You're not in the queue. Do `!qjoin` to enter the queue.");
            }
        }

        [Command("qleave")]
        [UserRegistered]
        [DropNotPending]
        [NotInDrop]
        public async Task QueueLeave()
        {
            if (UserDataHandler.IsInQueue(Context.User.Id))
            {
                UserDataHandler.RemoveFromQueue(Context.User.Id);
                await Context.User.SendMessageAsync("You've left the queue.");
            }
            else
            {
                await Context.User.SendMessageAsync("You're not in the queue.");
            }
        }

        [Command("script")]
        [UserRegistered]
        public async Task GetScript()
        {
            List<string> msg = new List<string>();
            msg.Add("**Description**\n- This script will make you run in circles, so you can be afk while the drop is happening and still get the all money and not get kicked\n");
            msg.Add("**Step 1:** Go to https://autohotkey.com/download/ and download \"Windows Installer\" and install it");
            msg.Add("**Step 2:** Download the circles.ahk script from below");
            msg.Add("**Step 3:** Run the script during the drop, to toggle it on & off press Shift + F3 simultaneously, for best cash make sure you sprint while running in circles");
            msg.Add("\nThat's it, if you need any assistance although everything is pretty clear and simple ask in #general");
            await Context.User.SendMessageAsync(String.Join("\n", msg));
            await Context.Channel.SendFileAsync(@"data\files\circles.ahk");
        }

        [Command("openlobbies")]
        [UserRegistered]
        public async Task OpenLobbies()
        {
            List<string> msg = new List<string>();
            ulong[] lobbies = UserDataHandler.GetOpenLobbies();
            bool AnyOpenLobbies = lobbies.Length > 0;
            if (AnyOpenLobbies)
            {
                msg.Add("__**Currently open lobbies:**__");
                for (int i = 0; i < lobbies.Length; i++)
                {
                    msg.Add($"- **{Context.Client.GetUser(lobbies[i])}**'s lobby ({UserDataHandler.GetLobbySize(lobbies[i])} dropee(s) inside)");
                }
            }
            else
            {
                msg.Add("Currently there are no open lobbies.");
            }
            await Context.User.SendMessageAsync(String.Join("\n", msg));
        }

        [Command("lobbyleave")]
        [RequireRoleDrop]
        public async Task LeaveLobby()
        {
            UserDataHandler.LoadUser(Context.User.Id);
            await Context.User.SendMessageAsync($"You've left {Context.Client.GetUser(UserDataHandler.UserDB.Lobby)}'s lobby.");
            await Context.Client.GetUser(UserDataHandler.UserDB.Lobby).SendMessageAsync($"{Context.User} has left your lobby, you may kick him in-game.");
            await Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(Context.User.Id).RemoveRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole));
            UserDataHandler.RemoveFromLobby(Context.User.Id);
        }

        [Command("here")]
        [UserRegistered]
        [DropPending]
        public async Task AfkCheck()
        {
            UserDataHandler.LoadUser(Context.User.Id);
            UserDataHandler.UserDB.AfkCheckResult = true;
            UserDataHandler.SaveUser(Context.User.Id);
            await Context.User.SendMessageAsync("You've confirmed that you're not afk, please wait for other users or time to expire.");
        }

        [Command("help")]
        [UserRegistered]
        public async Task HelpMessage()
        {
            string msg = "__**User commands**__\n**!register [SC Username]**\n- To register a SC Username, (Example use - `!register Droppy12`)\n**!qjoin**\n- To join the queue\n**!qpos**\n- To see your current position in queue \n**!here**\n- To prove you're not AFK, you'll be asked to do it once you're accepted\n**!qleave**\n- To leave the queue\n**!script**\n- To get a message containing a step-by-step guide for using a circle running script\n**!openlobbies**\n- To see all currently open lobbies \n**!lobbyleave**\n- To leave the drop lobby when you're in it\n**!usercount**\n- To get the top usercount for this server";
            msg += "\n\nFor dropper commands - `!help dropper`, for staff `!help staff`";
            msg += $"\n`{dec.droppyVersion} - report bugs in #helpdesk`";
            await Context.User.SendMessageAsync(msg);
        }

        [Command("help")]
        [UserRegistered]
        public async Task HelpMessage(string helpType)
        {
            string msg = String.Empty;
            if (helpType == "dropper")
            {
                msg += "__**Dropper commands**__";
                msg += $"\n**!openlobby**\n[{Configurator.GetFromConfig.dropperRole}+] - To open a lobby";
                msg += $"\n**!accept [number of users]**\n[{Configurator.GetFromConfig.dropperRole}+] - To accept users into your lobby (Example use - `!accept 5`)";
                msg += $"\n**!emptylobby**\n[{Configurator.GetFromConfig.dropperRole}+] - To empty your lobby of users";
                msg += $"\n**!closelobby**\n[{Configurator.GetFromConfig.dropperRole}+] - To close your lobby";
                msg += $"\n**!userinfo [userID]**\n[{Configurator.GetFromConfig.dropperRole}+] - To see some information about the user (Example use - `!userinfo {Context.User.Id})`";
                msg += $"\n**!lobbyinfo**\n[{Configurator.GetFromConfig.dropperRole}+] - To see who's in your lobby";
                msg += $"\n**!lobbykick [userID] [reason]**\n[{Configurator.GetFromConfig.dropperRole}+] - To kick a user from your lobby (Example use - `!lobbykick {Context.User.Id} killing other dropees`)";
                msg += $"\n\nFor user commands - `!help`, for staff `!help staff`";
            }
            else if (helpType == "staff")
            {
                msg += "__**Staff commands**__";
                msg += $"\n**!forcecloselobby [userID]**\n[{Configurator.GetFromConfig.moderatorRole}+] - To forcefully close dropper's lobby (Example use - `!forcecloselobby {Context.User.Id})`";
                msg += $"\n**!qinfo**\n[{Configurator.GetFromConfig.moderatorRole}+] - To see first 20 members in queue";
                msg += $"\n**!qadd [userID]**\n[{Configurator.GetFromConfig.moderatorRole}+] - To add an user into the queue (Example use - `!qadd {Context.User.Id}`)";
                msg += $"\n**!unregister [userID]**\n[{Configurator.GetFromConfig.moderatorRole}+] - To unregister an user (Example use - `!unregister {Context.User.Id}`)";
                msg += $"\n**!qremove [userID]**\n[{Configurator.GetFromConfig.moderatorRole}+] - To remove an user from the queue (Example use - `!qremove {Context.User.Id}`)";
                msg += $"\n\nFor user commands - `!help`, for dropper `!help dropper`";
            }
            else
            {
                await Context.User.SendMessageAsync("Invalid command argument, `!help` | `!help dropper` | `!help staff`");
                return;
            }
            msg += $"\n**`{dec.droppyVersion} - report bugs in #helpdesk`**";
            await Context.User.SendMessageAsync(msg);
        }

        [Command("usercount")]
        [UserRegistered]
        public async Task UserCount()
        {
            await Context.User.SendMessageAsync($"**Current:** `Invalid guild id specified... message the dev to tell him he's retarded`\n**Top:** `4369` [{Convert.ToDateTime("2018-08-26T20:10:54.1162933+02:00")}] :heart:");
        }
    }
}
