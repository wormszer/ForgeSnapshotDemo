//#define USE_OOB_INPUT_VALIDATION
#define VALIDATE_PREDICTION
#define USE_MOUSE_FOR_ROTATION
#define USE_SCREENJOY_FOR_ROTATION
#define DEBUG_STATE
#define USE_UNRELIABLE_INPUT

using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios;
using System.Text;
using DamienG.Security.Cryptography;

public class PlayerCharacter : PlayerCharacterBehavior
{
    [SerializeField]
    private CharacterController _CharacterController;

    [SerializeField]
    private Weapon[] _Weapons;

    [SerializeField]
    private int _MaxHealth = 100;
    public int MaxHealth { get { return _MaxHealth; } }
    [SerializeField]
    private int _Health = 0;

    [SerializeField]
    private PlayerModel _PlayerModel;
    public PlayerModel PlayerModel { get { return _PlayerModel; } }

    [SerializeField]
    private GameObject DBG_IsLocalOwner;

    [SerializeField]
    private GameObject DBG_IsClientHost;

    [SerializeField]
    private GameObject DBG_UsePrediction;

    [SerializeField]
    private GameObject DBG_IsSmoothing;

    [SerializeField]
    private GameObject DBG_IsServer;

    [SerializeField]
    private float _JumpVelocity = 12.0f;

    [SerializeField]
    private float _Gravity = 50.0f;

    [SerializeField]
    private float _Acceleration = 0.3f;

    //structure used by the local client to replay his last inputs after server validation
    private struct InputPrediction
    {
        public ulong Time;
        public Vector3 Movement;
        public Quaternion Rotation;
    }

    //input command sent to the server
    private struct InputCommand
    {
        public ulong Time;
        public Vector3 Velocity;
        public Quaternion Rotation;
    }

    private InputCommand[] _UnreliableInputBuffer = new InputCommand[3];
    private int _UnreliableInputBufferWriteIndex = 0;
    private BMSByte _UnreliableInputBytesSend = new BMSByte();
    private BMSByte _UnreliableInputBytesRecv = new BMSByte();

    private Queue<InputPrediction> _InputQueue = new Queue<InputPrediction>();
    private Queue<InputCommand> _ServerInputQueue = new Queue<InputCommand>();

    private Vector3 _ServerExternalForces = Vector3.zero;

    private bool _IsGameStarted = false;

    private ulong _LastTime = 0;
    private ulong _LastProcessed = 0;

    private bool _FiringWeapon = false;
    private uint _LocalPlayerId = 99999;
    private NetworkingPlayer _LocalPlayer;

    private int _Kills = 0;
    public int Kills { get { return _Kills; } }

    private ulong _NetworkDelay = 200;
    private ulong _NetworkRewind = 1000;

    private bool _IsDead = false;
    public bool IsDead { get { return _IsDead; } }

    public event System.Action<PlayerCharacter> OnDeath;
    public event System.Action<PlayerCharacter> OnKill;

    private int _ForceUpdates = 0;

    //the unique networkplayer id of the player who owns this PlayerCharacter
    public uint LocalPlayerId
    {
        get { return _LocalPlayerId; }
        set { _LocalPlayerId = value; }
    }

    //all network objects are server owned(server authority / server can only change)
    //but we need to let the player control one, so it the clients newtorkPlayer id == this id then this is their playercharacter
    public bool IsLocalOwner
    {
        get { return networkObject.MyPlayerId == _LocalPlayerId; }
    }

    //is this the client player on the server
    public bool IsClientHost
    {
        get { return networkObject.IsServer && IsLocalOwner; }
    }

    //should this playercharacter use prediction
    private bool UsePrediction
    {
        get { return IsLocalOwner && !IsClientHost; }
    }

    public ulong Timestep
    {
        get
        {
            return GameManager.Instance.Tick > _NetworkDelay ? GameManager.Instance.Tick - _NetworkDelay : 0;
        }
    }

