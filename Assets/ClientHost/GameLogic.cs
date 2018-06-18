using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections.Generic;
using System.Collections;

public class GameLogic : SceneSingleton<GameLogic>
{
    public event System.Action OnStartGame;
    public event System.Action OnGameOver;

    private List<PlayerCharacter> _Players = new List<PlayerCharacter>();
    public List<PlayerCharacter> Players { get { return _Players; } }

    [SerializeField]
    private int _RequiredPlayerCount = 2;

    [SerializeField]
    private GameObject[] _Managers;

    [SerializeField]
    private CameraFollower _CameraFollower;

    public double RoundTripLatency { get; protected set; }

    void InstantiateManagers()
    {
        foreach(var manager in _Managers)
        {
            var inst = Instantiate(manager);
        }
    }

    private void Awake()
    {
        InstantiateManagers();
    }

    private void Start()
    {
        NetworkManager.Instance.objectInitialized += ObjectInitialized;

        if (NetworkManager.Instance.Networker.IsServer)
        {
            NetworkManager.Instance.Networker.playerAccepted += Networker_playerAccepted; ;
            NetworkManager.Instance.Networker.playerDisconnected += Networker_playerDisconnected;

            var player_char = NetworkManager.Instance.InstantiatePlayerCharacter() as PlayerCharacter;
            player_char.LocalPlayerId = NetworkManager.Instance.Networker.Me.NetworkId;

            NetworkManager.Instance.InstantiateGameManager();

            StartCoroutine(coServerWatchForPlayers());
        }
        else
        {
            NetworkManager.Instance.Networker.disconnected += DisconnectedFromServer;
            NetworkManager.Instance.Networker.onPingPong += OnPingPong;
        }

        StartCoroutine(coWaitForPlayers());

        //NetworkManager.Instance.Networker.LatencySimulation = 0;
        //NetworkManager.Instance.Networker.PacketLossSimulation = 0.5f;

        //Application.targetFrameRate = 30;
    }

    private void OnPingPong(double ping, NetWorker sender)
    {
        RoundTripLatency = ping;
    }

    //Server
    private void Networker_playerAccepted(NetworkingPlayer player, NetWorker sender)
    {
        //create the server owned player and assign it it's the player id
        MainThreadManager.Run(() =>
        {
            var player_char = NetworkManager.Instance.InstantiatePlayerCharacter() as PlayerCharacter;
            player_char.LocalPlayerId = player.NetworkId;
        });
    }
    
    //Server
    private void Player_char_OnDeath(PlayerCharacter player_character)
    {
        player_character.OnDeath -= Player_char_OnDeath;
        CheckForEndOfMatch();
    }

    //Server
    private void Networker_playerDisconnected(NetworkingPlayer player, NetWorker sender)
    {
        _Players.RemoveAll((x) =>
        {
            return x.networkObject.NetworkId == player.NetworkId;
        });

        if (_Players.Count < _RequiredPlayerCount)
        {
            MainThreadManager.Run(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                NetworkManager.Instance.Disconnect();
            });
        }
    }

    public void RestartServer()
    {
        Cleanup();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        NetworkManager.Instance.Disconnect();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.objectInitialized -= ObjectInitialized;
            NetworkManager.Instance.Networker.playerAccepted -= Networker_playerAccepted; ;
            NetworkManager.Instance.Networker.playerDisconnected -= Networker_playerDisconnected;
        }
    }

    /// <summary>
    /// Called whenever a new object is being initialized on the network
    /// </summary>
    /// <param name="behavior">The behavior for the object that is initialized</param>
    /// <param name="obj">The network object that is being initialized</param>
    private void ObjectInitialized(INetworkBehavior behavior, NetworkObject obj)
    {
        if (obj is PlayerCharacterNetworkObject)
        {
            var player_char = behavior as PlayerCharacter;
            _Players.Add(player_char);
            player_char.OnDeath += Player_char_OnDeath;
        }
    }

    //runs on all the clients, waiting for enough people to start
    private IEnumerator coWaitForPlayers()
    {
        //while (_Players.Count < _RequiredPlayerCount)
        //{
        //    yield return null;
        //}

        ////we need to wait a little bit longer because of threading with the network
        //yield return null;

        //hook up the local camera, loop to give time for the network rpc's
        bool looking_for_localowner = true;
        while (looking_for_localowner)
        {
            //look for our player and attach the camera
            var pchar = _Players.Find(x => x.networkObject.ClientRegistered && x.IsLocalOwner && x.PlayerModel != null);
            if(pchar != null)
            { 
                _CameraFollower.Target = pchar.PlayerModel.transform;
                looking_for_localowner = false;
            }
            yield return null;
        }

        if (_CameraFollower.Target == null)
        {
            Debug.LogErrorFormat("No LocalOwner");
        }

        if (OnStartGame != null)
        {
            OnStartGame();
        }

        while(true)
        {
			NetworkManager.Instance.Networker.Ping();
            yield return new WaitForSeconds(5.0f);
        }
    }

    //server coroutine looking for new players to join and send them the start messages
    private IEnumerator coServerWatchForPlayers()
    {
        while (_Players.Count < _RequiredPlayerCount)
        {
            yield return null;
        }

        yield return null;

        int last_player_count = _Players.Count;
        int last_player_index = 0;

        var wait = new WaitForSeconds(0.1f);

        while (true)
        {
            last_player_count = _Players.Count;
            for (; last_player_index < last_player_count; )
            {
                var pchar = _Players[last_player_index];
                if (pchar.networkObject.ClientRegistered)
                {
                    pchar.networkObject.SendRpc(PlayerCharacterBehavior.RPC_SET_LOCAL_PLAYER_ID, Receivers.All, pchar.LocalPlayerId);
                    pchar.networkObject.SendRpc(PlayerCharacterBehavior.RPC_START_GAME, Receivers.All);
                    last_player_index++;
                }
                else
                {
                    break;
                }
            }

            if(!NetworkManager.Instance.Networker.IsConnected)
            {
                RestartServer();
            }

            yield return wait;
        }
    }

    private bool CheckForEndOfMatch()
    {
        int alive_count = 0;
        for(int i = 0;i < _Players.Count;i++)
        {
            var pchar = _Players[i];

            if(!pchar.IsDead)
            {
                alive_count++;
            }
        }

        if(alive_count <= 1)
        {
            if(OnGameOver != null)
            {
                OnGameOver();
            }
            return true;
        }

        return false;
    }

    private void DisconnectedFromServer(NetWorker sender)
    {
        NetworkManager.Instance.Networker.disconnected -= DisconnectedFromServer;

        MainThreadManager.Run(() =>
        {
            foreach (var no in sender.NetworkObjectList)
            {
                if (no.Owner.IsHost)
                {
                    BMSLogger.Instance.Log("Server disconnected");
                    //Should probably make some kind of "You disconnected" screen. ah well
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
            }

            NetworkManager.Instance.Disconnect();
        });
    }
}