using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider _Slider;

    [SerializeField]
    private PlayerCharacter _PlayerCharacter;

    [SerializeField]
    private Vector3 _Offset;

    [SerializeField]
    private Text _Snapshot;

	void Start ()
    {
        var health_canvas = GameObject.Find("HealthBarCanvas");
        if (health_canvas)
        {
            this.transform.SetParent(health_canvas.transform);
        }

        _Slider.maxValue = _PlayerCharacter.MaxHealth;
    }

    void Update ()
    {
        if (_Slider.value != _PlayerCharacter.Health)
        {
            _Slider.value = _PlayerCharacter.Health;
        }

        if (Time.frameCount % 50 == 0)
        {
            long start;
            long end;
            int count;
            _PlayerCharacter.GetSnapShotWindow(out start, out end, out count);
            _Snapshot.text = string.Format("{0} => {1} {2}", start, end, count);
        }
    }

    private void LateUpdate()
    {
        if (_PlayerCharacter != null && _PlayerCharacter.PlayerModel)
        {
            transform.position = _PlayerCharacter.PlayerModel.transform.position + _Offset;
        }
    }
}