    public ulong RewindTimestep
    {
        get
        {
            return GameManager.Instance.Tick > _NetworkRewind ? GameManager.Instance.Tick - _NetworkRewind : 0;
        }
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        BMSLogger.Instance.LogToFile = false;
        BMSLogger.Instance.LoggerVisible = false;
        RewindManager.Instance.RegisterSnapshot(this);

        _Weapons[0].networkObject = networkObject;
        _Weapons[0].PlayerCharacter = this;
        _Weapons[0].WeaponIndex = 0;

        _Weapons[1].networkObject = networkObject;
        _Weapons[1].PlayerCharacter = this;
        _Weapons[1].WeaponIndex = 1;

        networkObject.UpdateInterval = 10; //make sure the update rate is fast because we are sending it every time we tick

        ProjectileManager.Instance.PlayerCharacter = this;

        _IsDead = false;
        _Kills = 0;

        _Health = _MaxHealth;
        networkObject.Health = _Health;
    }

    private void Start()
    {
        _PlayerModel = Instantiate(_PlayerModel);
        _PlayerModel.PlayerCharacter = this;

        if (_PlayerModel == null)
        {
            Debug.LogErrorFormat("Start: No Player Model !!");
        }

        _IsDead = false;
        _Kills = 0;

#if !USE_OOB_INPUT_VALIDATION
        if (!networkObject.IsServer)
        {
            networkObject.OnSnapshotAdded += NetworkObject_OnSnapshotAdded;
        }
#endif
    }

    private Vector3 _Vertical_Acceleration = Vector3.zero;
    private float _LastGroundTime = 0;
    private int _GroundCounts = 0;
    private float _LastUTime = 0;

#if UNITY_STANDALONE
    private ILocalOwnerInputStrategy _InputStrategy = new MouseKeyboardInputStrategy();
#elif UNITY_IOS || UNITY_ANDROID
    private ILocalOwnerInputStrategy _InputStrategy = new OnScreenInputStrategy();
#endif

    interface ILocalOwnerInputStrategy
    {
        void CollectInput();
        float ForwardBack { get; }
        float LeftRight { get; }
        bool Jump { get; }
        bool Action1 { get; }
        bool Action2 { get; }
        Vector3 LookVector { get; }
    }

    class MouseKeyboardInputStrategy : ILocalOwnerInputStrategy
    {
        public float ForwardBack { get; protected set; }
        public float LeftRight { get; protected set; }
        public bool Jump { get; protected set; }
        public bool Action1 { get; protected set; }
        public bool Action2 { get; protected set; }
        public Vector3 LookVector { get; protected set; } = Vector3.zero;

        public void CollectInput()
        {
            ForwardBack = 0.0f;
            LeftRight = 0.0f;
            Jump = false;
            Action1 = false;
            Action2 = false;

            if (Input.GetKey(KeyCode.W))
            {
                ForwardBack = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                ForwardBack = -1.0f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                LeftRight = 1.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                LeftRight = -1.0f;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump = true;
            }

            if (Input.GetMouseButton(0))
            {
                Action1 = true;
            }

            if (Input.GetMouseButton(1))
            {
                Action2 = true;
            }

            LookVector = new Vector3(Input.mousePosition.x - Screen.width * 0.5f, 0.0f, Input.mousePosition.y - Screen.height * 0.5f);
        }
    }

    class OnScreenInputStrategy : ILocalOwnerInputStrategy
    {
        public float ForwardBack { get; protected set; }
        public float LeftRight { get; protected set; }
        public bool Jump { get; protected set; }
        public bool Action1 { get; protected set; }
        public bool Action2 { get; protected set; }
        public Vector3 LookVector { get; protected set; } = Vector3.zero;

