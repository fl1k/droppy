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
    public class staffCommands : ModuleBase<SocketCommandContext>
    {
        Declarations dec = new Declarations();
        logHandler logHandler = new logHandler();

        [Command("qadd")]
        [RequireRoleDropper]
        public async Task Qadd(ulong userID)
        {
            if (!UserDataHandler.IsInQueue(userID))
            {
                if (UserDataHandler.IsRegistered(userID))
                {
                    using (StreamWriter sw = new StreamWriter(dec.queuePath, true))
                    {
                        sw.WriteLine(userID);
                        await Context.User.SendMessageAsync($"You've added {Context.Client.GetUser(userID)} into the queue.");
                        await (Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has added you to queue."));
                    }
                }
                else
                {
                    await Context.User.SendMessageAsync("User isn't registered.");
                }
            }
            else
            {
                await Context.User.SendMessageAsync("User is already in the queue.");
            }
        }

        [Command("qremove")]
        [RequireRoleModerator]
        public async Task RemoveUserFromQueue(ulong userID)
        {
            if (UserDataHandler.IsInQueue(userID))
            {
                UserDataHandler.RemoveFromQueue(userID);
                await Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has removed you from the queue. If you think this wasn't right report him to the higher level staff.");
                await Context.User.SendMessageAsync($"You've removed {Context.Client.GetUser(userID)} from the queue");
                logHandler.WriteLine($"[{DateTime.Now}] [Info] {Context.User} has removed {Context.Client.GetUser(userID)}({userID}) from the queue.");
            }
            else
            {
                await Context.User.SendMessageAsync("User is not in the queue.");
            }
        }

        [Command("unregister")]
        [RequireRoleModerator]
        public async Task Unregister(ulong userID)
        {
            if (UserDataHandler.IsRegistered(userID))
            {
                UserDataHandler.Unregister(userID);
                UserDataHandler.RemoveFromQueue(userID);
                await Context.Client.GetUser(userID).SendMessageAsync($"{Context.User} has unregistered you. If you think this wasn't right report him to the higher level staff.");
                await Context.User.SendMessageAsync($"You've unregistered {Context.Client.GetUser(userID)}.");
                logHandler.WriteLine($"[{DateTime.Now}] [Info] {Context.User} has unregistered {Context.Client.GetUser(userID)}({userID}).");
            }
            else
            {
                await Context.User.SendMessageAsync("User is not registered.");
            }
        }
    }
}
