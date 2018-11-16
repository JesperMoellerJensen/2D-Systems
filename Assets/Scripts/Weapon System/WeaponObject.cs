using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum WeaponType
{
    Primary,
    Secondary
}

[CreateAssetMenu(menuName = "Obtainable Objects/Weapon")]
public class WeaponObject : ScriptableObject
{
    public WeaponType WeaponType;
    public bool AutoFire = false;
    public bool Burst = false;
    [Range(0, 100)]
    public float DamagePerBullet = 10f;
    [Range(0.1f, 50)]
    public float FireRate = 1f;
    [Range(1, 100)]
    public int ClipSize = 30;
    [Range(0, 1)]
    public float Accuracy = 1f;
    //[Range(0,1)]
    //public float Recoil = 1f;
    [Range(2, 10)]
    public int BulletsPerShot = 2;
    [Range(0, 180)]
    public float BulletSpread = 1f;
    [Range(0.1f, 10)]
    public float Range = 1f;
    [Range(1, 10)]
    public float ReloadTime = 1f;

    public Vector2 BulletSpawnPoint = new Vector2(0, 0);
    public Sprite BulletSprite;
    public AudioClip FireSound;

}