using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField]
    private PlayerCharacter _PlayerCharacter;
    public PlayerCharacter PlayerCharacter
    {
        get { return _PlayerCharacter; }
        set { _PlayerCharacter = value; }
    }


    [SerializeField]
    private float _FollowRatePos = 1.5f;
    public float FollowRatePos
    {
        get { return _FollowRatePos; }
        set { _FollowRatePos = value; }
    }

    [SerializeField]
    private float _FollowRateRateRot = 360f;
    public float FollowRateRateRot
    {
        get { return _FollowRateRateRot; }
        set { _FollowRateRateRot = value; }
    }

    private Vector3 _Velocity = Vector3.zero;

    [SerializeField]
    private bool _DBG_PlayerLocalPlayer = false;

    private Quaternion _LastRot = Quaternion.identity;
    private int _SameCount = 0;
    void Update ()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.Networker == null)
        {
            return;
        }
        //move the player model with the network representation
        if (_PlayerCharacter != null)
        {
            UnityEngine.Profiling.Profiler.BeginSample("PlayerModel");
            if (_PlayerCharacter.IsLocalOwner || !PlayerCharacterNetworkObject.SNAPSHOTS_ENABLED)
            {
                if (_DBG_PlayerLocalPlayer)
                {
                    transform.position = _PlayerCharacter.transform.position;
                    transform.rotation = _PlayerCharacter.transform.rotation;
                }
                else
                {
                    //if is the localplayer (predicted) then just smooth it a little bit
                    transform.position = Vector3.SmoothDamp(transform.position, _PlayerCharacter.transform.position, ref _Velocity, FollowRatePos * Time.deltaTime);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, _PlayerCharacter.transform.rotation, FollowRateRateRot * Time.deltaTime);
                }
            }
            else
            {
                //not the local owned player
                if (NetworkManager.Instance.Networker.IsServer)
                {
                    //we are on the server so we need to add some extra delay??? to the visual representation to try and match the other clients
                    //because the local network representation is update faster for the client host
                    ulong timestep = _PlayerCharacter.Timestep;
                    UnityEngine.Profiling.Profiler.BeginSample("GetSnapShot");
                    var snapshot = _PlayerCharacter.GetSnapShot(timestep);
                    UnityEngine.Profiling.Profiler.EndSample();
                    transform.position = snapshot.position;
                    transform.rotation = snapshot.rotation;
                }
                else
                {
                    //non local models are using snapshot interpolation
                    transform.position = _PlayerCharacter.transform.position;
                    transform.rotation = _PlayerCharacter.transform.rotation;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        else
        {
            Debug.LogErrorFormat("PlayerModel: No Player Character!!");
        }
    }
}
