#define DEBUG_PROJECTILES

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }
    public PlayerCharacter PlayerCharacter { get; set; }

    [SerializeField]
    private Projectile[] _ProjectilesPrefabs;

    private List<Projectile> _ActiveProjectiles = new List<Projectile>();
    private Queue<Projectile>[] _FreeProjectiles = new Queue<Projectile>[0];

#if DEBUG_PROJECTILES
    private GameObject _DBG_GOFreeProjectiles;
    private GameObject _DBG_GOActiveProjectiles;

#endif

    private void Awake()
    {
        Instance = this;

        _FreeProjectiles = new Queue<Projectile>[_ProjectilesPrefabs.Length];
        for(int i = 0; i<_FreeProjectiles.Length;i++)
        {
            _FreeProjectiles[i] = new Queue<Projectile>();
        }

#if DEBUG_PROJECTILES
        _DBG_GOFreeProjectiles = new GameObject("Free");
        _DBG_GOFreeProjectiles.transform.SetParent(this.transform);

        _DBG_GOActiveProjectiles = new GameObject("Active");
        _DBG_GOActiveProjectiles.transform.SetParent(this.transform);
#endif
    }

    private System.Threading.Thread _MainThread = null;
    private void Start()
    {
        _MainThread = System.Threading.Thread.CurrentThread;
    }

    private void Update()
    {
        float delta_time = Time.deltaTime;
        
        for(int i = 0; i< _ActiveProjectiles.Count;i++)
        {
            var projectile = _ActiveProjectiles[i];
            projectile.Move(PlayerCharacter.Timestep, delta_time);
            if(projectile.Impacted)
            {
                if(projectile.CollidedPlayerCharacter)
                {

                }
            }
        }
    }

    public Projectile CreateProjectile(ulong ms, Vector3 position, Quaternion rotation, int index, long id)
    {
        Debug.AssertFormat(_MainThread == System.Threading.Thread.CurrentThread, "Invalid Thread");
        Projectile projectile = null;

        if (_FreeProjectiles[index].Count > 0)
        {
            projectile = _FreeProjectiles[index].Dequeue();
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
        }
        else
        {
            projectile = Instantiate<Projectile>(_ProjectilesPrefabs[index], position, rotation);
        }

        projectile.Spawned(PlayerCharacter.Timestep);
        projectile.ProjectileID = id;
        projectile.ProjectileIndex = index;

        _ActiveProjectiles.Add(projectile);
#if DEBUG_PROJECTILES
        projectile.transform.SetParent(_DBG_GOActiveProjectiles.transform);
#endif

        return projectile;
    }

    public Projectile CreateProjectile(PlayerCharacter owner, ulong ms, Vector3 position, Vector3 direction, int index, long id)
    {
        Debug.AssertFormat(_MainThread == System.Threading.Thread.CurrentThread, "Invalid Thread");

        var look_rotation = Quaternion.LookRotation(direction, Vector3.up);
        Projectile projectile = CreateProjectile(ms, position, look_rotation, index, id);

        projectile.Owner = owner;
        return projectile;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        Debug.AssertFormat(_MainThread == System.Threading.Thread.CurrentThread, "Invalid Thread");
        projectile.Returned();

        if (!_ActiveProjectiles.Remove(projectile))
        {
            Debug.LogErrorFormat("Could not remove projectile {0} {1}", projectile.Owner.networkObject.NetworkId, projectile.ProjectileID);
        }

        _FreeProjectiles[projectile.ProjectileIndex].Enqueue(projectile);
#if DEBUG_PROJECTILES
        projectile.transform.SetParent(_DBG_GOFreeProjectiles.transform);
#endif
    }
}
