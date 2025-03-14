using MEC;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;
using DamageType = Exiled.API.Enums.DamageType;

namespace SCP008X.Components;

public class Scp008 : MonoBehaviour
{
    private Player _ply;
    private float _curAhp;
    private CoroutineHandle _ahp;
    private CoroutineHandle _s008;
    public void Awake()
    {
        _ply = Player.Get(gameObject);
        _ahp = Timing.RunCoroutine(RetainAhp());
        _s008 = Timing.RunCoroutine(Infection());
        Exiled.Events.Handlers.Player.Hurting += WhenHurt;
        Exiled.Events.Handlers.Player.ChangingRole += WhenRoleChange;
    }
    public void OnDestroy()
    {
        Exiled.Events.Handlers.Player.Hurting -= WhenHurt;
        Exiled.Events.Handlers.Player.ChangingRole -= WhenRoleChange;
        _ply = null;
        Timing.KillCoroutines(_ahp);
        Timing.KillCoroutines(_s008);
    }

    private void WhenHurt(HurtingEventArgs ev)
    {
        if (ev.Player != _ply || ev.Player.Role != RoleTypeId.Scp0492)
            return;

        if (_curAhp > 0)
            _curAhp -= ev.Amount;
        else
            _curAhp = 0;
    }

    private void WhenRoleChange(ChangingRoleEventArgs ev)
    {
        if (ev.Player != _ply)
            return;

        switch (ev.Player.Role.Team)
        {
            case Team.SCPs:
                switch (ev.NewRole)
                {
                    case RoleTypeId.Scp0492:
                        Timing.RunCoroutine(RetainAhp());
                        Log.Debug($"Started coroutine for {_ply.Nickname}: RetainAHP.");
                        break;
                    case RoleTypeId.Scp096:
                        Timing.KillCoroutines(_ahp);
                        Log.Debug($"Killed coroutine for {_ply.Nickname}: RetainAHP.");
                        _ply.MaxHealth = 500f;
                        break;
                }
                break;
            case Team.FoundationForces:
            case Team.ChaosInsurgency:
                Timing.RunCoroutine(Infection());
                Timing.KillCoroutines(_ahp);
                Log.Debug($"Traded coroutines for {_ply.Nickname}: RetainAHP -> Infection.");
                break;
        }
    }

    private IEnumerator<float> RetainAhp()
    {
        for(; ; )
        {
            if(_ply.Role == RoleTypeId.Scp0492)
            {
                if (_ply.Health <= _curAhp)
                {
                    _ply.Health = _curAhp;
                }
                else
                {
                    if (_ply.Health >= Scp008X.Instance.Config.MaxAhp)
                    {
                        _ply.Health = Scp008X.Instance.Config.MaxAhp;
                    }
                    _curAhp = _ply.Health;
                }
            }

            yield return Timing.WaitForSeconds(0.05f);
        }
    }

    private IEnumerator<float> Infection()
    {
        for(; ; )
        {
            _ply.Health -= 2;
            if(_ply.Health <= 0)
            {
                _ply.Hurt(_ply, 1f, DamageType.Scp0492, null);
                _ply.Health++;
                break;
            }

            yield return Timing.WaitForSeconds(2f);
        }
    }
}