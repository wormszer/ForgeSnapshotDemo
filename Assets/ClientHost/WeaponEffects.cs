using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEffects : MonoBehaviour
{
    [SerializeField]
    private Weapon _Weapon;

    [SerializeField]
    private Effect _FireEffect;

    [SerializeField]
    private Effect _ImpactEffect;

    // Use this for initialization
    void Start ()
    {
		if(_FireEffect != null)
        {
            _FireEffect = Instantiate(_FireEffect, transform);
            _Weapon.OnFired += _Weapon_OnFired;
        }
        if (_ImpactEffect != null)
        {
            _ImpactEffect = Instantiate(_ImpactEffect, transform);
            _Weapon.OnImpacted += _Weapon_OnImpacted;
        }
    }

    private void _Weapon_OnImpacted(Vector3 position)
    {
        _ImpactEffect.transform.position = position;
        _ImpactEffect.Play();
    }

    private void _Weapon_OnFired(Vector3 position, Vector3 direction)
    {
        _FireEffect.transform.position = position;
        _FireEffect.Play();
    }

}
