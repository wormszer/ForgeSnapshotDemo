using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedSnapshot()]
	public abstract partial class PlayerCharacterBehavior : NetworkBehavior, IRewindable
	{
		public const byte RPC_SEND_INPUTS = 0 + 5;
		public const byte RPC_START_GAME = 1 + 5;
		public const byte RPC_FIRE_WEAPON = 2 + 5;
		public const byte RPC_WEAPON_FIRED = 3 + 5;
		public const byte RPC_SET_LOCAL_PLAYER_ID = 4 + 5;
		public const byte RPC_DIE = 5 + 5;
		public const byte RPC_WEAPON_IMPACTED = 6 + 5;
		public const byte RPC_KILL = 7 + 5;
		public const byte RPC_UPDATE_POSITION = 8 + 5;
		public const byte RPC_SEND_INPUT_BUFFER = 9 + 5;
		
		public PlayerCharacterNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (PlayerCharacterNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("SendInputs", SendInputs, typeof(ulong), typeof(Vector3), typeof(Quaternion));
			networkObject.RegisterRpc("StartGame", StartGame);
			networkObject.RegisterRpc("FireWeapon", FireWeapon, typeof(ulong), typeof(byte), typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("WeaponFired", WeaponFired, typeof(ulong), typeof(byte));
			networkObject.RegisterRpc("SetLocalPlayerId", SetLocalPlayerId, typeof(uint));
			networkObject.RegisterRpc("Die", Die, typeof(uint));
			networkObject.RegisterRpc("WeaponImpacted", WeaponImpacted, typeof(ulong), typeof(byte), typeof(long), typeof(Vector3));
			networkObject.RegisterRpc("Kill", Kill, typeof(uint));
			networkObject.RegisterRpc("UpdatePosition", UpdatePosition, typeof(ulong), typeof(Vector3), typeof(Quaternion), typeof(Vector3));
			networkObject.RegisterRpc("SendInputBuffer", SendInputBuffer, typeof(uint), typeof(byte[]));

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId))
					ProcessOthers(gameObject.transform, obj.NetworkId + 1);
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new PlayerCharacterNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new PlayerCharacterNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// SendInputs(ulong Time, Vector3 Velocity, Quaternion Rotation)
		/// </summary>
		[GeneratedRPC(new string[]{"Time", "Velocity", "Rotation"}, new string[]{"ulong", "Vector3", "Quaternion"})]
		public abstract void SendInputs(RpcArgs args);
		/// <summary>
		/// StartGame()
		/// </summary>
		[GeneratedRPC(new string[]{}, new string[]{})]
		public abstract void StartGame(RpcArgs args);
		/// <summary>
		/// FireWeapon(ulong Time, byte Weapon, Vector3 Position, Vector3 Direction)
		/// </summary>
		[GeneratedRPC(new string[]{"Time", "Weapon", "Position", "Direction"}, new string[]{"ulong", "byte", "Vector3", "Vector3"})]
		public abstract void FireWeapon(RpcArgs args);
		/// <summary>
		/// WeaponFired(ulong Time, byte Weapon)
		/// </summary>
		[GeneratedRPC(new string[]{"Time", "Weapon"}, new string[]{"ulong", "byte"})]
		public abstract void WeaponFired(RpcArgs args);
		/// <summary>
		/// SetLocalPlayerId(uint PlayerId)
		/// </summary>
		[GeneratedRPC(new string[]{"PlayerId"}, new string[]{"uint"})]
		public abstract void SetLocalPlayerId(RpcArgs args);
		/// <summary>
		/// Die(uint PlayerId)
		/// </summary>
		[GeneratedRPC(new string[]{"PlayerId"}, new string[]{"uint"})]
		public abstract void Die(RpcArgs args);
		/// <summary>
		/// WeaponImpacted(ulong Time, byte Weapon, long Id, Vector3 Position)
		/// </summary>
		[GeneratedRPC(new string[]{"Time", "Weapon", "Id", "Position"}, new string[]{"ulong", "byte", "long", "Vector3"})]
		public abstract void WeaponImpacted(RpcArgs args);
		/// <summary>
		/// Kill(uint PlayerId)
		/// </summary>
		[GeneratedRPC(new string[]{"PlayerId"}, new string[]{"uint"})]
		public abstract void Kill(RpcArgs args);
		/// <summary>
		/// UpdatePosition(ulong Time, Vector3 Position, Quaternion Rotation, Vector3 Velocity)
		/// </summary>
		[GeneratedRPC(new string[]{"Time", "Position", "Rotation", "Velocity"}, new string[]{"ulong", "Vector3", "Quaternion", "Vector3"})]
		public abstract void UpdatePosition(RpcArgs args);
		/// <summary>
		/// SendInputBuffer(uint sequence, byte[] inputbuffer)
		/// </summary>
		[GeneratedRPC(new string[]{"sequence", "inputbuffer"}, new string[]{"uint", "byte[]"})]
		public abstract void SendInputBuffer(RpcArgs args);
	        
        public void StartRewind()
        {
			networkObject.StartRewind();
        }
		
        public void RewindTo(ulong timestep)
        {
			networkObject.RewindTo(timestep);
        }

        public void FinishRewind()
        {
			networkObject.FinishRewind();
        }
		
		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}