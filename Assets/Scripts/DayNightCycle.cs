using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {

    [SerializeField] private Transform sun;
    [SerializeField] private Transform moon;
    [SerializeField] private float timeScale;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        sun.RotateAround(Vector3.zero, Vector3.right, timeScale * Time.deltaTime);
        moon.RotateAround(Vector3.zero, Vector3.right, timeScale * Time.deltaTime);
        sun.LookAt(Vector3.zero);
        moon.LookAt(Vector3.zero);
    }
}
