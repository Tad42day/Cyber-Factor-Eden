using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    public static GameController instance;

    private UserInput input { get { return GameObject.FindGameObjectWithTag("Player").GetComponent<UserInput>(); } set { input = value; } }
    private PlayerUI playerUI { get { return FindObjectOfType<PlayerUI>(); } set { playerUI = value; } }
    private WeaponHandler wp { get { return GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponHandler>(); } set { wp = value; } }

    public bool paused;
    public GameObject painelPause;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            if(instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (!paused)
            {
                paused = true;
            }
            else
            {
                paused = false;
            }
        }

        if (paused)
        {
            Time.timeScale = 0f;
            painelPause.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            painelPause.SetActive(false);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (input)
        {
            if (playerUI)
            {
                if (wp)
                {
                    if (playerUI.ammoText)
                    {
                        if (wp.currentWeapon == null)
                        {
                            playerUI.ammoText.text = "Unarmed";
                        }
                        else
                        {
                            playerUI.ammoText.text = wp.currentWeapon.ammo.clipAmmo + " / " + wp.currentWeapon.ammo.carryingAmmo;
                        }
                    }
                }

                if(playerUI.healthBar && playerUI.healthText)
                {
                    playerUI.healthText.text = Mathf.Round(playerUI.healthBar.value).ToString() + " / " + Mathf.Round(playerUI.healthBar.maxValue);
                }
            }
            
        }
    }

}
