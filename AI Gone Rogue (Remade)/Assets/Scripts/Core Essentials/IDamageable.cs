using UnityEngine;

/// <summary>
/// Used to represent something that can be damaged by weapons, damage sources and enemies.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Damages the entity with the specified amount of damage and the weapon type of the weapon used.
    /// The script implementing this interface determines what happens.
    /// </summary>
    /// <param name="damage">The amount of damage to deal.</param>
    /// <param name="weaponType">The type of the weapon used (e.g. Laser, Explosive etc.). Defaults to WeaponType.None.</param>
    /// <param name="damager">The GameObject that damaged the player. It can be the GameObject that fired the damaging bullet.</param>
    /// <returns>A boolean indicating whether the object damaged is dead.</returns>
    bool Damage(float damage, GameObject damager, WeaponType weaponType = WeaponType.None);
}
