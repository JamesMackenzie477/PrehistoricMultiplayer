using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealth))]
public class FallDamage : MonoBehaviour {

    [SerializeField] private float minSurviveTime;
    [SerializeField] private float damagePerSecond;

    private PlayerHealth healthScript;
    private CharacterController controller;

    private float airTime = 0;

	// Use this for initialization
	void Start () {
        healthScript = GetComponent<PlayerHealth>();
        controller = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void Update () {
		if (!controller.isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (airTime > minSurviveTime)
            {
                healthScript.health -= airTime * damagePerSecond;
            }
            airTime = 0;
        }
	}
}
