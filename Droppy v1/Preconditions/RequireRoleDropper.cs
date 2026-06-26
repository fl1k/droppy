using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class RequireRoleDropper : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var guild = (Context as SocketCommandContext).Client.GetGuild(Configurator.GetFromConfig.guildID);
            var user = Context.User as IUser;
            var dropperRole = guild.Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.dropperRole);

            if (dropperRole is null)
                return Task.FromResult(PreconditionResult.FromError($"Role was not found."));
            else if (guild.GetUser(user.Id).Roles.Contains(guild.Roles.FirstOrDefault(x => x.Name == dropperRole.Name)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"You require {dropperRole.Name} role to do this."));
        }
    }
}
