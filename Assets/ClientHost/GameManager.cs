using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;

public class GameManager : GameManagerBehavior
{
    public static GameManager Instance { get; private set; }

    public const int SNAPSHOT_GROUP_ID = MessageGroupIds.START_OF_GENERIC_IDS + 1;
    private long _Tick = 0;

    [SerializeField]
    private ulong _NetworkDelay = 200;

    public ulong Tick { get { return (ulong)_Tick; } }

    private long _RemoteTick = 0;

    private void Awake()
    {
        Instance = this;
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
    }

    float _ElapsedTime = 0;
    long _LastSyncTime = 0;

    private void FixedUpdate()
    {
        _Tick += (long)(Time.fixedDeltaTime * 1000U);

        if (networkObject.IsServer)
        {
            _ElapsedTime += Time.fixedDeltaTime;
            if (_ElapsedTime > 5)
            {
                networkObject.SendRpcUnreliable(RPC_SYNC_TIMESTEP, Receivers.Others, _Tick);
                _ElapsedTime = 0;
            }
        }
    }

    public override void SyncTimestep(RpcArgs args)
    {
        var tick = args.GetNext<long>();
        Debug.LogFormat("SyncTimestep: {0}", (long)tick - (long)_Tick);
        if (tick < _LastSyncTime)
        {
            Debug.LogErrorFormat("SyncTimestep: {0} {1}", _LastSyncTime, tick);
        }

        System.Threading.Interlocked.Exchange(ref _Tick, (long)tick);
        _LastSyncTime = tick;
    }
}
