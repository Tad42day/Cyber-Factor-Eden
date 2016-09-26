using UnityEngine;
using System.Collections;
using System;

public class UserInput : MonoBehaviour {

    public PlayerMovement playerMovement { get; set; }
    public WeaponHandler weaponHandler { get; set; }

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string reloadButton = "Reload";
        public string aimButton = "Fire2";
        public string fireButton = "Fire1";
        public string dropWeaponButton = "DropWeapon";
        public string switchWeaponButton = "SwitchWeapon";
    }
    [SerializeField]
    InputSettings input;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 10.0f;
        public bool requireInputForTurn = true;
        public LayerMask aimDetectionLayers;
    }
    [SerializeField]
    OtherSettings other;

    public bool debugAim;
    public Transform spine;
    private bool aiming; 

    public Camera TPSCamera;

	// Use this for initialization
	void Start ()
    {
        playerMovement = GetComponent<PlayerMovement>();
        weaponHandler = GetComponent<WeaponHandler>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        CharacterLogic();
        CameraLookoLogic();
        WeaponLogic();
	}

    void LateUpdate()
    {
        if (weaponHandler)
        {
            if (weaponHandler.currentWeapon)
            {
                if (aiming)
                {
                    PositionSpine();
                }
            }
        }
    }

    void PositionSpine()
    {
        if(!spine || !weaponHandler.currentWeapon || !TPSCamera)
        {
            return;
        }

        Transform mainCamT = TPSCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 dir = mainCamT.forward;
        Ray ray = new Ray(mainCamPos, dir);

        spine.LookAt(ray.GetPoint(50f));

        Vector3 eulerAngleOffset = weaponHandler.currentWeapon.userSettings.spineRotation;
        spine.Rotate(eulerAngleOffset);
    }

    void WeaponLogic()
    {
        if (!weaponHandler)
        {
            return;
        }

        aiming = Input.GetButton(input.aimButton) || debugAim;

        if (weaponHandler.currentWeapon)
        {
            weaponHandler.Aim(aiming);

            other.requireInputForTurn = !aiming;

            weaponHandler.FingerOnTrigger(Input.GetButton(input.fireButton));

            if (Input.GetButtonDown(input.reloadButton))
            {
                weaponHandler.Reload();
            }

            if (Input.GetButtonDown(input.dropWeaponButton))
            {
                weaponHandler.DropCurrentWeapon();
            }

            if (Input.GetButtonDown(input.switchWeaponButton))
            {
                weaponHandler.SwitchWeapon();
            }

            if (!weaponHandler.currentWeapon)
            {
                return;
            }

            weaponHandler.currentWeapon.shootRay = new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);
        }
    }

    void CameraLookoLogic()
    {
        if (!TPSCamera)
        {
            return;
        }

        if (other.requireInputForTurn)
        {
            if (Input.GetAxis(input.horizontalAxis) != 0f || Input.GetAxis(input.verticalAxis) != 0f)
            {
                CharacterLook();
            }
        }
        else
        {
            CharacterLook();
        }
    }

    void CharacterLogic()
    {
        if (!playerMovement)
        {
            return;
        }

        if (playerMovement)
        {
            playerMovement.Animate(Input.GetAxis(input.verticalAxis), Input.GetAxis(input.horizontalAxis));

            if (Input.GetButtonDown(input.jumpButton))
            {
                playerMovement.Jump();
            }
        }
    }

    void CharacterLook()
    {
        Transform mainCamT = TPSCamera.transform;
        Transform pivotT = mainCamT.parent;
        Vector3 pivotPos = pivotT.position;
        Vector3 lookTarget = pivotPos + (pivotT.forward * other.lookDistance);
        Vector3 thisPos = transform.position;
        Vector3 lookDir = lookTarget - thisPos;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);

        lookRot.x = 0f;
        lookRot.z = 0f;

        Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
        transform.rotation = newRotation;
    }
}
