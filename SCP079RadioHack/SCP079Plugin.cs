using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using Mirror;

namespace SCP079RadioHack
{
    public class SCP079Plugin : Plugin
    {
        public override string Name => "SCP-079 Radio Hack";
        public override string Description => "A plugin that adds an ability for SCP-079 to hack the NTF radio.";
        public override string Author => "Sarean";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 0, 0);

        public static NetworkMessageDelegate OriginalVoiceHandler => _originalVoiceHandler;

        private static NetworkMessageDelegate _originalVoiceHandler;
        private static SCP079RadioHack _scp079RadioHack;

        public override void Enable()
        {
            Logger.Info("Enabling SCP-079 Radio Hack...");
            _scp079RadioHack = new SCP079RadioHack();
            CustomHandlersManager.RegisterEventsHandler(_scp079RadioHack);
        }

        public override void Disable()
        {
            Logger.Info("Disabling SCP-079 Radio Hack...");
            CustomHandlersManager.UnregisterEventsHandler(_scp079RadioHack);
        }
    }
}
