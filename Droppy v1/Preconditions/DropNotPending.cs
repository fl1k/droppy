using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Droppy_v1.Preconditions
{
    public class DropNotPending : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            var user = Context.User as IUser;
            UserDataHandler.LoadUser(user.Id);

            if (UserDataHandler.UserDB.DropPending == true)
                return Task.FromResult(PreconditionResult.FromError($"You can't use this command during the afk check process."));
            else if (UserDataHandler.UserDB.DropPending == false)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"Unknown error lol. Please report this to a staff member."));
        }
    }
}
