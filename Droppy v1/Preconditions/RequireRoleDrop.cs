using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class RequireRoleDrop : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var guild = (Context as SocketCommandContext).Client.GetGuild(Configurator.GetFromConfig.guildID);
            var user = Context.User as IUser;
            var dropRole = guild.Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropRole);

            if (dropRole is null)
                return Task.FromResult(PreconditionResult.FromError($"Role was not found."));
            else if (guild.GetUser(user.Id).Roles.Contains(guild.Roles.FirstOrDefault(x => x.Name == dropRole.Name)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"You're not in a lobby."));
        }
    }
}
