using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using Exiled.Events.Handlers;

namespace SCP008X;

public class Scp008X : Plugin<Config>
{
    internal static Scp008X Instance { get; } = new();
    private Scp008X() { }

    public override PluginPriority Priority => PluginPriority.Medium;

    public override string Author => "DGvagabond && MedveMarci";
    public override string Name => "Scp008X";
    public override Version Version { get; } = new(2, 0, 0, 0);
    public override Version RequiredExiledVersion { get; } = new(2, 1, 16);

    private EventHandlers _events;

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

    private void RegisterEvents()
    {
        _events = new EventHandlers();

        Player.Died += _events.OnPlayerDied;
        Player.Left += EventHandlers.OnPlayerLeave;
        Player.Joined += EventHandlers.OnPlayerJoin;
        Player.Dying += _events.OnPlayerDying;
        Player.Hurting += _events.OnPlayerHurt;
        Server.RoundEnded += _events.OnRoundEnd;
        Player.UsedItem += _events.OnUsedItem;
        Player.ChangingRole += _events.OnRoleChange;
        Scp049.StartingRecall += EventHandlers.OnReviving;
        Scp049.FinishingRecall += EventHandlers.OnRevived;
        Server.RoundStarted += EventHandlers.OnRoundStart;
    }

    private void UnregisterEvents()
    {
        Player.ChangingRole -= _events.OnRoleChange;
        Scp049.StartingRecall -= EventHandlers.OnReviving;
        Scp049.FinishingRecall -= EventHandlers.OnRevived;
        Server.RoundStarted -= EventHandlers.OnRoundStart;
        Player.Joined -= EventHandlers.OnPlayerJoin;
        Player.UsedItem -= _events.OnUsedItem;
        Server.RoundEnded -= _events.OnRoundEnd;
        Player.Hurting -= _events.OnPlayerHurt;
        Player.Dying -= _events.OnPlayerDying;
        Player.Left -= EventHandlers.OnPlayerLeave;
        Player.Died -= _events.OnPlayerDied;

        _events = null;
    }
}