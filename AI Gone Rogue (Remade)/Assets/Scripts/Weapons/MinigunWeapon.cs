using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigunWeapon : Weapon
{
    [Space]
    public float range = 15f;
    public float damage = 2f;

    [Header("Timings")]
    public float chargeUpTime = 1f;
    private float timeBeforeFiring;
    public float fireRate = 5f;
    private float firePeriod;
    private float timeTillNextFire;
    private bool canFire = false;

    [Header("Bullet Trail")]
    public float trailSpeed = 80f;
    public float trailFadeOutDuration = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip audioClip;

    [Header("References")]
    public BulletTrail bulletTrail;
    public GameObject weaponUser;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        firePeriod = 1 / fireRate;
        timeTillNextFire = firePeriod;
        timeBeforeFiring = chargeUpTime;
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        if (IsFiring)
        {
            if (!canFire)
            {
                // countdown charge time
                timeBeforeFiring -= Time.deltaTime;

                if (timeBeforeFiring <= 0)
                {
                    canFire = true;
                }
            }
            else
            {
                timeTillNextFire -= Time.deltaTime;

                if (timeTillNextFire <= 0)
                {
                    audioSource.PlayOneShot(audioClip, 1.0f);

                    if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, range, LayersToHit, interactionWithTriggers))
                    {
                        BulletTrail instance = Instantiate(bulletTrail, transform.position, transform.rotation);
                        instance.destination = hitInfo.point;
                        instance.speed = trailSpeed;
                        instance.fadeOutDuration = trailFadeOutDuration;

                        if (IsHitLayerDamageable(hitInfo.transform.gameObject.layer) && hitInfo.transform.TryGetComponent(out IDamageable damageable))
                        {
                            damageable.Damage(damage, weaponUser);
                        }
                    }
                    else
                    {
                        BulletTrail instance = Instantiate(bulletTrail, transform.position, transform.rotation);
                        instance.speed = trailSpeed;
                        instance.fadeOutDuration = trailFadeOutDuration;
                        instance.destination = transform.position + transform.forward * range;
                    }

                    timeTillNextFire = firePeriod;
                }
            }
        }
    }

    protected override void OnStartFiring()
    {
        firePeriod = 1 / fireRate;
        timeTillNextFire = firePeriod;
        timeBeforeFiring = chargeUpTime;
    }

    protected override void OnStopFiring()
    {
        canFire = false;
    }
}
