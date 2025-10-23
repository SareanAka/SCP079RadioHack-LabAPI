using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace SCP079RadioHack
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SCP079HackCommandParent : ParentCommand
    {
        public SCP079HackCommandParent() => LoadGeneratedCommands();

        public override string Command => "scp079radio";

        public override string[] Aliases { get; } = { "radio079" };

        public override string Description => "Base Command for the SCP079 MTF radio hack plugin";

        public sealed override void LoadGeneratedCommands()
        {

            RegisterCommand(new SCP079RadioHackCommand());
            RegisterCommand(new SCP079RadioProxy());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);

            if(playerSender == null)
            {
                response = "This command can only be used by players.";
                return false;
            }

            if (!Round.IsRoundStarted)
            {
                response = "The round hasn't started yet";
                return false;
            }

            if (playerSender.Role != RoleTypeId.Scp079)
            {
                response = "You must be SCP-079 to use this command.";
                return false;
            }

            response = "You must specify a subcommand (eg. hack or proxy).";
            return false;
        }
    }

    public class SCP079RadioHackCommand : ICommand
    {
        public string Command => "hack";
        public string[] Aliases => new[] { "h" }; // helpful alias
        public string Description => "Toggles SCP-079’s ability to speak through the MTF radio channel.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);

            if (player == null)
            {
                response = "This command can only be used by players.";
                return false;
            }

            if (!Round.IsRoundStarted)
            {
                response = "The round hasn't started yet";
                return false;
            }

            if (player.Role != RoleTypeId.Scp079)
            {
                response = "You must be SCP-079 to use this command.";
                return false;
            }

            SCP079RadioHack.Toggle(player);
            response = "Toggled SCP-079’s ability to speak through the MTF radio channel.";
            return true;
        }
    }

    public class SCP079RadioProxy : ICommand
    {
        public string Command => "proxy";
        public string[] Aliases => new[] { "p" }; // helpful alias
        public string Description => "Toggles SCP-079’s ability to speak through a proxy";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);

            if (player == null)
            {
                response = "This command can only be used by players.";
                return false;
            }

            if (!Round.IsRoundStarted)
            {
                response = "The round hasn't started yet";
                return false;
            }

            if (player.Role != RoleTypeId.Scp079)
            {
                response = "You must be SCP-079 to use this command.";
                return false;
            }

            SCP079RadioHack.ToggleProxyUsage(player);
            response = "Toggled SCP-079’s ability to speak through a proxy.";
            return true;
        }
    }
}
