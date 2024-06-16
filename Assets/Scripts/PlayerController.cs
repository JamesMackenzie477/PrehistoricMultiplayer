using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {

	// unity arguments
    [SerializeField] private float lookSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private AudioClip m_FootstepSound;
    [SerializeField] private AudioClip m_JumpSound;

    private CharacterController m_CharacterController;
    private AudioSource m_AudioSource;
    private Camera m_Camera;
    private Vector3 moveDirection = Vector3.zero;
    private Client client;

    public bool isSprinting;

    // used to looking
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    // used for free look
    private float freeLookYaw = 0.0f;
    private float freeLookPitch = 0.0f;

    // Use this for initialization
    void Start()
    {
        // gets character controller and audio source for use in script
        m_CharacterController = GetComponent<CharacterController>();
        m_AudioSource = GetComponent<AudioSource>();
        m_Camera = GetComponentInChildren<Camera>();
        // gets client script to communicate with server
        client = FindObjectOfType(typeof(Client)) as Client;
    }
	
	// Update is called once per frame
	void Update()
    {
        playerMovement();
        playerRotation();
    }

    void playerMovement()
    {
        // if the character is currently on the ground
        if (m_CharacterController.isGrounded)
        {
            // does some magic shit
            moveDirection = transform.TransformDirection(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            // if player is holding shift
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveDirection *= sprintSpeed;
                isSprinting = true;
            }
            else
            {
                moveDirection *= walkSpeed;
                isSprinting = false;
            }
            // if player is jumping
            if (Input.GetKey(KeyCode.Space))
            {
                // sets vertical direction
                moveDirection.y = jumpSpeed;
                // plays jumping sound
                playSound(m_JumpSound);
            }
            else
            {
                // quick fix for flying bug
                moveDirection.y = 0;
            }
        }
        // more magic shit
        moveDirection.y -= gravity * Time.deltaTime;
        // sets characters movement
        m_CharacterController.Move(moveDirection * Time.deltaTime);
        // converts co-ordinates to server command
        object[] serverCommand = { "MOVE", transform.position.x, transform.position.y, transform.position.z, transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w };
        // sends server command to server
        client.Send(serverCommand);
    }

    void playerRotation()
    {
        // hides mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // checks if player is using freelook
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            // updates pitch and yaw depending on which way the mouse is moving
            freeLookYaw += lookSpeed * Input.GetAxis("Mouse X");
            freeLookPitch -= lookSpeed * Input.GetAxis("Mouse Y");
            // stops player from looking too far up or down
            if (freeLookPitch > 90) freeLookPitch = 90;
            else if (freeLookPitch < -90) freeLookPitch = -90;
            // sets rotation of camera
            m_Camera.transform.eulerAngles = new Vector3(freeLookPitch, freeLookYaw, 0);
        }
        else
        {
            freeLookYaw = yaw;
            freeLookPitch = pitch;
            // updates pitch and yaw depending on which way the mouse is moving
            yaw += lookSpeed * Input.GetAxis("Mouse X");
            pitch -= lookSpeed * Input.GetAxis("Mouse Y");
            // stops player from looking too far up or down
            if (pitch > 90) pitch = 90;
            else if (pitch < -90) pitch = -90;
            // sets rotation of player/camera
            transform.eulerAngles = new Vector3(0, yaw, 0);
            m_Camera.transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
    }

    // takes audio clip as argument and plays it
    void playSound(AudioClip audio)
    {
        // adds audio clip to audiosource array
        m_AudioSource.clip = audio;
        // plays audio clip
        m_AudioSource.Play();
    }
}