        public void CollectInput()
        {
            ForwardBack = 0.0f;
            LeftRight = 0.0f;
            Jump = false;
            Action1 = false;
            Action2 = false;

            if (Input.GetKey(KeyCode.W))
            {
                ForwardBack = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                ForwardBack = -1.0f;
            }
            else if (UIScreenController.Instance != null)
            {
                ForwardBack += UIScreenController.Instance.Joystick.y;
            }

            if (Input.GetKey(KeyCode.A))
            {
                LeftRight = 1.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                LeftRight = -1.0f;
            }
            else if (UIScreenController.Instance != null)
            {
                LeftRight  = UIScreenController.Instance.Joystick.x;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump = true;
            }
            else if (UIScreenController.Instance != null && UIScreenController.Instance.JumpButton)
            {
                Jump = true;
            }

            if (Input.GetMouseButton(0))
            {
                Action1 = true;
            }

            if (Input.GetMouseButton(1))
            {
                Action2 = true;
            }

            if (UIScreenController.Instance != null && UIScreenController.Instance.LeftButton)
            {
                Action1 = true;
            }

            if (UIScreenController.Instance != null && UIScreenController.Instance.RightButton)
            {
                Action2 = true;
            }

            LookVector = new Vector3(LeftRight, 0.0f, ForwardBack);
        }
    }

    private void FixedUpdate()
    {
        if (networkObject.IsServer)
        {
            ServerFixedUpdate();
        }

        if (IsLocalOwner)
        {
            LocalOwnerFixedUpdate();
        }
    }

    private int FORCE_FULL_SNAPSHOT_EVERY_X_TICKS = 10;
    private int _ForceCompleteSnapshotCounter = 0;

    private void ServerFixedUpdate()
    {
        //force a snapshot to happen
        //snapshots must be forced at some specific rate, since they are sent unreliable with no ack back
        //we can never be sure if it was recieved or not.

        networkObject.SetAllDirty();
        //these are always send for every player, since its just the players its probably ok
        //but for other objects it would be good to only send to players when nearby or visisble

        InputCommand input = new InputCommand();
        lock (_ServerInputQueue)
        {
            if (_ServerInputQueue.Count > 0)
            {
                input = _ServerInputQueue.Dequeue();
            }
        }

        if (input.Time > 0)
        {
            //Debug.LogFormat("SendInputs: {0} {1} {2} {3}", gameObject.name, timestep, vel.ToString("F4"), rot.eulerAngles);
            ProcessMovement(input.Time, input.Velocity, input.Rotation);
        }
    }

