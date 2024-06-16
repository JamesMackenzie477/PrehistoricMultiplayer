using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

    [SerializeField] private float decSpeed;

    // player vitals
    public float health = 100;
    public float hunger = 100;
    public float thirst = 100;

    // sets if the player is dead
    public bool isDead = false;

    private GameObject player;
    private PlayerController playerScript;

    // Use this for initialization
    void Start()
    {
        // gets the player controller script
        playerScript = GetComponent<PlayerController>();
    }
	
	// Update is called once per frame
	void Update () {

        // if hunger and thirst is 0 then start to decrease health
        if (hunger <= 0 || thirst <= 0)
        {
            health -= decSpeed * Time.deltaTime;
            hunger = 0;
            thirst = 0;
        }
        else
        {
            // decrease vitals over time
            // decrease faster if player is sprinting
            if (playerScript.isSprinting)
            {
                hunger -= decSpeed * 2 * Time.deltaTime;
                thirst -= decSpeed * 2 * Time.deltaTime;
            }
            else
            {
                hunger -= decSpeed * Time.deltaTime;
                thirst -= decSpeed * Time.deltaTime;
            }
        }

        // health regeneration
        if (hunger >= 60 && thirst >= 60 && health < 100 && health > 0)
        {
            health += decSpeed * Time.deltaTime;
        }

        // fixes health because it's a float
        // also checks if player is dead
        // if so a bool is set to true
        if (health <= 0)
        {
            health = 0;
            isDead = true;
        }
        else if (health > 100)
        {
            health = 100;
        }
    }
}
