using UnityEngine;

public class ChargeAndFireWeapon : Weapon
{
    [Space]
    public float range = 25f;
    public float damage = 25f;

    [Header("Timings")]
    public float chargeTime = 5f;
    public float timeLeftTillNextFire;
    public float timeWhenAboutToFire = 1f;

    [Header("Aim Line Materials")]
    public Material aimingLineMaterial;
    public Material aboutToFireLineMaterial;
    private bool materialChanged = false;

    [Header("Bullet Trail")]
    public float trailFadeOutDuration = 2f;
    public float trailSpeed = 80f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireAudioClip;

    [Header("Component References")]
    public LineRenderer aimingLine;
    public GameObject weaponUser;
    public BulletTrail bulletTrail;

    // Start is called before the first frame update
    private void Start()
    {
        timeLeftTillNextFire = chargeTime;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsFiring)
        {
            timeLeftTillNextFire -= Time.deltaTime;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, range, LayersToHit, interactionWithTriggers))
            {
                aimingLine.SetPosition(1, new Vector3(0, 0, hitInfo.distance));

                if (!materialChanged && timeLeftTillNextFire <= timeWhenAboutToFire)
                {
                    aimingLine.material = aboutToFireLineMaterial;
                    materialChanged = true;
                }

                if (timeLeftTillNextFire <= 0)
                {
                    audioSource.PlayOneShot(fireAudioClip, 1.0f);

                    BulletTrail instance = Instantiate(bulletTrail, transform.position, transform.rotation);
                    instance.speed = trailSpeed;
                    instance.fadeOutDuration = trailFadeOutDuration;
                    instance.destination = hitInfo.point;

                    if (IsHitLayerDamageable(hitInfo.transform.gameObject.layer) && hitInfo.transform.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.Damage(damage, weaponUser, weaponType);
                    }

                    timeLeftTillNextFire = chargeTime;

                    if (materialChanged)
                    {
                        aimingLine.material = aimingLineMaterial;
                    }

                    materialChanged = false;
                }
            }
            else
            {
                aimingLine.SetPosition(1, new Vector3(0, 0, range));

                if (!materialChanged && timeLeftTillNextFire <= timeWhenAboutToFire)
                {
                    aimingLine.material = aboutToFireLineMaterial;
                }
                
                if (timeLeftTillNextFire <= 0)
                {
                    BulletTrail instance = Instantiate(bulletTrail, transform.position, transform.rotation);
                    instance.speed = trailSpeed;
                    instance.fadeOutDuration = trailFadeOutDuration;
                    instance.destination = transform.position + transform.forward * range;

                    timeLeftTillNextFire = chargeTime;

                    if (materialChanged)
                    {
                        aimingLine.material = aimingLineMaterial;
                    }

                    materialChanged = false;
                }
            }
        }
    }

    protected override void OnStartFiring()
    {
        aimingLine.enabled = true;
    }

    protected override void OnStopFiring()
    {
        aimingLine.enabled = false;
        timeLeftTillNextFire = chargeTime;

        if (materialChanged)
        {
            aimingLine.material = aimingLineMaterial;
        }

        materialChanged = false;
    }

}
