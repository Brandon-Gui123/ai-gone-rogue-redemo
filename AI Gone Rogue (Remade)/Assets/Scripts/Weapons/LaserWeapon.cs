using UnityEngine;

public class LaserWeapon : Weapon
{
    [Space]
    public float range = 18f;
    public float damage = 5f;
    public Material antiTurretFireMaterial;
    public Material antiTurretAimMaterial;

    [Header("Damage Rate")]
    public float damagePeriod = 0.1f;
    private float timeTillNextDamage;
    public bool useScaledTime = false;

    [Header("Component References")]
    public bool useAimingGuide = true;
    public LineRenderer aimingGuide;
    public LineRenderer laserFire;
    public ParticleSystem impactParticles;
    public GameObject weaponUser;
    public AudioSource weaponAudio;
    public AudioSource impactAudio;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        WeaponTypeChangedEvent += OnWeaponTypeChanged;
        timeTillNextDamage = damagePeriod;
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        // if we use an aiming guide, we perform a raycast every frame so as to update the aiming guide's length
        // else, we only raycast when the weapon fires so that we update just the laser fire's length
        if (useAimingGuide)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, range, LayersToHit, interactionWithTriggers))
            {
                if (IsFiring)
                {
                    if (!impactAudio.isPlaying)
                    {
                        impactAudio.Play();
                    }

                    UpdateLaserFire(hitInfo.point, hitInfo.distance, true);
                    DoDamageCycle(hitInfo.transform);
                }
                else
                {
                    aimingGuide.SetPosition(1, new Vector3(0, 0, hitInfo.distance));
                }
            }
            else
            {
                if (IsFiring)
                {
                    if (impactAudio.isPlaying)
                    {
                        impactAudio.Stop();
                    }

                    UpdateLaserFire(Vector3.zero, range, false);
                }
                else
                {
                    aimingGuide.SetPosition(1, new Vector3(0, 0, range));
                }
            }
        }
        else
        {
            if (IsFiring)
            {
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, range))
                {
                    UpdateLaserFire(hitInfo.point, hitInfo.distance, true);
                    DoDamageCycle(hitInfo.transform);
                }
                else
                {
                    UpdateLaserFire(Vector3.zero, range, false);
                }
            }
        }
    }

    private void DoDamageCycle(Transform hitTransform)
    {
        timeTillNextDamage -= (useScaledTime) ? Time.deltaTime : Time.unscaledDeltaTime;

        if (timeTillNextDamage <= 0)
        {
            if (IsHitLayerDamageable(hitTransform.gameObject.layer) && hitTransform.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(damage, weaponUser, weaponType);
            }

            timeTillNextDamage = damagePeriod;
        }
    }

    private void UpdateLaserFire(Vector3 impactPosition, float distance, bool startParticles)
    {
        laserFire.SetPosition(1, new Vector3(0, 0, distance));
        impactParticles.transform.position = impactPosition;

        if (startParticles)
        {
            if (impactParticles.isStopped || !impactParticles.isEmitting)
            {
                impactParticles.Play();
            }
        }
        else
        {
            if (!impactParticles.isStopped)
            {
                impactParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    protected override void OnStartFiring()
    {
        aimingGuide.enabled = false;
        laserFire.enabled = true;
        weaponAudio.Play();
    }

    protected override void OnStopFiring()
    {
        aimingGuide.enabled = true;
        laserFire.enabled = false;

        if (impactParticles.isEmitting)
        {
            impactParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        timeTillNextDamage = damagePeriod;
        weaponAudio.Stop();
        impactAudio.Stop();
    }

    private void OnWeaponTypeChanged(WeaponType oldType, WeaponType newType)
    {
        if (newType == WeaponType.AntiTurret)
        {
            laserFire.material = antiTurretFireMaterial;
            aimingGuide.material = antiTurretAimMaterial;
        }
    }
}
