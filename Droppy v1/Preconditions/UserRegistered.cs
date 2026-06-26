using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class UserRegistered : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var guild = (Context as SocketCommandContext).Client.GetGuild(Configurator.GetFromConfig.guildID);
            var user = Context.User as IUser;

            if (UserDataHandler.IsRegistered(user.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"You've not registered yet. Use `!register [Social club username]` to register. Example - `register droppy420`"));
        }
    }
}
