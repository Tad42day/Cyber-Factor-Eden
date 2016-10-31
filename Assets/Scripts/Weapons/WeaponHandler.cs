using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeaponHandler : MonoBehaviour {

    Animator animator;
    AudioController ac;

    [System.Serializable]
    public class UserSettings
    {
        public Transform rightHand;
        public Transform pistolUnequipSpot;
        public Transform rifleUnequipeSpot;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class Animations
    {
        public string weaponTypeInt = "WeaponType";
        public string reloadingBool = "isReloading";
        public string aimingBool = "isAiming";
    }
    [SerializeField]
    public Animations animations;

    public Weapon currentWeapon;
    public List<Weapon> weaponList = new List<Weapon>();
    public int maxWeapons = 2;

    private bool aim;
    private bool reload;
    private int weaponType;
    private bool settingWeapon;

	// Use this for initialization
	void Start ()
    {
        GameObject checkSoundTag = GameObject.FindGameObjectWithTag("AudioController");

        if (checkSoundTag != null)
        {
            ac = checkSoundTag.GetComponent<AudioController>();
        }

        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (currentWeapon)
        {
            currentWeapon.SetEquipped(true);
            currentWeapon.SetOwner(this);
            AddWeaponToList(currentWeapon);
            currentWeapon.ownerAiming = aim;

            if (currentWeapon.ammo.clipAmmo <= 0)
            {
                Reload();
            }

            if (reload)
            {
                if (settingWeapon)
                {
                    reload = false;
                }
            }
        }

        if(weaponList.Count > 0)
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                if(weaponList[i] != currentWeapon)
                {
                    weaponList[i].SetEquipped(false);
                    weaponList[i].SetOwner(this);
                }
            }
        }

        Animate();
	}

    void Animate()
    {
        if (!animator)
        {
            return;
        }

        animator.SetBool(animations.aimingBool, aim);
        animator.SetBool(animations.reloadingBool, reload);
        animator.SetInteger(animations.weaponTypeInt, weaponType);

        if (!currentWeapon)
        {
            weaponType = 0;
            return;
        }

        switch (currentWeapon.weaponType)
        {
            case Weapon.WeaponType.Pistol:
                weaponType = 1;
                break;
            case Weapon.WeaponType.Rifle:
                weaponType = 2;
                break;
        }
    }

    void AddWeaponToList(Weapon weapon)
    {
        if (weaponList.Contains(weapon))
        {
            return;
        }

        weaponList.Add(weapon);
    }

    public void FireCurrentWeapon(Ray aimRay)
    {
        if(currentWeapon.ammo.clipAmmo == 0)
        {
            Reload();
            return;
        }

        //currentWeapon.PullTrigger(pulling && aim && !reload && !settingWeapon);
        if(aim && !reload && !settingWeapon)
            currentWeapon.Fire(aimRay);
    }

    public void Reload()
    {
        if (reload || !currentWeapon)
        {
            return;
        }

        if(currentWeapon.ammo.carryingAmmo <= 0 || currentWeapon.ammo.clipAmmo == currentWeapon.ammo.maxClipAmmo)
        {
            return;
        }

        if(ac != null)
        {
            if(currentWeapon.sounds.reloadSound != null)
            {
                if(currentWeapon.sounds.audioS != null)
                {
                    ac.PlaySound(
                        currentWeapon.sounds.audioS,
                        currentWeapon.sounds.reloadSound,
                        true,
                        currentWeapon.sounds.pitchMin,
                        currentWeapon.sounds.pitchMax);
                }
            }
        }

        reload = true;

        StartCoroutine(StopReload());
    }

    IEnumerator StopReload()
    {
        yield return new WaitForSeconds(currentWeapon.weaponSettings.reloadDuration);
        currentWeapon.LoadClip();
        reload = false;
    }

    public void Aim(bool aiming)
    {
        aim = aiming;
    }

    public void DropCurrentWeapon()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.SetEquipped(false);
        currentWeapon.SetOwner(null);
        weaponList.Remove(currentWeapon);
    }

    public void SwitchWeapon()
    {
        if (settingWeapon || weaponList.Count == 0)
        {
            return;
        }

        if (currentWeapon)
        {
            int currentWeaponIndex = weaponList.IndexOf(currentWeapon);
            int nextWeaponIndex = (currentWeaponIndex + 1) % weaponList.Count;

            currentWeapon = weaponList[nextWeaponIndex];
        }
        else
        {
            currentWeapon = weaponList[0];
        }

        settingWeapon = true;
        StartCoroutine(StopSettingWeapon());
    }

    IEnumerator StopSettingWeapon()
    {
        yield return new WaitForSeconds(0.7f);
        settingWeapon = false;
    }

    void OnAnimatorIK()
    {
        if (!animator)
        {
            return;
        }

        if(currentWeapon && currentWeapon.userSettings.leftHandIKTarget && weaponType == 2 && !reload && !settingWeapon)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

            Transform target = currentWeapon.userSettings.leftHandIKTarget;
            Vector3 targetPos = target.position;
            Quaternion targetRot = target.rotation;

            animator.SetIKPosition(AvatarIKGoal.LeftHand, targetPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRot);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }

}
