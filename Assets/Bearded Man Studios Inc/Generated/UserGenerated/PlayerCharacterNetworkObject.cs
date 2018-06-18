using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedSnapshot()]
	public partial class PlayerCharacterNetworkObject : NetworkObject
	{
		public const int IDENTITY = 6;
        public const bool SNAPSHOTS_ENABLED = true;

		#pragma warning disable 0067
        public event System.Action<SnapShot> OnSnapshotAdded;
		#pragma warning restore 0067

        public struct SnapShot
        {
            public ulong tick;
			public Vector3 position;		
			public Quaternion rotation;		
			public Vector3 velocity;		
			public int Health;		
			public ulong LastCommand;		
        };
        protected System.Collections.Generic.LinkedList<SnapShot> _Snapshots = new System.Collections.Generic.LinkedList<SnapShot>();
		
		private byte[] _dirtyFields = new byte[1];

		[GeneratedNetworkField()]
		private Vector3 _position;
		public Vector3 position
		{
			get { return _position; }
			set
			{
				// Don't do anything if the value is the same
				if (_position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_position = value;
				hasDirtyFields = true;
			}
		}

		public void SetpositionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}
		
		[GeneratedNetworkField()]
		private Quaternion _rotation;
		public Quaternion rotation
		{
			get { return _rotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_rotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_rotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetrotationDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}
		
		[GeneratedNetworkField()]
		private Vector3 _velocity;
		public Vector3 velocity
		{
			get { return _velocity; }
			set
			{
				// Don't do anything if the value is the same
				if (_velocity == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_velocity = value;
				hasDirtyFields = true;
			}
		}

		public void SetvelocityDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}
		
		[GeneratedNetworkField()]
		private int _Health;
		public int Health
		{
			get { return _Health; }
			set
			{
				// Don't do anything if the value is the same
				if (_Health == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_Health = value;
				hasDirtyFields = true;
			}
		}

		public void SetHealthDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}
		
		[GeneratedNetworkField()]
		private ulong _LastCommand;
		public ulong LastCommand
		{
			get { return _LastCommand; }
			set
			{
				// Don't do anything if the value is the same
				if (_LastCommand == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x10;
				_LastCommand = value;
				hasDirtyFields = true;
			}
		}

		public void SetLastCommandDirty()
		{
			_dirtyFields[0] |= 0x10;
			hasDirtyFields = true;
		}
		

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _velocity);
			UnityObjectMapper.Instance.MapBytes(data, _Health);
			UnityObjectMapper.Instance.MapBytes(data, _LastCommand);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			_rotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			_velocity = UnityObjectMapper.Instance.Map<Vector3>(payload);
			_Health = UnityObjectMapper.Instance.Map<int>(payload);
			_LastCommand = UnityObjectMapper.Instance.Map<ulong>(payload);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			ulong tick = GameManager.Instance.Tick;			
			UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, tick);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _rotation);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _velocity);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _Health);
			if ((0x10 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _LastCommand);
			
            if (IsServer)
            {
                //server keeps snapshots for the rewind ability

                //need to locally produce store the snapshot
                SnapShot snapshot = new SnapShot()
                {
                    tick = tick,
                    position = _position,
                    rotation = _rotation,
                    velocity = _velocity,
                    Health = _Health,
                    LastCommand = _LastCommand,
                };

                lock (_Snapshots)
                {
                    _Snapshots.AddLast(snapshot);
					if (_Snapshots.Count > (IsServer ? 65 : 15))
                    {
                        //Debug.LogFormat("SerializeDirtyFieldsSnapshot: {0} {1} {2} {3}", _Snapshots.Count, _Snapshots.First.Value.tick, tick, _Snapshots.Last.Value.tick);
                    }
                }
                if (OnSnapshotAdded != null)
                {
                    OnSnapshotAdded(snapshot);
                }
            }
			
			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);
			
            var last_snap = _Snapshots.Last; // this snapshot should always be after the last one           
			ulong tick = UnityObjectMapper.Instance.Map<ulong>(data);

            SnapShot snapshot = new SnapShot()
            {
                tick = tick,
            };

            //initialize to the previous values, incase not all fields have changed
            if (last_snap != null)
            {
				snapshot.position = last_snap.Value.position;
				snapshot.rotation = last_snap.Value.rotation;
				snapshot.velocity = last_snap.Value.velocity;
				snapshot.Health = last_snap.Value.Health;
				snapshot.LastCommand = last_snap.Value.LastCommand;
            }

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
			if ((0x10 & readDirtyFlags[0]) != 0)
			{
				snapshot.LastCommand = UnityObjectMapper.Instance.Map<ulong>(data);
			}

            lock (_Snapshots)
            {
                if(_Snapshots.Last != null && snapshot.tick < _Snapshots.Last.Value.tick)
                {
                    Debug.LogErrorFormat("ReadDirtyFields OOB: {0} {1}", _Snapshots.Last.Value.tick, tick);
                }
                _Snapshots.AddLast(snapshot);
            }
			if (OnSnapshotAdded != null)
			{
				OnSnapshotAdded(snapshot);
			}
            //Debug.LogFormat("ReadDirtyFieldsSnapshot: {0} {1}", timestep, (long)Networker.Time.Timestep - (long)timestep);
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
			{
				return;			
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

        public void SetAllDirty()
        {
            // Reset all the dirty fields
            for (int i = 0; i < _dirtyFields.Length; i++)
                _dirtyFields[i] = 0xFF;
            hasDirtyFields = true;
        }
		
        public void CleanSnapshots(ulong time_to_keep)
        {
            lock (_Snapshots)
            {
				if(_Snapshots.Count > 0)
				{		
					var snap = _Snapshots.First;
					//clear the buffer out of anything older than the time_to_keep from the most recent entry
					ulong timestep = _Snapshots.Last.Value.tick - time_to_keep;
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

					if (_Snapshots.Count > (IsServer ? 65 : 15))
					{
						//Debug.LogFormat("Snapshots: {0} {1} {2} {3}", _Snapshots.Count, _Snapshots.First.Value.tick, timestep, _Snapshots.Last.Value.tick);
					}
				}
            }
        }
		
        public bool FindSnapshots(ulong timestep, out SnapShot lower, out SnapShot upper)
        {
            lock (_Snapshots)
            {
                lower = default(SnapShot);
                upper = lower;

                //Debug.LogFormat("UpdateSnapshot D: {0} {1} {2}", timestep, (long)Networker.Time.Timestep - timestep, (_Snapshots.Last.Value.tick - _Snapshots.First.Value.tick));

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
		
        public void GetSnapShotWindow(out ulong start, out ulong end, out int count)
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
		
        public SnapShot GetSnapshot(ulong timestep)
        {
            SnapShot lower;
            SnapShot upper;

            if (FindSnapshots(timestep, out lower, out upper))
            {
                float t = (timestep - lower.tick) / (float)(upper.tick - lower.tick);

                //Debug.LogFormat("UpdateSnapshot: {0} {1} {2} {3}", t, lower.tick, timestep, upper.tick);
                return new SnapShot()
                {
                    tick = timestep,
					position = InterpolateVector3.Interpolate(lower.position, upper.position, t),
					rotation = InterpolateQuaternion.Interpolate(lower.rotation, upper.rotation, t),
					velocity = InterpolateVector3.Interpolate(lower.velocity, upper.velocity, t),
					Health = Interpolated<int>.Interpolate(lower.Health, upper.Health, t),
					LastCommand = Interpolated<ulong>.Interpolate(lower.LastCommand, upper.LastCommand, t),
                };
            }
            else
            {
                ulong start;
                ulong end;
                int count;
                GetSnapShotWindow(out start, out end, out count);
                float t = (timestep - start) / (float)(end - start);
                Debug.LogFormat("UpdateSnapshot: {0} {1} {2} {3} {4}", t, start, timestep, end, count);
            }

            return new SnapShot();
        }
		
        public void ApplySnapshot(SnapShot snapshot)
        {
			_position = snapshot.position;
			_rotation = snapshot.rotation;
			_velocity = snapshot.velocity;
			_Health = snapshot.Health;
			_LastCommand = snapshot.LastCommand;
        }
		        
		SnapShot _RewindSnapshot = new SnapShot();

        public SnapShot StartRewind()
        {
            ulong timestep = GameManager.Instance.Tick;
            //need to locally produce store the snapshot
            _RewindSnapshot = new SnapShot()
            {
                tick = timestep,
				position = _position,
				rotation = _rotation,
				velocity = _velocity,
				Health = _Health,
				LastCommand = _LastCommand,
            };
			
			return _RewindSnapshot;
        }
		
        public SnapShot RewindTo(ulong timestep)
        {
            var snapshot = GetSnapshot(timestep);
            ApplySnapshot(snapshot);
			return snapshot;
        }

        public void FinishRewind()
        {
            ApplySnapshot(_RewindSnapshot);
        }
		
		public PlayerCharacterNetworkObject() : base() { Initialize(); }
		public PlayerCharacterNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerCharacterNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }
	
		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
