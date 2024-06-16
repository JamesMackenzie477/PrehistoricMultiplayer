using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI : MonoBehaviour {

    [SerializeField] private Text health;
    [SerializeField] private Text hunger;
    [SerializeField] private Text thirst;
    [SerializeField] private Image block;

    private PlayerHealth playerScript;

    // Use this for initialization
    void Start () {
        // gets the player controller script
        playerScript = transform.parent.parent.GetComponent<PlayerHealth>();
        // gui elements position
        health.rectTransform.position = new Vector3(100, 60, 0);
        hunger.rectTransform.position = new Vector3(100, 40, 0);
        thirst.rectTransform.position = new Vector3(100, 20, 0);
        block.rectTransform.position = new Vector3(65, 47, 0);
    }
	
	// Update is called once per frame
	void Update () {
        // gui elements update
        health.text = string.Format("Health: {0}%", Math.Ceiling(playerScript.health));
        hunger.text = string.Format("Hunger: {0}%", Math.Ceiling(playerScript.hunger));
        thirst.text = string.Format("Thirst: {0}%", Math.Ceiling(playerScript.thirst));
    }
}
