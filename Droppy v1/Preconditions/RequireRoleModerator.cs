using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class RequireRoleModerator : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var guild = (Context as SocketCommandContext).Client.GetGuild(Configurator.GetFromConfig.guildID);
            var user = Context.User as IUser;
            var moderatorRole = guild.Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.moderatorRole);

            if (moderatorRole is null)
                return Task.FromResult(PreconditionResult.FromError($"Role was not found."));
            else if (guild.GetUser(user.Id).Roles.Contains(guild.Roles.FirstOrDefault(x => x.Name == moderatorRole.Name)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"You require {moderatorRole.Name} role to do this."));
        }
    }
}
