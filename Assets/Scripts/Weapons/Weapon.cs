using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour {

    Collider coll;
    Rigidbody rigidBody;
    Animator animator;
    AudioSource source;
    public AudioController ac;

    public enum WeaponType
    {
        Pistol,
        Rifle
    }

    public WeaponType weaponType;

    [System.Serializable]
    public class UserSettings
    {
        public Transform leftHandIKTarget;
        public Vector3 spineRotation;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class WeaponSettings
    {
        [Header("-Bullet options-")]
        public Transform bulletSpawn;
        public float damage = 5.0f;
        public float bulletSpread = 5.0f;
        public float fireRate = 0.2f;
        public LayerMask bulletLayers;
        public float range = 200.0f;

        [Header("-Effects-")]
        public GameObject muzzleFlash;
        public GameObject decal;
        public GameObject shell;
        public GameObject clip;

        [Header("-Other settings-")]
        public GameObject crosshairPrefab;
        public float reloadDuration = 2.0f;
        public Transform shellEjectSpawn;
        public float shellEjectSpeed = 7.5f;
        public GameObject clipGO;
        public Transform clipEjectPos;

        [Header("-Positioning-")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;

        [Header("-Animations-")]
        public bool useAnimations;
        public int fireAnimationLayer = 0;
        public string fireAnimationName = "Fire";
    }
    [SerializeField]
    public WeaponSettings weaponSettings;

    [System.Serializable]
    public class Ammunition
    {
        public int carryingAmmo;
        public int clipAmmo;
        public int maxClipAmmo;
    }
    [SerializeField]
    public Ammunition ammo;

    [System.Serializable]
    public class SoundSettings
    {
        public AudioClip[] gunshotSounds;
        public AudioClip reloadSound;
        [Range(0, 3)] public float pitchMin = 1f;
        [Range(0, 3)] public float pitchMax = 1.2f;
        public AudioSource audioS;
    }
    [SerializeField]
    public SoundSettings sounds;

    // Crosshair
    public Ray aimRay { protected get; set; }
    public bool ownerAiming { get; set; }

    private WeaponHandler owner;
    private bool equipped;
    //private bool pullingTrigger;
    private bool resettingCartridge;

	// Use this for initialization
	void Start ()
    {
        GameObject checkSoundTag = GameObject.FindGameObjectWithTag("AudioController");

        if(checkSoundTag != null)
        {
            ac = checkSoundTag.GetComponent<AudioController>();
        }

        coll = GetComponent<Collider>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();

        if(weaponSettings.crosshairPrefab != null)
        {
            weaponSettings.crosshairPrefab = Instantiate(weaponSettings.crosshairPrefab);
            ToggleCrosshair(false);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (owner)
        {
            DisableEnableComponents(false);

            if (equipped)
            {
                if (owner.userSettings.rightHand)
                {
                    Equip();

                    //if (pullingTrigger)
                    //{
                    //    Fire(shootRay);
                    //}

                    if (ownerAiming)
                    {
                        PositionCrosshair(aimRay);
                    }
                    else
                    {
                        ToggleCrosshair(false);
                    }
                }
            }
            else
            {
                Unequip(weaponType);
                ToggleCrosshair(false);
            }
        }
        else
        {
            DisableEnableComponents(true);

            transform.SetParent(null);

            ownerAiming = false;
        }
	}

    void GunEffects()
    {
        #region muzzleFlash
        if (weaponSettings.muzzleFlash)
        {
            Vector3 bulletSpawnPos = weaponSettings.bulletSpawn.position;
            GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash, bulletSpawnPos, Quaternion.identity) as GameObject;
            Transform muzzleT = muzzleFlash.transform;

            muzzleT.SetParent(weaponSettings.bulletSpawn);
            Destroy(muzzleFlash, 1.0f);
        }
        #endregion

        #region shell
        if (weaponSettings.shell)
        {
            if (weaponSettings.shellEjectSpawn)
            {
                Vector3 shellEjectPos = weaponSettings.shellEjectSpawn.position;
                Quaternion shellEjectRot = weaponSettings.shellEjectSpawn.rotation;
                GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot) as GameObject;

                if (shell.GetComponent<Rigidbody>())
                {
                    Rigidbody rigidB = shell.GetComponent<Rigidbody>();

                    rigidB.AddForce(weaponSettings.shellEjectSpawn.forward * weaponSettings.shellEjectSpeed, ForceMode.Impulse);
                }

                Destroy(shell, Random.Range(15.0f, 20.0f));
            }
        }
        #endregion

        PlayGunshotSound();

    }

    void PlayGunshotSound()
    {
        if (ac == null)
        {
            return;
        }

        if (sounds.audioS != null)
        {
            if (sounds.gunshotSounds.Length > 0)
            {
                ac.InstantiateClip(
                    weaponSettings.bulletSpawn.position, // O local onde será reproduzido o som
                    sounds.gunshotSounds[Random.Range(0, sounds.gunshotSounds.Length)], // Qual AudioClip irá ser usado
                    2, // Tempo até destroir o audio
                    true, // Se queremos randomizar o pitch
                    sounds.pitchMin, // Valor minimo do pitch
                    sounds.pitchMax); // Valor maximo do pitch
            }
        }
    }

    void PositionCrosshair(Ray ray)
    {
        RaycastHit hit;
        Transform bSpawn = weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = ray.GetPoint(weaponSettings.range) - bSpawnPoint;

        if (Physics.Raycast(bSpawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            if(weaponSettings.crosshairPrefab != null)
            {
                ToggleCrosshair(true);
                weaponSettings.crosshairPrefab.transform.position = hit.point;
                weaponSettings.crosshairPrefab.transform.LookAt(Camera.main.transform);
            }
            else
            {
                ToggleCrosshair(false);
            }
        }
    }

    void ToggleCrosshair(bool enabled)
    {
        if(weaponSettings.crosshairPrefab != null)
        {
            weaponSettings.crosshairPrefab.SetActive(enabled);
        }
    }

    void HitEffects(RaycastHit hit)
    {
        #region decal
        if (hit.collider.gameObject.isStatic)
        {
            if (weaponSettings.decal)
            {
                Vector3 hitPoint = hit.point;
                Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
                GameObject decal = Instantiate(weaponSettings.decal, hitPoint, lookRotation) as GameObject;
                Transform decalT = decal.transform;
                Transform hitT = hit.transform;

                decalT.SetParent(hitT);
                Destroy(decal, Random.Range(30.0f, 45.0f));
            }
        }
        #endregion

        if(hit.collider.gameObject.tag == "Enemy")
        {
            Debug.Log("Atingiu um inimigo");
        }
    }

    public void Fire(Ray ray)
    {
        if(ammo.clipAmmo <= 0 || resettingCartridge || !weaponSettings.bulletSpawn || !equipped)
        {
            return;
        }

        RaycastHit hit;
        Transform bSpawn = weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = ray.GetPoint(weaponSettings.range) - bSpawnPoint;

        dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;

        if(Physics.Raycast(bSpawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            HitEffects(hit);
        }

        GunEffects();

        if (weaponSettings.useAnimations)
        {
            animator.Play(weaponSettings.fireAnimationName, weaponSettings.fireAnimationLayer);
        }

        ammo.clipAmmo--;
        resettingCartridge = true;

        StartCoroutine(LoadNextBullet());

    }

    IEnumerator LoadNextBullet()
    {
        yield return new WaitForSeconds(weaponSettings.fireRate);
        resettingCartridge = false;
    }

    void DisableEnableComponents(bool enabled)
    {
        if (!enabled)
        {
            rigidBody.isKinematic = true;
            coll.enabled = false;

        }
        else
        {
            rigidBody.isKinematic = false;
            coll.enabled = true;
        }
    }

    // Equipa esta arma na mão do jogador
    void Equip()
    {
        if (!owner)
        {
            return;
        }
        else if (!owner.userSettings.rightHand)
        {
            return;
        }

        transform.SetParent(owner.userSettings.rightHand);
        transform.localPosition = weaponSettings.equipPosition;

        Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRotation);

        transform.localRotation = equipRot;
    }

    void Unequip(WeaponType wpType)
    {
        if (!owner)
        {
            return;
        }

        switch (wpType)
        {
            case WeaponType.Pistol:
                transform.SetParent(owner.userSettings.pistolUnequipSpot);
                break;
            case WeaponType.Rifle:
                transform.SetParent(owner.userSettings.rifleUnequipeSpot);
                break;
        }

        transform.localPosition = weaponSettings.unequipPosition;

        Quaternion unequipRot = Quaternion.Euler(weaponSettings.unequipRotation);

        transform.localRotation = unequipRot;
    }

    public void LoadClip()
    {
        int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;

        if(ammoNeeded >= ammo.carryingAmmo)
        {
            ammo.clipAmmo = ammo.carryingAmmo;
            ammo.carryingAmmo = 0;
        }
        else
        {
            ammo.carryingAmmo -= ammoNeeded;
            ammo.clipAmmo = ammo.maxClipAmmo;
        }
    }

    public void SetEquipped(bool equip)
    {
        equipped = equip;
    }

    //public void PullTrigger(bool isPulling)
    //{
    //    pullingTrigger = isPulling;
    //}

    public void SetOwner(WeaponHandler wp)
    {
        owner = wp;
    }

}
