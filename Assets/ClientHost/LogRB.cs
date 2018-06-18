using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogRB : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _RigidBody;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Debug.LogFormat("RB: {0}", _RigidBody.velocity.ToString("F3"));
    }
}
