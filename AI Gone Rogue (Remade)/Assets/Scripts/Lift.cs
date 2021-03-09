using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;          // for blending between virtual cameras?
using DigitalRuby.Tween;    // for animating the lift moving upwards

[RequireComponent(typeof(AudioSource))]
public class Lift : MonoBehaviour
{
    public float startElevation = -10f;
    public float targetElevation = 3.75f;
    public float elevationDuration = 4f;

    [Space]
    public PlayerController player;
    public Animator liftAnimator;
    public Camera mainCamera;

    [Header("Audio Clips")]
    public AudioClip liftReachedSound;
    public AudioClip liftDoorSound;
    private AudioSource liftAudioSource;

    [Header("Cinemachine Virtual Cameras")]
    [Range(0f, 1f)] public float blendAtProgress = 0.25f;
    public CinemachineVirtualCamera outsideLiftVCam;
    public CinemachineVirtualCamera playerVCam;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        liftAudioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        void OnLiftReached(ITween<float> t)
        {
            // toggle the animation of opening the lift doors
            liftAnimator.SetTrigger("Open");

            // allow the player to move around and fire stuff
            player.enabled = true;
            player.weapon.gameObject.SetActive(true);
            player.rigidbody.isKinematic = false;
            player.slowmo.gameObject.SetActive(true);

            // start the game time's clock since the player can now do stuff
            GameManager.currentInstance.StartGameTime();

            // unparent the player
            player.transform.SetParent(null, true);

            // play lift reached sound, as well as the doors opening
            liftAudioSource.PlayOneShot(liftReachedSound, 1.0f);
            liftAudioSource.PlayOneShot(liftDoorSound, 1.0f);
        }

        void OnLiftTravelling(ITween<float> t)
        {
            transform.position = new Vector3(transform.position.x, t.CurrentValue, transform.position.z);

            if (t.CurrentProgress >= blendAtProgress && outsideLiftVCam.enabled)
            {
                playerVCam.enabled = true;
                outsideLiftVCam.enabled = false;
            }
        }

        gameObject.Tween(null, startElevation, targetElevation, elevationDuration, TweenScaleFunctions.CubicEaseInOut,
            OnLiftTravelling,
            OnLiftReached,
            true
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
