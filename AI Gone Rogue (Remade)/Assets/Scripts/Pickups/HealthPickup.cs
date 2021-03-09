using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    public float healthToRestore = 10f;

    protected override void OnPickup(GameObject picker)
    {
        if (picker.TryGetComponent(out PlayerController playerController))
        {
            playerController.Heal(healthToRestore);
            Destroy(gameObject);
        }
    }
}
