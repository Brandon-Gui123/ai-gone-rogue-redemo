using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected Weapon armedWeapon;
    [SerializeField] protected Pickup pickup;

    // general checks may include checking line-of-sight to player
    // this check variable here just provides a convenient way to
    // check for certain stuff at a specified interval
    [Header("General Checks")]
    [Tooltip("Whether to let the general check time run and thus, check for certain things at a certain interval")]
    [SerializeField] protected bool doGeneralCheck = true;
    [Tooltip("The number of seconds between general checkings")]
    [SerializeField] protected float generalCheckDelay = 0.25f;
    protected float timeTillNextGeneralCheck;

    [Header("Line-of-sight Checking")]
    [Tooltip("Layers to detect in the line-of-sight checking from enemy to target.")]
    [SerializeField] protected LayerMask layersToDetect;
    [Tooltip("Will triggers block the enemy's line of sight?\n\nUse Global - Use interaction specified in global Physics.queriesHitTriggers settings\nCollide - Let triggers block enemy's sight\nIgnore - Allow enemies to see through triggers")]
    [SerializeField] protected QueryTriggerInteraction interactionWithTriggers = QueryTriggerInteraction.UseGlobal;

    [Header("Pickup Drop")]
    [Tooltip("Chance to drop the assigned pickup upon death.")]
    [SerializeField, Range(0f, 1f)] protected float pickupDropChance = 0.25f;

    /// <summary>
    /// Whether or not the target is within range and the enemy has line-of-sight to it.
    /// </summary>
    protected bool isTargetWithinView;

    public abstract bool Damage(float damage, GameObject damager, WeaponType weaponType = WeaponType.None);

    public bool HasArmedWeapon()
    {
        return armedWeapon != null;
    }

    public void InheritLayerSettingsFromWeapon()
    {
        layersToDetect = armedWeapon.LayersToHit;
        interactionWithTriggers = armedWeapon.InteractionWithTriggers;
    }

    protected virtual bool PerformLineOfSightCheck(int targetInstanceID, Vector3 targetPosition, float raycastDistance)
    {
        if (Physics.Raycast(transform.position, targetPosition - transform.position, out RaycastHit hitInfo, raycastDistance, layersToDetect, interactionWithTriggers))
        {
            // we compare instance IDs to check if two or more references refer to the same instance
            return hitInfo.transform.GetInstanceID() == targetInstanceID;
        }
        else
        {
            return false;
        }
    }

    protected virtual void AttemptDropPickup()
    {
        if (!pickup)
        {
            return;
        }

        float randomNum = Random.Range(0f, 1f);
        if (randomNum <= pickupDropChance)
        {
            Instantiate(pickup, transform.position, Quaternion.identity);
        }
    }
}
