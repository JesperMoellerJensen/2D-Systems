using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAim : MonoBehaviour
{

    public float Smoothness = 10f;

    Quaternion _startRot;

    private void Awake()
    {
        _startRot = transform.rotation;
    }

    void LateUpdate()
    {
        var pos = Camera.main.WorldToScreenPoint(transform.position);
        var dir = Input.mousePosition - pos;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion actual = Quaternion.AngleAxis(angle, Vector3.forward);
        _startRot = Quaternion.Lerp(_startRot, actual, Time.deltaTime * Smoothness);

        transform.rotation = _startRot;

        Debug.DrawRay(transform.position,transform.right*10f,Color.green);
    }

}
