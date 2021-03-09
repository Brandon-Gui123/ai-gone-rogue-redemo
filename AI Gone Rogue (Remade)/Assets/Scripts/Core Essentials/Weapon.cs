using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public delegate void WeaponTypeChangedDelegate(WeaponType oldType, WeaponType newType);
    public event WeaponTypeChangedDelegate WeaponTypeChangedEvent;

    [Tooltip("Sync both the \"layers to hit\" layer mask with the \"layers to damage\" layer mask." +
        "\nEnsures that the former layer mask contains marked layers in the latter.")]
    [SerializeField] private bool syncLayerMasks = false;

    [SerializeField] protected WeaponType weaponType = WeaponType.None;

    [Tooltip("The layers that the weapon's projectiles or fire will hit.")]
    [SerializeField] protected LayerMask layersToHit;

    [Tooltip("How will the weapon fire interact with triggers?\n\nUse Global - Use interaction specified in global Physics.queriesHitTriggers settings\nCollide - Have weapon fire collide with triggers\nIgnore - Have weapon fire ignore and go through triggers")]
    [SerializeField] protected QueryTriggerInteraction interactionWithTriggers = QueryTriggerInteraction.UseGlobal;

    [Tooltip("The layers that this weapon's projectiles will hit and cause damage, possibly reducing health.")]
    [SerializeField] protected LayerMask layersToDamage;

    public bool IsFiring { get; private set; }

    public QueryTriggerInteraction InteractionWithTriggers {
        get => interactionWithTriggers; protected set => interactionWithTriggers = value;
    }

    public LayerMask LayersToHit { get => layersToHit; protected set => layersToHit = value; }

    public WeaponType WeaponType {
        get { return weaponType; }
        set
        {
            WeaponTypeChangedEvent?.Invoke(weaponType, value);
            weaponType = value;
        }
    }

    /// <summary>
    /// Sets <see cref="IsFiring"/> to <see langword="true"/> and calls <see cref="OnStartFiring"/> to commence weapon fire.
    /// </summary>
    public void StartFiring()
    {
        IsFiring = true;
        OnStartFiring();
    }

    /// <summary>
    /// Sets <see cref="IsFiring"/> to <see langword="false"/> and calls <see cref="OnStopFiring"/> to stop weapon fire.
    /// </summary>
    public void StopFiring()
    {
        IsFiring = false;
        OnStopFiring();
    }

    /// <summary>
    /// Is the layer of the collider that was hit applicable to what we've set for the <see cref="layersToDamage"/> layer mask?
    /// </summary>
    /// <param name="hitLayer">The layer integral of the hit collider. This is not the LayerMask of the hit collider.</param>
    /// <returns></returns>
    protected bool IsHitLayerDamageable(int hitLayer)
    {
        return layersToDamage == (layersToDamage | (1 << hitLayer));
    }

    protected abstract void OnStartFiring();
    protected abstract void OnStopFiring();

    // This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    private void OnValidate()
    {
        // to prevent possible loss of changes, only sync layers when specified to do so
        if (syncLayerMasks)
        {
            // a layer must be hit in order for it to be damaged
            // hence, this validation allows setting bitmask values
            // in the layersToHit to have ones in layersToDamage
            // the operator performs an OR operation between the two variables,
            // then assigns it to the one on the left
            LayersToHit |= layersToDamage;
        }
    }
}
