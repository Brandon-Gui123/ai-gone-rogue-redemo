using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;       // for accessing the NavMeshObstacle component

public class Door : MonoBehaviour
{
    public bool locked;
    public bool CurrentlyLocked { get; private set; }
    public bool DoorMoving { get; set; } = false;

    public bool IsOpen { get; set; } = false;

    [Header("Emissive Materials")]
    public Material autoOpenDoorMaterial;
    public Material lockedDoorMaterial;
    public Material openDoorMaterial;

    [Header("Proximity Check")]
    public Vector3 boxSize;
    public float checkPeriod = 0.25f;
    private float timeLeftTillNextCheck;
    public LayerMask layersToCheck;
    private Vector3 boxCenter;
    private bool hasEntityNearby;

    [Header("Audio Clips")]
    public AudioClip doorUnlockClip;

    [Header("Component References")]
    public MeshRenderer meshRenderer;
    public AudioSource audioSource;
    public Animator animator;

    // Start is called before the first frame update
    private void Start()
    {
        CurrentlyLocked = locked;

        if (CurrentlyLocked)
        {
            ChangeLockIndicationMaterial(lockedDoorMaterial, 1);
        }
        else
        {
            ChangeLockIndicationMaterial(autoOpenDoorMaterial, 1);
        }

        timeLeftTillNextCheck = checkPeriod;

        boxCenter = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        // only do checking when the door is not opening or closing
        // since it is unnecessary and we aren't going to change the door's state
        // while it is opening or closing until it is done
        if (!DoorMoving && !CurrentlyLocked)
        {
            DoProximityCheck(ref hasEntityNearby);
        }

        if (!DoorMoving)
        {
            float currentHeight = transform.position.y;

            if (hasEntityNearby && !IsOpen)
            {
                audioSource.Play();
                animator.SetBool("isOpened", true);
                ChangeLockIndicationMaterial(openDoorMaterial, 1);
            }
            else if (!hasEntityNearby && IsOpen)
            {
                audioSource.Play();
                animator.SetBool("isOpened", false);
                IsOpen = false;
            }
        }
    }

    // This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    private void OnValidate()
    {
        if (locked)
        {
            ChangeLockIndicationMaterial(lockedDoorMaterial, 1);
        }
        else
        {
            ChangeLockIndicationMaterial(autoOpenDoorMaterial, 1);
        }
    }

    public void ChangeLockIndicationMaterial(Material material, int index)
    {
        // the shared materials property returns a copy of the array of materials used
        // so we can't just change directly using the index
        // we need to get the array, change an element, and pass it back into the property
        Material[] materials = meshRenderer.sharedMaterials;
        materials[index] = material;
        meshRenderer.sharedMaterials = materials;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.6f);

        if (Application.isEditor && !Application.isPlaying)
        {
            Gizmos.DrawCube(transform.position, boxSize);
        }
        else
        {
            Gizmos.DrawCube(boxCenter, boxSize);
        }
    }

    /// <summary>
    /// Checks around the door for any nearby entities.
    /// Note that this method requires a reference to a boolean so as to alter that boolean. The reason why we use "ref" is because
    /// returning a boolean or using an "out" doesn't make sense as we must always provide the value of the boolean every time the method runs.
    /// This means if the line-of-sight check is not completed due to the checking delay, the output will be incorrect.
    /// Hence, by using a ref, we can alter it whenever the output is given and not on every method call.
    /// </summary>
    /// <param name="hasEntityNearby">A reference boolean parameter.</param>
    private void DoProximityCheck(ref bool hasEntityNearby)
    {
        timeLeftTillNextCheck -= Time.deltaTime;

        if (timeLeftTillNextCheck <= 0)
        {
            hasEntityNearby = Physics.CheckBox(boxCenter, boxSize / 2, transform.rotation, layersToCheck);
            timeLeftTillNextCheck = checkPeriod;
        }
    }

    public void Unlock()
    {
        audioSource.PlayOneShot(doorUnlockClip, 1.0f);
        CurrentlyLocked = false;
        ChangeLockIndicationMaterial(autoOpenDoorMaterial, 1);
    }
}
