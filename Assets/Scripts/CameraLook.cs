using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraLook : MonoBehaviour {

    [SerializeField] private float zoomSpeed;
    [SerializeField] private Texture2D crosshair;
    [SerializeField] private bool crosshairEnabled;

    private Camera m_Camera;

    // Use this for initialization
    void Start () {
        m_Camera = GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
        playerZoom();
    }

    void OnGUI()
    {
        if (crosshairEnabled)
        {         
            // draws crosshair on screen
            GUI.DrawTexture(new Rect((Screen.width - crosshair.width) / 2, (Screen.height - crosshair.height) / 2, crosshair.width, crosshair.height), crosshair);
        }
    }

    void playerZoom()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            /*if (m_Camera.fieldOfView > 45)
                m_Camera.fieldOfView -= zoomSpeed * Time.deltaTime;
            else
                m_Camera.fieldOfView = 40;*/
            m_Camera.fieldOfView = 40;
        }
        else
        {
            /*if (m_Camera.fieldOfView < 85)
                m_Camera.fieldOfView += zoomSpeed * Time.deltaTime;
            else
                m_Camera.fieldOfView = 90;*/
            m_Camera.fieldOfView = 90;
        }
    }

}
