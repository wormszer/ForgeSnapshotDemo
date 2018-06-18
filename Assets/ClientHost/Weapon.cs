using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    protected ulong AttackRateMS = 100;

    protected virtual Vector3 Origin { get { return transform.position; } }
    protected virtual Vector3 Direction { get { return transform.forward; } }

    public byte WeaponIndex { get; set; }

    protected ulong _LastAttackMS = 0;

    public NetworkObject networkObject { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; }

    public event System.Action<Vector3, Vector3> OnFired;
    public event System.Action<Vector3> OnImpacted;

    protected void RaiseOnFired(Vector3 position, Vector3 direction)
    {
        if(OnFired != null)
        {
            OnFired(position, direction);
        }
    }

    protected void RaiseOnImpacted(Vector3 position)
    {
        if(OnImpacted != null)
        {
            OnImpacted(position);
        }
    }

    public bool CanFire
    {
        get
        {
            ulong server_ms = PlayerCharacter.Timestep;
            if (server_ms - _LastAttackMS < AttackRateMS)
            {
                return false;
            }

            return true;
        }
    }

    public bool Fire()
    {
        ulong server_ms = PlayerCharacter.Timestep;
        if (CanFire)
        {
            networkObject.SendRpc(PlayerCharacter.RPC_FIRE_WEAPON, Receivers.Others, server_ms, WeaponIndex, Origin, Direction);
            ProcessFireWeapon(server_ms, Origin, Direction);
            Debug.LogWarningFormat("Fired: {0}", server_ms);
            return true;
        }

        return false;
    }

    //called on both the server and client(prediction)
    public virtual bool ProcessFireWeapon(ulong ms, Vector3 position, Vector3 direction)
    {
        if (networkObject.IsServer)
        {
            if(!CanFire)
            {
                return false;
            }
        }

        ulong server_ms = PlayerCharacter.Timestep;
        _LastAttackMS = server_ms;
        return true;
    }

    public virtual void Impacted(ulong ms, long id, Vector3 position)
    {
    }

    public void Fired()
    {

    }
}
