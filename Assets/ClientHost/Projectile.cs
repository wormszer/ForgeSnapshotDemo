using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _Speed = 1.0f;

    [SerializeField]
    private ulong _DurationMS = 2000;

    [SerializeField]
    private int _Damage = 10;

    [SerializeField]
    private GameObject _Visuals;

    [SerializeField]
    private Collider[] _Colliders;

    public int ProjectileIndex { get; set; }
    public PlayerCharacter Owner { get; set; }

    public long ProjectileID { get; set; }

    public float Speed { get { return _Speed; } set { _Speed = value; } }

    private ulong _FireTimeMS = 0;
    private bool _Impacted = false;
    private bool _Collisions = false;
    public bool Impacted { get { return _Impacted; } }

    private PlayerCharacter _CollidedPlayerCharacter;
    public PlayerCharacter CollidedPlayerCharacter { get { return _CollidedPlayerCharacter; } }

    public event System.Action<Projectile> OnImpact;

    private void Awake()
    {
        _Visuals.SetActive(false);
    }

    public bool Collisions
    {
        get { return _Collisions; }
        set
        {
            _Collisions = value;
            for(int i = 0;i< _Colliders.Length;i++)
            {
                _Colliders[i].enabled = _Collisions;
            }
        }
    }

    public void Spawned(ulong server_ms)
    {
        //long server_ms = PlayerCharacter.Timestep;
        _FireTimeMS = server_ms;
        _Impacted = false;
        _CollidedPlayerCharacter = null;
        _Visuals.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Returned()
    {
        _Visuals.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Move(ulong server_ms, float delta_time)
    {
        //long server_ms = PlayerCharacter.Timestep;

        transform.position += transform.forward * _Speed * delta_time;

        if (server_ms - _FireTimeMS > _DurationMS)
        {
            Impact();
        }
    }

    public void Impact()
    {
        _Impacted = true;
        if(OnImpact != null)
        {
            OnImpact(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //we hit something
        var player_character = other.GetComponent<PlayerCharacter>();
        if (player_character != null)
        {
            _CollidedPlayerCharacter = player_character;
        }
        
        Impact();
    }

    public void ApplyImpact(PlayerCharacter player_character)
    {
        player_character.ApplyDamage(_Damage, Owner);
        player_character.AddExternalForce(Vector3.up, 1000);
    }
}