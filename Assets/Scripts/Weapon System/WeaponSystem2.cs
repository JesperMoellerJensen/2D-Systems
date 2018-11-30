using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem2 : MonoBehaviour {

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
        if (Weapon.AutoFire)
        {
            if (Input.GetKey(KeyCode.E))
            {
                AttemptToFire();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptToFire();
            }
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
        Vector2 RayOrigin = transform.position + transform.TransformDirection(Weapon.BulletSpawnPoint);
        float weaponAccuracy = Random.Range(-1 + Weapon.Accuracy, 1 - Weapon.Accuracy);

        angle = angle * Quaternion.Euler(0, 0, weaponAccuracy * 20);


        Debug.DrawRay(RayOrigin, angle * transform.right * 10f, Color.red, 1f);
        var bullet = Instantiate(
            Weapon.Projectile,
            RayOrigin,
            transform.rotation*angle);
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
}
