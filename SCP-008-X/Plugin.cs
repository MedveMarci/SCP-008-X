﻿using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Exiled.Events.Handlers;

namespace SCP008X
{
    public class Plugin : Plugin<Config>
    {
        internal static Plugin Instance { get; } = new Plugin();
        private Plugin() { }

        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        public override string Author { get; } = "DGvagabond";
        public override string Name { get; } = "Scp008X";
        public override Version Version { get; } = new Version(1, 0, 0, 2);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 5);

        private Handlers.Player PlayerEvents;
        private Handlers.Server ServerEvents;
        public static Plugin Singleton;

        public override void OnEnabled()
        {
            try
            {
                base.OnEnabled();
                RegisterEvents();
            }

            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            UnregisterEvents();
        }

        public void RegisterEvents()
        {
            Singleton = this;
            PlayerEvents = new Handlers.Player(this);
            ServerEvents = new Handlers.Server(this);

            Player.Left += PlayerEvents.OnPlayerLeave;
            Player.Hurting += PlayerEvents.OnPlayerHurt;
            Player.Dying += PlayerEvents.OnPlayerDying;
            Player.Died += PlayerEvents.OnPlayerDied;
            Player.ChangingRole += PlayerEvents.OnRoleChange;
            Player.MedicalItemUsed += PlayerEvents.OnHealing;
            Scp049.StartingRecall += PlayerEvents.OnReviving;
            Scp049.FinishingRecall += PlayerEvents.OnRevived;
            Server.RoundStarted += ServerEvents.OnRoundStart;
        }
        public void UnregisterEvents()
        {
            Player.Left -= PlayerEvents.OnPlayerLeave;
            Player.Hurting -= PlayerEvents.OnPlayerHurt;
            Player.Dying -= PlayerEvents.OnPlayerDying;
            Player.Died -= PlayerEvents.OnPlayerDied;
            Player.ChangingRole -= PlayerEvents.OnRoleChange;
            Player.MedicalItemUsed -= PlayerEvents.OnHealing;
            Scp049.StartingRecall -= PlayerEvents.OnReviving;
            Scp049.FinishingRecall -= PlayerEvents.OnRevived;
            Server.RoundStarted -= ServerEvents.OnRoundStart;

            PlayerEvents = null;
            ServerEvents = null;
        }
    }
}