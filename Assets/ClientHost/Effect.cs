using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField]
    private float _Duration = 0.0f;

    private float _StartTime = 0.0f;

    public void Play()
    {
        _StartTime = Time.time;
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update ()
    {
		if(Time.time - _StartTime > _Duration)
        {
            gameObject.SetActive(false);
        }
	}
}
