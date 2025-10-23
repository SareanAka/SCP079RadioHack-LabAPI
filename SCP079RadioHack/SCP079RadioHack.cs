using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace SCP079RadioHack
{
    public class SCP079RadioHack : CustomEventsHandler
    {
        private static readonly HashSet<ReferenceHub> Active = new HashSet<ReferenceHub>();
        private static bool _useProxy = true;
        private static ReferenceHub _radioProxy;
        private static bool _proxySpawning; // guard

        public static void Toggle(Player player)
        {
            var hub = player.ReferenceHub;
            if (!Active.Add(hub))
            {
                Active.Remove(hub);
                player.SendHint("<color=red>Radio link deactivated.</color>", 3f);
            }
            else
            {
                player.SendHint("<color=green>Radio link activated.</color>", 3f);
            }
        }

        public static void ToggleProxyUsage(Player player)
        {
            var hub = player.ReferenceHub;
            if (_useProxy)
            {
                _useProxy = false;
                player.SendHint("<color=red>Radio proxy deactivated.</color>", 3f);
            }
            else
            {
                _useProxy = true;
                player.SendHint("<color=green>Radio proxy activated.</color>", 3f);
            }
        }

        // + Clean up proxy on round reset
        public override void OnServerWaitingForPlayers()
        {
            DestroyProxy();
        }

        public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp079 && ev.Message.Channel == VoiceChatChannel.Proximity)
            {
                if (!Active.Contains(ev.Player.ReferenceHub))
                    return;

                ev.IsAllowed = false;

                var speakerProxy = EnsureProxy(ev.Player.Nickname);
                if (ev.Player.Nickname != ev.Player.DisplayName)
                {
                    Player.Get(speakerProxy).DisplayName = ev.Player.DisplayName;
                }

                var channel = VoiceChatChannel.RoundSummary;

                if (!_useProxy)
                {
                    speakerProxy = ev.Player.ReferenceHub;
                    channel = VoiceChatChannel.Radio;
                }

                var radioMessage = new VoiceMessage(speakerProxy, channel, ev.Message.Data, ev.Message.DataLength, false);

                foreach (var player in Player.ReadyList)
                {
                    if (player.IsDummy || player == ev.Player)
                        continue;

                    if (!CanReceiveRadio(player, ev.Player.ReferenceHub))
                        continue;

                    player.Connection.Send(radioMessage, 0);
                }
            }
            else if (ev.Message.Channel == VoiceChatChannel.Radio)
            {
                // Send it to all SCP-079s with the hack enabled.
                var targets = Player.ReadyList
                    .Where(p => p.Role == RoleTypeId.Scp079 && Active.Contains(p.ReferenceHub))
                    .ToList();

                if (targets.Any())
                {
                    // Copy the voice message, change the channel to RoundSummary chat
                    var newMessage = new VoiceMessage(ev.Message.Speaker, VoiceChatChannel.RoundSummary,
                        ev.Message.Data, ev.Message.DataLength, false);

                    foreach (var computer in targets)
                    {
                        computer.Connection.Send(newMessage, 0);
                    }
                }
            }

        }

        public override void OnPlayerReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (!Active.Contains(ev.Player.ReferenceHub))
                return;

            if (ev.Player.Role == RoleTypeId.Scp079 && ev.Message.Channel == VoiceChatChannel.Radio)
            {
                ev.IsAllowed = false;
            }
        }

        private static bool CanReceiveRadio(Player receiver, ReferenceHub senderHub)
        {
            var radio = receiver.Items
                .Select(i => i.Base)
                .OfType<InventorySystem.Items.Radio.RadioItem>()
                .FirstOrDefault();

            return radio != null && radio.IsUsable;
        }

        // Spawn or return the existing Tutorial dummy used as radio speaker
        private static ReferenceHub EnsureProxy(string spoofedName)
        {
            if (_radioProxy != null && _radioProxy)
                return _radioProxy;

            if (_proxySpawning)
                return _radioProxy; // another packet is already spawning it

            _proxySpawning = true;
            try
            {
                var dummy = DummyUtils.SpawnDummy(spoofedName);
                _radioProxy = dummy; // assign immediately to avoid spawn storms

                dummy.transform.position = new Vector3(40f, 315f, -32f);

                // neutral role
                var roleManager = dummy.roleManager;
                roleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.None, RoleSpawnFlags.None);


                // hide from spectators via wrapper
                var p = Player.Get(dummy);
                if (p != null)
                p.IsSpectatable = false;
                p.UserGroup = null;

                LabApi.Features.Console.Logger.Info("Configured 079 radio proxy.");

                if (dummy.nicknameSync != null)
                    dummy.nicknameSync.Network_myNickSync = spoofedName;
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error($"Failed to spawn/configure 079 radio proxy: {ex}");
            }
            finally
            {
                _proxySpawning = false;
            }

            return _radioProxy;
        }

        private static void DestroyProxy()
        {
            if (_radioProxy == null)
                return;

            if (_radioProxy.gameObject != null && NetworkServer.active)
                NetworkServer.Destroy(_radioProxy.gameObject);

            _radioProxy = null;
            _proxySpawning = false;
        }
    }
}
