using System;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using SCP008X.Components;
using Object = UnityEngine.Object;
using User = Exiled.API.Features.Player;

namespace SCP008X
{
    public class EventHandlers
    {
        private readonly Random _gen = new();
        private int _victims;
        
        public static void OnRoundStart()
        {
            if (Scp008X.Instance.Config.CassieAnnounce && Scp008X.Instance.Config.Announcement != null)
            {
                Cassie.DelayedMessage(Scp008X.Instance.Config.Announcement, 5f);
            }
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            if (Scp008X.Instance.Config.SummaryStats)
            {
                Map.ShowHint($"\n\n\n\n\n\n\n\n\n\n\n\n\n\n<align=left><color=yellow><b>SCP-008 Victims:</b></color> {_victims}/{RoundSummary.ChangedIntoZombies}", 30f);
            }
        }
        public static void OnPlayerJoin(JoinedEventArgs ev)
        {
            ev.Player.SendConsoleMessage("This server uses SCP-008-X, all zombies have been buffed!", "yellow");
        }
        public static void OnPlayerLeave(LeftEventArgs ev)
        {
            
            if(ev.Player.Role==RoleTypeId.Scp0492 && ev.Player.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                ClearScp008(ev.Player);
            }
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Player.UserId == "PET")
            {
                Log.Debug($"{ev.Player} is a pet object, skipping method call.");
                ev.IsAllowed = false;
                return;
            }

