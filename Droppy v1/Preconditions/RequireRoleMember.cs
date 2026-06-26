using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class RequireRoleMember : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var guild = (Context as SocketCommandContext).Client.GetGuild(Configurator.GetFromConfig.guildID);
            var user = Context.User as IUser;
            var memberRole = guild.Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.memberRole);

            if (memberRole is null)
                return Task.FromResult(PreconditionResult.FromError($"Role was not found."));
            else if (guild.GetUser(user.Id).Roles.Contains(guild.Roles.FirstOrDefault(x => x.Name == memberRole.Name)))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"You've not `;agree`d to our TOS yet therefore you may not access our server."));
        }
    }
}
