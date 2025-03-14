using CommandSystem;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using SCP008X.Components;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
        public string Command { get; } = "infect";

        public string[] Aliases { get; } = null;

        public string Description { get; }="Forcefully infect a player with SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }
            Player ply = Player.Get(arguments.At(0));
            if(ply == null)
            {
                response = "Invalid player.";
                return false;
            }
            switch (ply.Role.Team)
            {
                case Team.SCPs:
                    response = "You can not infect SCP players.";
                    return false;
                case Team.OtherAlive:
                    response = "You can not infect this class.";
                    return false;
                case Team.Dead:
                    response = "You can not infect the dead.";
                    return false;
            }
            if(ply.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                response = "This player is already infected.";
                return false;
            }
            ply.ReferenceHub.gameObject.AddComponent<Scp008>();
            ply.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
            response = $"{ply.Nickname} has been infected.";
            return true;
        }
    }
}