    private void LocalOwnerFixedUpdate()
    {
        UnityEngine.Profiling.Profiler.BeginSample("PlayerCharacter: Fixed");

        float t = Time.fixedDeltaTime;
        ulong timestep = GameManager.Instance.Tick;

        _LastTime = timestep;
        _LastUTime = Time.time;

        Vector3 gravity = (Vector3.up * _Gravity);
        Vector3 gravity_vel = gravity * t;

        Vector3 movement_velocity = Vector3.zero;

        //these controls still need some work, they are some assumptions here about simulation time steps etc
        Vector3 forward = Vector3.forward * _Acceleration;
        Vector3 right = Vector3.right * _Acceleration;

        _InputStrategy.CollectInput();

        movement_velocity += forward * _InputStrategy.ForwardBack;
        movement_velocity += -right * _InputStrategy.LeftRight;

        _GroundCounts++;
        if (_CharacterController.isGrounded)
        {
            _Vertical_Acceleration = Vector3.zero;

            if (_LastGroundTime != 0)
            {
                Debug.LogFormat("Landed: {0} {1}", Time.time - _LastGroundTime, _GroundCounts * t);
                _LastGroundTime = 0;
            }
        }
        else
        {
            if (_LastGroundTime == 0)
            {
                _LastGroundTime = Time.time;
                _GroundCounts = 0;
            }
        }

        _Vertical_Acceleration -= gravity_vel;

        if (_CharacterController.isGrounded)
        {
            if (_InputStrategy.Jump)
            {
                _Vertical_Acceleration += transform.up * _JumpVelocity;
            }
        }

        if (_InputStrategy.Action1)
        {
            _Weapons[0].Fire();
        }

        if (_InputStrategy.Action2)
        {
            _Weapons[1].Fire();
        }

        Vector3 look = _InputStrategy.LookVector;

        var rotation = transform.rotation;
        if (look.sqrMagnitude > 0)
        {
            rotation = Quaternion.LookRotation(look, Vector3.up);
            //Debug.LogFormat("Look: {0} {1} {2}", look.ToString("F3"), rotation.eulerAngles.ToString("F3"), networkObject.NetworkId);
        }

        movement_velocity += _Vertical_Acceleration * t;

        //Debug.LogFormat("Vert: {0}", (_Vertical_Acceleration).ToString("F3"));

        //send our movements to the server for validation
        SendInput(timestep, movement_velocity, rotation);
        if (UsePrediction)
        {
            //if we are the owner and the server then we should not do prediction for the client
            //because the server updates the local player instantly to the correct positions

            //otherwise we apply the same movements to the client instantly so the controls feel responsive
            ProcessMovement(timestep, movement_velocity, rotation);
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    uint _Sequence = 1;
    uint _LastSequence = 0;

#if DEBUG_JITTER
    private byte[] tmp2 = new byte[0];
#endif

    private void SendInput(ulong timestep, Vector3 velocity, Quaternion rotation) 
    {
        UnityEngine.Profiling.Profiler.BeginSample("SendInput");
#if USE_UNRELIABLE_INPUT
        //add the most recent input to the write index
        _UnreliableInputBuffer[_UnreliableInputBufferWriteIndex].Time = timestep;
        _UnreliableInputBuffer[_UnreliableInputBufferWriteIndex].Velocity = velocity;
        _UnreliableInputBuffer[_UnreliableInputBufferWriteIndex].Rotation = rotation;
        _UnreliableInputBytesSend.Clear();

        //since we always send the full array we can kind of exploit it, and know that writeindex + 1 is the oldest in the list
        //then just step forward again the full size and we will be back on the value we just wrote, adding it last
        int idx = (_UnreliableInputBufferWriteIndex + 1) % _UnreliableInputBuffer.Length;
        for (int i = 0; i < _UnreliableInputBuffer.Length; i++)
        {
            //we want to send the oldest first so when we check on the server we can just iterate through the args
            var input = _UnreliableInputBuffer[idx];

            UnityEngine.Profiling.Profiler.BeginSample("Mapping");
            UnityObjectMapper.Instance.MapBytes(_UnreliableInputBytesSend, input.Time);
            UnityObjectMapper.Instance.MapBytes(_UnreliableInputBytesSend, input.Velocity);
            UnityObjectMapper.Instance.MapBytes(_UnreliableInputBytesSend, input.Rotation);
            UnityEngine.Profiling.Profiler.EndSample();

            idx = (idx + 1) % _UnreliableInputBuffer.Length;
        }
        _UnreliableInputBufferWriteIndex = (_UnreliableInputBufferWriteIndex + 1) % _UnreliableInputBuffer.Length;

        UnityEngine.Profiling.Profiler.BeginSample("SendRPC");
        networkObject.SendRpcUnreliable(RPC_SEND_INPUT_BUFFER, Receivers.Server, _Sequence++, _UnreliableInputBytesSend.byteArr);
        UnityEngine.Profiling.Profiler.EndSample();
#else
        networkObject.SendRpc(RPC_SEND_INPUTS, Receivers.Server, timestep, velocity, rotation);
#endif
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void Update()
    {
        if (!_IsGameStarted)
        {
            networkObject.CleanSnapshots(_NetworkRewind);
            return;
        }

        if (!NetworkManager.Instance)
        {
            return;
        }
        UnityEngine.Profiling.Profiler.BeginSample("PlayerCharacter Update");

        DBG_IsLocalOwner.SetActive(IsLocalOwner);
        DBG_IsClientHost.SetActive(IsClientHost);
        DBG_UsePrediction.SetActive(UsePrediction);
        DBG_IsSmoothing.SetActive(!IsLocalOwner && !networkObject.IsServer);
        DBG_IsServer.SetActive(networkObject.IsServer);

        if (networkObject.IsServer)
        {
            //set the server positions to the players to be updated on the clients
            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;
            networkObject.Health = _Health;

            //if this is the server and not the local owner, the visuals will introduce a delay

            //keep the server snapshots just long enough for the maximum rewind time
            UnityEngine.Profiling.Profiler.BeginSample("CleanSnapshots");
            networkObject.CleanSnapshots(_NetworkRewind);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        else
        {
            if (!IsLocalOwner)
            {
                // If this is not owned by the current network client then it needs to
                // assign it to the position specified
                //(other players on your client, when not on the server)

                if (PlayerCharacterNetworkObject.SNAPSHOTS_ENABLED)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("GetSnapShot");
                    var snapshot = networkObject.GetSnapshot(Timestep);
                    networkObject.ApplySnapshot(snapshot);
                    UnityEngine.Profiling.Profiler.EndSample();
                }

                transform.position = networkObject.position;
                transform.rotation = networkObject.rotation;
                _Health = networkObject.Health;
            }
            UnityEngine.Profiling.Profiler.BeginSample("CleanSnapshots");
            networkObject.CleanSnapshots(_NetworkDelay);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public PlayerCharacterNetworkObject.SnapShot GetSnapShot(ulong timestep)
    {
        var snapshot = networkObject.GetSnapshot(timestep);
        return snapshot;
    }

    public void GetSnapShotWindow(out long start, out long end, out int count)
    {
        ulong s;
        ulong e;
        networkObject.GetSnapShotWindow(out s, out e, out count);
        start = (long)s - (long)GameManager.Instance.Tick;
        end = (long)e - (long)GameManager.Instance.Tick;
    }

    //called on both the client(predict) and server
    void ProcessMovement(ulong input_time, Vector3 movement, Quaternion rotation)
    {
        UnityEngine.Profiling.Profiler.BeginSample("ProcessMovement");
        ulong server_ms = Timestep;

        //Debug.LogFormat("ProcessMovement: {0} {1} {2} {3}", gameObject.name, ms, movement.ToString("F4"), rotation.eulerAngles);

        if (UsePrediction)
        {
            //keep for client prediction
            _InputQueue.Enqueue(new InputPrediction()
            {
                Time = input_time,
                Movement = movement,
                Rotation = rotation
            });
        }

        Vector3 external_forces = Vector3.zero;

        if (networkObject.IsServer)
        {
            //external forces are like explosions etc

            for (int i = _ExternalForces.Count - 1; i >= 0; i--)
            {
                var external_force = _ExternalForces[i];

                external_forces += external_force.Force;

                ulong elapsed_ms = server_ms - external_force.StartMS;
                //Debug.LogFormat("External Movements: {0} {1} {2}", external_force.Force, elapsed_ms, external_force.DurationMS);

                if (elapsed_ms > external_force.DurationMS)
                {
                    _ExternalForces.RemoveAt(i);
                    //Debug.LogFormat("External Movements Finshed: {0} {1}", Time.time - external_force.Time, elapsed_ms);
                }
                else
                {
                    _ExternalForces[i] = external_force;
                }
            }
        }
        else
        {
            //to make our predictions more accurate we need to apply the current forces from there server too every movement
            //this cuts our error down until a major change happens
            external_forces += _ServerExternalForces;
        }

        transform.rotation = rotation;

        _CharacterController.Move(movement + external_forces);

        if (networkObject.IsServer)
        {
            //we are the server so let the client player know we have processed their inputs
            _LastProcessed = input_time;
            networkObject.LastCommand = input_time;
            networkObject.velocity = external_forces;
#if USE_OOB_INPUT_VALIDATION
            //only send to the localowner, other clients do not care about this data
            networkObject.SendRpc(_LocalPlayer, RPC_UPDATE_POSITION, ms, transform.position, transform.rotation, external_forces);
#else
            //the client validates his input based on the snapshot, not a special data stream
            UpdateMovement(input_time, transform.position, transform.rotation, external_forces);
#endif
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    /// client(owner) -> server
    public override void SendInputs(RpcArgs args)
    {
        ulong timestep = args.GetNext<ulong>();
        Vector3 vel = args.GetNext<Vector3>();
        Quaternion rot = args.GetNext<Quaternion>();

        lock (_ServerInputQueue)
        {
            _ServerInputQueue.Enqueue(new InputCommand()
            {
                Time = timestep,
                Velocity = vel,
                Rotation = rot
            });
        }
    }

    private ulong _LastInputHandled = 0;

    public override void SendInputBuffer(RpcArgs args)
    {
        lock (_ServerInputQueue)
        {
            uint seq = args.GetNext<uint>();
            if (seq > _LastSequence)
            {
                //our unreliable input has arrived it will contain multiple inputs
                _UnreliableInputBytesRecv.Clear();
                _UnreliableInputBytesRecv.Append(args.GetNext<byte[]>());
                _LastSequence = seq;
                //they should be stored oldest to newest, with newest being the last input collected
                for (int i = 0; i < _UnreliableInputBuffer.Length; i++)
                {
                    ulong timestep = UnityObjectMapper.Instance.Map<ulong>(_UnreliableInputBytesRecv);
                    Vector3 velocity = UnityObjectMapper.Instance.Map<Vector3>(_UnreliableInputBytesRecv);

                    Quaternion rotation = UnityObjectMapper.Instance.Map<Quaternion>(_UnreliableInputBytesRecv);

                    if (timestep > _LastInputHandled)
                    {
                        _ServerInputQueue.Enqueue(new InputCommand()
                        {
                            Time = timestep,
                            Velocity = velocity,
                            Rotation = rotation
                        });
                        _LastInputHandled = timestep;
                    }
                }
            }
            else
            {
                Debug.LogFormat("OOS: {0} {1}", _LastSequence, seq);
            }
        }
    }

    private void NetworkObject_OnSnapshotAdded(PlayerCharacterNetworkObject.SnapShot snapShot)
    {
        MainThreadManager.Run(() =>
        {
            UpdateMovement(snapShot.LastCommand, snapShot.position, snapShot.rotation, snapShot.velocity);
        });
    }

    //server -> client (owner)
    public override void UpdatePosition(RpcArgs args)
    {
        ulong response_time = args.GetNext<ulong>();
        Vector3 pos = args.GetNext<Vector3>();
        Quaternion rot = args.GetNext<Quaternion>();
        Vector3 vel = args.GetNext<Vector3>();

        MainThreadManager.Run(() =>
        {
            UpdateMovement(response_time, pos, rot, vel);
        });
    }

    private void UpdateMovement(ulong response_time, Vector3 pos, Quaternion rot, Vector3 vel)
    {
        //Debug.LogFormat("UpdatePosition {4}: {0} {1} {2} {3}", gameObject.name, response_time, pos.ToString("F4"), rot.eulerAngles, IsLocalOwner);
        if (IsLocalOwner)
        {
#if VALIDATE_PREDICTION
            Vector3 start_position = transform.position;
#endif
            UnityEngine.Profiling.Profiler.BeginSample("UpdatePosition");
            //reset to the last server authenticated position and rotation
            transform.position = pos;
            transform.rotation = rot;
            _ServerExternalForces = vel; //used in the next client prediction

            if (UsePrediction)
            {
                //only apply prediction to the remote client

                var input_queue = _InputQueue;
                int in_count = _InputQueue.Count;

                //remove the earlier inputs before server validated us
                UnityEngine.Profiling.Profiler.BeginSample("Clear Queue");
                while (_InputQueue.Count > 0 && _InputQueue.Peek().Time <= response_time)
                {
                    var peek = _InputQueue.Peek();
                    _InputQueue.Dequeue();
                    in_count = _InputQueue.Count;
                }
                UnityEngine.Profiling.Profiler.EndSample();

                //replay all the outstanding movements
                in_count = _InputQueue.Count;
                UnityEngine.Profiling.Profiler.BeginSample("Movement");
                foreach (var input in _InputQueue)
                {
                    _CharacterController.Move(input.Movement);
                    transform.rotation = input.Rotation;
                }
                UnityEngine.Profiling.Profiler.EndSample();

                //if our prediction was good, we should not see any jumping or jitter on the client
                //TODO: possibly need to slowly correct any errors (more for the rendering side, PlayerModel does a little of this)
#if VALIDATE_PREDICTION
                Vector3 error = (transform.position - start_position);
                if (error.magnitude > 0.0f)
                {
                    Debug.LogFormat("Prediction Error: {0}", error.ToString("F4"));
                }
#endif
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public override void StartGame(RpcArgs args)
    {
        if (!IsLocalOwner && !networkObject.IsServer)
        {
            _PlayerModel.FollowRatePos = 0;
        }
        _IsGameStarted = true;
        Debug.LogFormat("Start Game!!");
        if (_PlayerModel == null)
        {
            Debug.LogErrorFormat("StartGame: No Player Model !!");
        }
    }

    private void OnDestroy()
    {
        //Cleanup();
    }

    private void Cleanup()
    {
        if (networkObject != null)
            networkObject.Destroy();
    }

    public override void FireWeapon(RpcArgs args)
    {
        ulong ms = args.GetNext<ulong>();
        byte widx = args.GetNext<byte>();
        Vector3 position = args.GetNext<Vector3>();
        Vector3 direction = args.GetNext<Vector3>();

        MainThreadManager.Run(() =>
        {
            _Weapons[widx].ProcessFireWeapon(ms, position, direction);
        });
    }

    public override void WeaponFired(RpcArgs args)
    {
        ulong ms = args.GetNext<ulong>();
        byte widx = args.GetNext<byte>();

        MainThreadManager.Run(() =>
        {
            _Weapons[widx].Fired();
        });
    }

    public override void WeaponImpacted(RpcArgs args)
    {
        ulong ms = args.GetNext<ulong>();
        byte widx = args.GetNext<byte>();
        long id = args.GetNext<long>();
        Vector3 position = args.GetNext<Vector3>();
        MainThreadManager.Run(() =>
        {
            _Weapons[widx].Impacted(ms, id, position);
        });
    }

    public override void SetLocalPlayerId(RpcArgs args)
    {
        _LocalPlayerId = args.GetNext<uint>();
        _LocalPlayer = networkObject.Networker.GetPlayerById(_LocalPlayerId);
        networkObject.IsLocalOwner = IsLocalOwner;
        MainThreadManager.Run(() =>
        {
            this.name = string.Format("Player-{0}", _LocalPlayerId);
        });
    }

    public void ApplyDamage(int amount, PlayerCharacter damger)
    {
        if (networkObject.IsServer)
        {
            _Health -= amount;
            if (_Health <= 0)
            {
                //tell the player was killed and who killed them
                networkObject.SendRpc(RPC_DIE, Receivers.All, damger.LocalPlayerId);

                //tell the killer they killed somebody
                //TODO: this is probably not really needed, could do this on each client as well when a RPC_DIE is recieved
                var killer = GameLogic.Instance.Players.Find(x => x.LocalPlayerId == damger.LocalPlayerId);
                if (killer != null)
                {
                    killer.networkObject.SendRpc(RPC_KILL, Receivers.All, LocalPlayerId);
                }
            }
        }
    }

    public int Health
    {
        get { return _Health; }
    }

    public struct ExternalForce
    {
        public Vector3 Force;
        public ulong DurationMS;
        public ulong StartMS;
        public float Time;
    }

    private List<ExternalForce> _ExternalForces = new List<ExternalForce>();

    public void AddExternalForce(Vector3 force, ulong duration_ms)
    {
        ulong server_ms = Timestep;

        _ExternalForces.Add(new ExternalForce()
        {
            Force = force,
            DurationMS = duration_ms,
            StartMS = server_ms,
            Time = Time.time
        });
    }

    public override void Die(RpcArgs args)
    {
        _IsDead = true;
        uint killer_id = args.GetNext<uint>();

        MainThreadManager.Run(() =>
        {
            if (OnDeath != null)
            {
                OnDeath(this);
            }
        });
    }

    public override void Kill(RpcArgs args)
    {
        uint killed_id = args.GetNext<uint>();
        _Kills++;

        MainThreadManager.Run(() =>
        {
            if (OnKill != null)
            {
                OnKill(this);
            }
        });
    }
}
