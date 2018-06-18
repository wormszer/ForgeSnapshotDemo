using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	public abstract partial class TestBehavior : NetworkBehavior
	{
		public const byte RPC_FUNC_BLANK = 0 + 5;
		public const byte RPC_FUNC_BYTE = 1 + 5;
		public const byte RPC_FUNC_CHAR = 2 + 5;
		public const byte RPC_FUNC_SHORT = 3 + 5;
		public const byte RPC_FUNC_U_SHORT = 4 + 5;
		public const byte RPC_FUNC_BOOL = 5 + 5;
		public const byte RPC_FUNC_INT = 6 + 5;
		public const byte RPC_FUNC_U_INT = 7 + 5;
		public const byte RPC_FUNC_FLOAT = 8 + 5;
		public const byte RPC_FUNC_LONG = 9 + 5;
		public const byte RPC_FUNC_U_LONG = 10 + 5;
		public const byte RPC_FUNC_DOUBLE = 11 + 5;
		public const byte RPC_FUNC_STRING = 12 + 5;
		public const byte RPC_FUNC_BYTE_ARRAY = 13 + 5;
		public const byte RPC_FUNC_ALL = 14 + 5;
		
		public TestNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (TestNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("FuncBlank", FuncBlank);
			networkObject.RegisterRpc("FuncByte", FuncByte, typeof(byte));
			networkObject.RegisterRpc("FuncChar", FuncChar, typeof(char));
			networkObject.RegisterRpc("FuncShort", FuncShort, typeof(short));
			networkObject.RegisterRpc("FuncUShort", FuncUShort, typeof(ushort));
			networkObject.RegisterRpc("FuncBool", FuncBool, typeof(bool));
			networkObject.RegisterRpc("FuncInt", FuncInt, typeof(int));
			networkObject.RegisterRpc("FuncUInt", FuncUInt, typeof(uint));
			networkObject.RegisterRpc("FuncFloat", FuncFloat, typeof(float));
			networkObject.RegisterRpc("FuncLong", FuncLong, typeof(long));
			networkObject.RegisterRpc("FuncULong", FuncULong, typeof(ulong));
			networkObject.RegisterRpc("FuncDouble", FuncDouble, typeof(double));
			networkObject.RegisterRpc("FuncString", FuncString, typeof(string));
			networkObject.RegisterRpc("FuncByteArray", FuncByteArray, typeof(byte[]));
			networkObject.RegisterRpc("FuncAll", FuncAll, typeof(byte), typeof(char), typeof(short), typeof(ushort), typeof(bool), typeof(int), typeof(uint), typeof(float), typeof(long), typeof(ulong), typeof(double), typeof(string), typeof(byte[]));

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
			Initialize(new TestNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new TestNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// FuncBlank()
		/// </summary>
		[GeneratedRPC(new string[]{}, new string[]{})]
		public abstract void FuncBlank(RpcArgs args);
		/// <summary>
		/// FuncByte(byte argByte)
		/// </summary>
		[GeneratedRPC(new string[]{"argByte"}, new string[]{"byte"})]
		public abstract void FuncByte(RpcArgs args);
		/// <summary>
		/// FuncChar(char argChar)
		/// </summary>
		[GeneratedRPC(new string[]{"argChar"}, new string[]{"char"})]
		public abstract void FuncChar(RpcArgs args);
		/// <summary>
		/// FuncShort(short argShort)
		/// </summary>
		[GeneratedRPC(new string[]{"argShort"}, new string[]{"short"})]
		public abstract void FuncShort(RpcArgs args);
		/// <summary>
		/// FuncUShort(ushort argUShort)
		/// </summary>
		[GeneratedRPC(new string[]{"argUShort"}, new string[]{"ushort"})]
		public abstract void FuncUShort(RpcArgs args);
		/// <summary>
		/// FuncBool(bool argBool)
		/// </summary>
		[GeneratedRPC(new string[]{"argBool"}, new string[]{"bool"})]
		public abstract void FuncBool(RpcArgs args);
		/// <summary>
		/// FuncInt(int argInt)
		/// </summary>
		[GeneratedRPC(new string[]{"argInt"}, new string[]{"int"})]
		public abstract void FuncInt(RpcArgs args);
		/// <summary>
		/// FuncUInt(uint argUInt)
		/// </summary>
		[GeneratedRPC(new string[]{"argUInt"}, new string[]{"uint"})]
		public abstract void FuncUInt(RpcArgs args);
		/// <summary>
		/// FuncFloat(float argFloat)
		/// </summary>
		[GeneratedRPC(new string[]{"argFloat"}, new string[]{"float"})]
		public abstract void FuncFloat(RpcArgs args);
		/// <summary>
		/// FuncLong(long argLong)
		/// </summary>
		[GeneratedRPC(new string[]{"argLong"}, new string[]{"long"})]
		public abstract void FuncLong(RpcArgs args);
		/// <summary>
		/// FuncULong(ulong argULong)
		/// </summary>
		[GeneratedRPC(new string[]{"argULong"}, new string[]{"ulong"})]
		public abstract void FuncULong(RpcArgs args);
		/// <summary>
		/// FuncDouble(double argDouble)
		/// </summary>
		[GeneratedRPC(new string[]{"argDouble"}, new string[]{"double"})]
		public abstract void FuncDouble(RpcArgs args);
		/// <summary>
		/// FuncString(string argString)
		/// </summary>
		[GeneratedRPC(new string[]{"argString"}, new string[]{"string"})]
		public abstract void FuncString(RpcArgs args);
		/// <summary>
		/// FuncByteArray(byte[] argByteArray)
		/// </summary>
		[GeneratedRPC(new string[]{"argByteArray"}, new string[]{"byte[]"})]
		public abstract void FuncByteArray(RpcArgs args);
		/// <summary>
		/// FuncAll(byte argByte, char argChar, short argShort, ushort argUShort, bool argBool, int argInt, uint argUInt, float argFloat, long argLong, ulong argULong, double argDouble, string argString, byte[] argByteArray)
		/// </summary>
		[GeneratedRPC(new string[]{"argByte", "argChar", "argShort", "argUShort", "argBool", "argInt", "argUInt", "argFloat", "argLong", "argULong", "argDouble", "argString", "argByteArray"}, new string[]{"byte", "char", "short", "ushort", "bool", "int", "uint", "float", "long", "ulong", "double", "string", "byte[]"})]
		public abstract void FuncAll(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}