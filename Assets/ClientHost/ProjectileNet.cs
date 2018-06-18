using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class ProjectileNet : ProjectileBehavior
{
    [SerializeField]
    private float _Speed = 1.0f;

    [SerializeField]
    private float _DurationSeconds = 4.0f;

    [SerializeField]
    private GameObject _Visuals;

    public float Speed { get { return _Speed; } set { _Speed = value; } }

    private float _StartTime = 0;

    private bool _IsReady = false;

    private void Awake()
    {
        _Visuals.SetActive(false);
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        networkObject.UpdateInterval = 100;
        networkObject.position = transform.position;
        networkObject.rotation = transform.rotation;
        networkObject.positionInterpolation.target = transform.position;
        networkObject.rotationInterpolation.target = transform.rotation;

        networkObject.SnapInterpolations();

        _IsReady = true;
        _StartTime = Time.time;
        _Visuals.SetActive(true);
    }

    // Update is called once per frame
    void Update ()
    {
        if(!_IsReady || networkObject == null)
        {
            return;
        }

		if(networkObject.IsOwner) //server
        {
            transform.position += transform.forward * _Speed;

            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;

            if(Time.time - _StartTime > _DurationSeconds)
            {
                Impact();
            }
        }
        else
        {
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;
        }
    }

    public override void Impacted(RpcArgs args)
    {
        networkObject.Destroy(0);
    }

    private void Impact()
    {
        networkObject.SendRpc(RPC_IMPACTED, Receivers.All);
    }

    private void OnTriggerEnter(Collider other)
    {
        //we hit something
        if (networkObject.IsOwner)
        {
            //Impact();
            var player_character = other.GetComponent<PlayerCharacter>();
            if(player_character != null)
            {
            }
        }
    }
}
