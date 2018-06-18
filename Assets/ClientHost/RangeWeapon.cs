using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class RangeWeapon : Weapon
{
    [SerializeField]
    protected int _ProjectileIndex;

    [SerializeField]
    protected Vector3 _Offset = new Vector3(0, 1.0f, 1.5f);

    private List<Projectile> _ActiveProjectiles = new List<Projectile>();
    private long _NextProjectileID = 1;

    protected override Vector3 Origin { get { return transform.position + transform.TransformDirection(_Offset); } }

    //called on both the server and client(prediction)
    public override bool ProcessFireWeapon(ulong ms, Vector3 position, Vector3 direction)
    {
        if (!base.ProcessFireWeapon(ms, position, direction))
        {
            return false;
        }

        //this weapon fires a projectile that moves through the world
        //TODO: there needs to be some kind of rewind or speculation as to the positions of the attacker and all the targets on the server because of the latency

        ulong server_ms = PlayerCharacter.Timestep;
        
        //everyone fire a local projectile
        var projectile = ProjectileManager.Instance.CreateProjectile(PlayerCharacter, server_ms, position, direction, _ProjectileIndex, _NextProjectileID);
        Debug.LogWarningFormat("ProcessFireWeapon: {0} {1}", server_ms, ms);

        if (networkObject.IsServer)
        {
            //only the server checks for collisions
            projectile.OnImpact += Projectile_OnImpact;
            projectile.Collisions = true;

            //RewindManager.Instance.StartRewind();
            //RewindManager.Instance.RewindTo(ms);
            ////need to check for initial impacts

            //RewindManager.Instance.FinishRewind();
        }
        else
        {
            //TODO: need to investigate possible corrections to the client projectiles so they more closely match the server trajectories
            //clients are visual only
            projectile.Collisions = false;
        }

        //we need to track the projectiles we fire so we can remove them later
        _ActiveProjectiles.Add(projectile);

        RaiseOnFired(position, direction);

        //unique particle ID for our projectiles
        _NextProjectileID++;

        return true;
    }

    //server only
    private void Projectile_OnImpact(Projectile projectile)
    {
        projectile.OnImpact -= Projectile_OnImpact;

        ulong server_ms = PlayerCharacter.Timestep;
        if (projectile.CollidedPlayerCharacter != null)
        {
            projectile.ApplyImpact(projectile.CollidedPlayerCharacter);
        }
        BMSLogger.Instance.LogFormat("On Projectile Impacted {0}", projectile.ProjectileID);
        networkObject.SendRpc(PlayerCharacter.RPC_WEAPON_IMPACTED, Receivers.All, server_ms, WeaponIndex, projectile.ProjectileID, projectile.transform.position);
    }

    //server only
    public override void Impacted(ulong ms, long id, Vector3 position)
    {
        //should just be on clients
        var projectile = _ActiveProjectiles.Find(x => x.ProjectileID == id);
        if (projectile)
        {
            if (!networkObject.IsServer)
            {
                projectile.Impact();
                BMSLogger.Instance.LogFormat("Projectile Impacted {0}", projectile.ProjectileID);
            }
            _ActiveProjectiles.Remove(projectile);
            ProjectileManager.Instance.ReturnProjectile(projectile);
        }
        else
        {
            Debug.LogErrorFormat("Projectile not found: {0} {1}", networkObject.NetworkId, id);
        }
        RaiseOnImpacted(position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Origin, 0.3f);
    }
#endif
}