            if (ev.Attacker.Role != RoleTypeId.Scp0492) return;
            if (ev.Player == ev.Attacker) return;
            if (Scp008X.Instance.Config.ZombieDamage >= 0)
            {
                ev.Amount = Scp008X.Instance.Config.ZombieDamage;
                Log.Debug($"Damage overriden to be {ev.Amount}.");
            }
            if (Scp008X.Instance.Config.Scp008Buff >= 0)
            {
                ev.Attacker.Health += Scp008X.Instance.Config.Scp008Buff;
                Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Attacker}.");
            }
            var chance = _gen.Next(1, 100);
            if (chance > Scp008X.Instance.Config.InfectionChance || ev.Player.Role.Team == Team.SCPs) return;
            try
            {
                Infect(ev.Player);
                Log.Debug($"Successfully infected {ev.Player} with {chance}% probability.");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to infect {ev.Player}! {e}");
                throw;
            }
        }
        public void OnUsedItem(UsedItemEventArgs ev)
        {
            var chance = _gen.Next(1, 100);
            if (!ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008)) return;
            switch (ev.Item.Type)
            {
                case ItemType.SCP500:
                    Object.Destroy(scp008);
                    Log.Debug($"{ev.Player} successfully cured themselves.");
                    break;
                case ItemType.Medkit:
                    if(chance <= Scp008X.Instance.Config.CureChance) { Object.Destroy(scp008); Log.Debug($"{ev.Player} cured themselves with {chance}% probability."); }
                    break;
            }
        }
        public void OnRoleChange(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Scp0492)
            {
                Log.Debug($"Calling Turn() method for {ev.Player}.");
                Turn(ev.Player);
            }

            if (ev.NewRole == RoleTypeId.Scp0492 && ev.NewRole == RoleTypeId.Scp096) return;
            ClearScp008(ev.Player); ev.Player.Health = 0; Log.Debug($"Called ClearSCP008() method for {ev.Player}.");
        }
        public static void OnReviving(StartingRecallEventArgs ev)
        {
            if (!Scp008X.Instance.Config.BuffDoctor) return;
            ev.IsAllowed = false;
            ev.Player.Role.Set(RoleTypeId.Scp0492);
        }
        public static void OnRevived(FinishingRecallEventArgs ev)
        {
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                ev.Player.ClearBroadcasts();
                ev.Player.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }
            if (Scp008X.Instance.Config.Scp008Buff >= 0) { ev.Player.Health += Scp008X.Instance.Config.Scp008Buff; Log.Debug($"Added {Scp008X.Instance.Config.Scp008Buff} AHP to {ev.Player}."); }
            ev.Player.Health = Scp008X.Instance.Config.ZombieHealth;
            Log.Debug($"Set {ev.Player}'s HP to {Scp008X.Instance.Config.Scp008Buff}.");
            ev.Player.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
        }
        public void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp0492) { ClearScp008(ev.Player); Log.Debug($"Called ClearSCP008() method for {ev.Player}."); }
            if (ev.Player.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                ev.Player.Role.Set(RoleTypeId.Scp0492);
            }
        }
        public void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp049 || ev.Player.Role == RoleTypeId.Scp0492)
            {
                _victims--;
                if (Scp008Check())
                {
                    Log.Debug("SCP008Check() passed. Announcing recontainment...");
                    Cassie.Message("SCP 0 0 8 containedsuccessfully . noscpsleft");
                }
            }

            if (!Scp008X.Instance.Config.AoeInfection || ev.Player.Role != RoleTypeId.Scp0492) return;
            Log.Debug("AOE infection enabled, running check...");
            var players = User.List.Where(x => x.CurrentRoom == ev.Player.CurrentRoom);
            players = players.Where(x => x.UserId != ev.Player.UserId);
            var infecteds = players.ToList();
            Log.Debug($"Made a list of {infecteds.Count} players.");
            if (infecteds.Count == 0) return;
            foreach (var ply in from ply in infecteds let chance = _gen.Next(1, 100) where chance <= Scp008X.Instance.Config.AoeChance && ply.Role.Team != Team.SCPs select ply)
            {
                Infect(ply);
                Log.Debug($"Called Infect() method for {ev.Player} due to AOE.");
            }
        }

        private static void ClearScp008(User player)
        {
            if (player.ReferenceHub.TryGetComponent(out Scp008 scp008))
                Object.Destroy(scp008);
        }

        private static void Infect(User player)
        {
            if(player.GameObject.TryGetComponent(out Scp008 _)) { return; }
            player.GameObject.AddComponent<Scp008>();
            player.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.InfectionAlert}", 10f);
        }
        
        private void Turn(User player)
        {
            _victims++;
            if (!player.ReferenceHub.TryGetComponent(out Scp008 _)) { player.GameObject.AddComponent<Scp008>(); }
            if (player.GetEffect(EffectType.Scp207).IsEnabled) { player.ReferenceHub.playerEffectsController.DisableEffect<Scp207>(); }
            if (player.CurrentItem.IsFirearm) { player.DropItems(); }
            if (Scp008X.Instance.Config.SuicideBroadcast != null)
            {
                player.ClearBroadcasts();
                player.Broadcast(10, Scp008X.Instance.Config.SuicideBroadcast);
            }
            if (!Scp008X.Instance.Config.RetainInventory) { player.ClearInventory(); }
            if (Scp008X.Instance.Config.Scp008Buff >= 0) { player.Health += Scp008X.Instance.Config.Scp008Buff; }
            player.Health = Scp008X.Instance.Config.ZombieHealth;
            player.ShowHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Instance.Config.SpawnHint}", 20f);
            if (!Scp008X.Instance.Config.AoeTurned) return;
            var players = User.List.Where(x => x.CurrentRoom == player.CurrentRoom);
            players = players.Where(x => x.UserId != player.UserId);
            var infecteds = players.ToList();
            if (infecteds.Count == 0) return;
            foreach (var ply in from ply in infecteds let chance = _gen.Next(1, 100) where chance <= Scp008X.Instance.Config.AoeChance && ply.Role.Team != Team.SCPs select ply)
            {
                Infect(ply);
            }
        }
        private static bool Scp008Check()
        {
            var check = 0;
            foreach(var ply in User.List)
            {
                if(ply.ReferenceHub.gameObject.TryGetComponent(out Scp008 _)) { check++; }
                if(ply.Role == RoleTypeId.Scp049) { check++; }
            }
            return check == 0;
        }
    }
}