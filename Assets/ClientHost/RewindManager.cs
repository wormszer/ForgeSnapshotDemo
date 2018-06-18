using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance { get; protected set; }

    private void Awake()
    {
        Instance = this;
    }

    private List<IRewindable> _SnapShots = new List<IRewindable>();

    public void RegisterSnapshot(IRewindable snapshot)
    {
        _SnapShots.Add(snapshot);
    }

    public void UnRegisterSnapshot(IRewindable snapshot)
    {
        _SnapShots.Remove(snapshot);
    }

    public void StartRewind()
    {
        for (int i = 0; i < _SnapShots.Count; i++)
        {
            _SnapShots[i].StartRewind();
        }
    }

    public void RewindTo(ulong timestep)
    {
        for (int i = 0; i < _SnapShots.Count; i++)
        {
            _SnapShots[i].RewindTo(timestep);
        }
    }

    public void FinishRewind()
    {
        for (int i = 0; i < _SnapShots.Count; i++)
        {
            _SnapShots[i].FinishRewind();
        }
    }
}
