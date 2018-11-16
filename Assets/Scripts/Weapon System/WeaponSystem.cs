using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public WeaponObject Weapon;

    private float _fireCooldown = 0f;
    private bool _isReloading;

    private bool _isClipEmpty
    {
        get
        {
            if (_currentAmmoCount == 0) return true;

            return false;
        }
    }

    private int _currentAmmoCount = 0;

    private void Start()
    {
        _currentAmmoCount = Weapon.ClipSize;
    }


    private void Update()
    {
        DetectInput();
    }

    private void DetectInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptToFire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadWeapon());
        }
    }

    private void AttemptToFire()
    {
        if (_isReloading == false && IsFireOnCoodown() == false && _isClipEmpty == false)
        {
            if (Weapon.Burst)
            {
                BurstFire();
            }
            else
            {
                Fire(Quaternion.identity);
            }
            //Subtract 1 from current ammo
            _currentAmmoCount--;
        }
        Debug.Log(_currentAmmoCount + "/" + Weapon.ClipSize);
    }

    private void BurstFire()
    {
        for (int i = 0; i < Weapon.BulletsPerShot; i++)
        {
            float angle = (Weapon.BulletSpread / 2) - (Weapon.BulletSpread / (Weapon.BulletsPerShot - 1)) * i;

            Quaternion angleZ = Quaternion.Euler(0, 0, angle);

            Fire(angleZ);
        }
    }

    private void Fire(Quaternion angle)
    {
        float weaponAccuracy = Random.Range(-1 + Weapon.Accuracy, 1 - Weapon.Accuracy);

        angle = angle * Quaternion.Euler(0, 0, weaponAccuracy * 20);
        Vector2 RayOrigin = transform.position + transform.TransformDirection(Weapon.BulletSpawnPoint);

        Debug.DrawRay(RayOrigin, angle * transform.right * 10f, Color.red, 1f);
        RaycastHit2D hit = Physics2D.Raycast(RayOrigin, angle * transform.right * Weapon.Range);
        if (hit)
        {
            DoDamage(hit.collider.gameObject);
        }
    }

    private bool IsFireOnCoodown()
    {
        if (Time.time > _fireCooldown)
        {
            _fireCooldown = Time.time + 1 / Weapon.FireRate;
            return false;
        }
        return true;
    }

    private IEnumerator ReloadWeapon()
    {
        Debug.Log("Reloading...");
        _isReloading = true;
        yield return new WaitForSeconds(Weapon.ReloadTime);
        _isReloading = false;
        _currentAmmoCount = Weapon.ClipSize;
        Debug.Log("DoneReloading");
    }




    private void DoDamage(GameObject target)
    {
        //target.TakeDamage(Weapon.DamagePerBullet);
    }


}
