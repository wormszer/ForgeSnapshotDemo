using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField]
    private Transform _Target;
    public Transform Target { get { return _Target; } set {
            _Target = value;
            if(_Target != null)
            {
                enabled = true;
            }
        } }

    [SerializeField]
    public Vector3 _Offset = Vector3.zero;

    private void Start()
    {
        enabled = false;
    }

    // Update is called once per frame
    void LateUpdate ()
    {
        transform.position = Target.position + _Offset;
    }
}
