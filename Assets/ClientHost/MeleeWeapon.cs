using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private float _Radius = 2.0f;

    [SerializeField]
    private int _Damage = 10;

    [SerializeField]
    protected Vector3 _Offset = new Vector3(0, 1.0f, 1.5f);
    protected override Vector3 Origin { get { return transform.position + transform.TransformDirection(_Offset); } }

    private Collider[] _Colliders = new Collider[15];

    //called on both the server and client(prediction)
    public override bool ProcessFireWeapon(ulong ms, Vector3 position, Vector3 direction)
    {
        if(!base.ProcessFireWeapon(ms, position, direction))
        {
            return false;
        }

        ulong server_ms = PlayerCharacter.Timestep;

        if (networkObject.IsServer)
        {
            //melee is an instantanious weapon, so we can just do a single test on server
            //TODO: there needs to be some kind of rewind or speculation as to the positions of the attacker and all the targets on the server because of the latency
            RewindManager.Instance.StartRewind();

            RewindManager.Instance.RewindTo(ms);

            int hits = Physics.OverlapSphereNonAlloc(position, _Radius, _Colliders);

            RewindManager.Instance.FinishRewind();

            for (int i = 0; i < hits; i++)
            {
                var player_character = _Colliders[i].GetComponent<PlayerCharacter>();
                if (player_character != null && player_character != PlayerCharacter)
                {
                    player_character.AddExternalForce(direction, 300);
                    player_character.ApplyDamage(_Damage, PlayerCharacter);

                    networkObject.SendRpc(PlayerCharacter.RPC_WEAPON_IMPACTED, Receivers.All, server_ms, WeaponIndex, (long)0, position);
                }
            }
        }

        RaiseOnFired(position, direction);

        return true;
    }

    //server only
    public override void Impacted(ulong ms, long id, Vector3 position)
    {
        base.Impacted(ms, id, position);
        RaiseOnImpacted(position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Origin, _Radius);
    }
#endif
}
