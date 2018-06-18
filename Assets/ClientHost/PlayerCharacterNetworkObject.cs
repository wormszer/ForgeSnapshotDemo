using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
    public partial class PlayerCharacterNetworkObject : NetworkObject
    {

        public bool IsLocalOwner { get; set; }

#if NOT_GENERATED
        public const bool SNAPSHOTS_ENABLED = true;

        //public void SetAllDirty()
        //{
        //    // Reset all the dirty fields
        //    for (int i = 0; i < _dirtyFields.Length; i++)
        //        _dirtyFields[i] = 0xFF;
        //    hasDirtyFields = true;
        //}

        public struct SnapShot
        {
            public long tick;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public int Health;
        };

        protected System.Collections.Generic.LinkedList<SnapShot> _Snapshots = new System.Collections.Generic.LinkedList<SnapShot>();

        protected void SerializeDirtyFieldsSnapshot()
        {
            if (IsServer)
            {
                //server keeps snapshots for the rewind ability

                ulong timestep = Networker.Time.Timestep;
                //need to locally produce store the snapshot
                SnapShot snapshot = new SnapShot()
                {
                    tick = (long)timestep,
                    position = _position,
                    rotation = _rotation,
                    velocity = _velocity,
                    Health = _Health
                };

                lock (_Snapshots)
                {
                    _Snapshots.AddLast(snapshot);
                    if (_Snapshots.Count > 5)
                    {
                        Debug.LogFormat("SerializeDirtyFieldsSnapshot: {0} {1} {2} {3}", _Snapshots.Count, _Snapshots.First.Value.tick, timestep, _Snapshots.Last.Value.tick);
                    }
                }
            }
        }

        protected void ReadDirtyFieldsSnapshot(BMSByte data, ulong timestep)
        {
            var last_snap = _Snapshots.Last; // this snapshot should always be after the last one
            
            SnapShot snapshot = new SnapShot()
            {
                tick = (long)timestep,
            };

            //initialize to the previous values, incase not all fields have changed
            if (last_snap != null)
            {
                snapshot.position = last_snap.Value.position;
                snapshot.rotation = last_snap.Value.rotation;
                snapshot.velocity = last_snap.Value.velocity;
                snapshot.Health = last_snap.Value.Health;
            }

            //update dirty fields
            if ((0x1 & readDirtyFlags[0]) != 0)
            {
                snapshot.position = UnityObjectMapper.Instance.Map<Vector3>(data);
            }
            if ((0x2 & readDirtyFlags[0]) != 0)
            {
                snapshot.rotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
            }
            if ((0x4 & readDirtyFlags[0]) != 0)
            {
                snapshot.velocity = UnityObjectMapper.Instance.Map<Vector3>(data);
            }
            if ((0x8 & readDirtyFlags[0]) != 0)
            {
                snapshot.Health = UnityObjectMapper.Instance.Map<int>(data);
            }

            lock (_Snapshots)
            {
                _Snapshots.AddLast(snapshot);
            }
            //Debug.LogFormat("ReadDirtyFieldsSnapshot: {0} {1}", timestep, (long)Networker.Time.Timestep - (long)timestep);
        }

        public void CleanSnapshots(long timestep)
        {
            lock (_Snapshots)
            {
#if ASE
                var snap = _SnapshotsInterpolation.First;
                while (snap != null && _SnapshotsInterpolation.Count > 20)
                {
                    _SnapshotsInterpolation.RemoveFirst();
                    snap = _SnapshotsInterpolation.First;
                }
#else
                var snap = _Snapshots.First;
                while (snap != null && snap.Value.tick < timestep)
                {
                    if (snap.Next != null && snap.Next.Value.tick < timestep)
                    {
                        _Snapshots.RemoveFirst();
                        snap = _Snapshots.First;
                    }
                    else
                    {
                        break;
                    }
                }

                if (_Snapshots.Count > (IsServer ? 15 : 5))
                {
                    Debug.LogFormat("Snapshots: {0} {1} {2} {3}", _Snapshots.Count, _Snapshots.First.Value.tick, timestep, _Snapshots.Last.Value.tick);
                }
#endif
            }
        }

        public bool FindSnapshots(long timestep, out SnapShot lower, out SnapShot upper)
        {
            lock (_Snapshots)
            {
                lower = default(SnapShot);
                upper = lower;

                //Debug.LogFormat("UpdateSnapshot D: {0} {1} {2}", timestep, (long)Networker.Time.Timestep - timestep, (_SnapshotsInterpolation.Last.Value.tick - _SnapshotsInterpolation.First.Value.tick));

                if (_Snapshots.Count == 0)
                    return false;

                if (_Snapshots.Last.Value.tick < timestep)
                {
                    return false;
                }

                var node = _Snapshots.First;
                while (node != null)
                {
                    if (node.Value.tick <= timestep)
                    {
                        lower = node.Value;
                        upper = lower;
                        if (node.Next != null)
                        {
                            if (node.Next.Value.tick > timestep)
                            {
                                upper = node.Next.Value;
                                return true;
                            }
                        }
                    }
                    node = node.Next;
                }
                return false;
            }
        }

        public void GetSnapShotWindow(out long start, out long end, out int count)
        {
            start = 0;
            end = 0;
            count = 0;
            lock (_Snapshots)
            {
                if (_Snapshots.Count > 0)
                {
                    start = _Snapshots.First.Value.tick;
                    end = _Snapshots.Last.Value.tick;
                    count = _Snapshots.Count;
                }                
            }
        }

        public SnapShot GetSnapshot(long timestep)
        {
            SnapShot lower;
            SnapShot upper;

            if (FindSnapshots(timestep, out lower, out upper))
            {
                float t = (timestep - lower.tick) / (float)(upper.tick - lower.tick);

                //Debug.LogFormat("UpdateSnapshot: {0} {1} {2} {3}", t, lower.tick, timestep, upper.tick);
                return new SnapShot()
                {
                    tick = (long)timestep,
                    position = Vector3.Lerp(lower.position, upper.position, t),
                    rotation = Quaternion.Slerp(lower.rotation, upper.rotation, t),
                    velocity = Vector3.Lerp(lower.velocity, upper.velocity, t),
                    Health = (int)Mathf.Lerp((float)lower.Health, (float)upper.Health, t)
                };
            }

            return new SnapShot();
        }

        public void ApplySnapshot(SnapShot snapshot)
        {
            _position = snapshot.position;
            _rotation = snapshot.rotation;
            _velocity = snapshot.velocity;
            _Health = snapshot.Health;
        }

        SnapShot _RewindSnapshot = new SnapShot();

        public void StartRewind()
        {
            ulong timestep = Networker.Time.Timestep;
            //need to locally produce store the snapshot
            _RewindSnapshot = new SnapShot()
            {
                tick = (long)timestep,
                position = _position,
                rotation = _rotation,
                velocity = _velocity,
                Health = _Health
            };
        }

        public void RewindTo(long timestep)
        {
            var snapshot = GetSnapshot(timestep);
            ApplySnapshot(snapshot);
        }

        public void FinishRewind()
        {
            ApplySnapshot(_RewindSnapshot);
        }
#endif
    }
}