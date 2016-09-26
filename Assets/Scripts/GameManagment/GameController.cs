using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    public static GameController instance;

    private UserInput player;

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

    }

    void UpdateUI()
    {

    }

}
