using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPickup : Pickup
{
    public GameObject goalPickupMain;   // to prevent rotating the collider, the model of the goal is placed as a child
    public bool isActivated = false;
    public int destroysToActivate = 2;

    [Header("Inside Cube Materials")]
    public Material deactivatedMaterial;
    public Material activatedMaterial;

    [Header("Animation Stuff")]
    public Vector3 startRotationEuler;
    public float rotateAmount = 10f;

    [Header("Component References")]
    public MeshRenderer meshRenderer;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        if (isActivated)
        {
            goalPickupMain.transform.eulerAngles = startRotationEuler;
        }
    }

    private void Update()
    {
        if (isActivated)
        {
            goalPickupMain.transform.Rotate(Vector3.up, rotateAmount * Time.deltaTime, Space.World);
        }
    }

    // This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    private void OnValidate()
    {
        ChangeInsideCubeMaterial(isActivated ? activatedMaterial : deactivatedMaterial, 1);
    }

    protected override void OnPickup(GameObject picker)
    {
        if (isActivated)
        {
            // is player if there is a player controller
            if (picker.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.slowmo.CurrentSlowmoState == Slowmo.SlowmoState.Inactive)
                {
                    Destroy(picker);
                    GameManager.currentInstance.GameSuccess();
                }
            }

        }
    }

    /// <summary>
    /// Updates the material used by the cube inside the goal pickup.
    /// </summary>
    private void ChangeInsideCubeMaterial(Material newMaterial, int index)
    {
        // because the sharedMaterials property returns a copy of an array
        // we need to use that array, make modifications to it, then set it back
        // to the property in order to effectively change the material
        Material[] materialsUsed = meshRenderer.sharedMaterials;
        materialsUsed[index] = newMaterial;
        meshRenderer.sharedMaterials = materialsUsed;
    }

    public void Activate()
    {
        isActivated = true;
        goalPickupMain.transform.eulerAngles = startRotationEuler;
        ChangeInsideCubeMaterial(activatedMaterial, 1);
    }

    public void FulfillRequirement()
    {
        destroysToActivate--;

        if (destroysToActivate <= 0)
        {
            Activate();
        }
    }
}
