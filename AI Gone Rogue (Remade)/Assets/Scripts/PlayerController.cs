using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using DigitalRuby.Tween;    // for transitions. Unity package courtesy of Jeff Johnson. Such a super small package at less than 40 KB!

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float speed;
    private Plane playerPlane;  // a horizontally-flat rectangle at the player's position, facing upwards
    private float resultRotationY;
    private Vector3 resultMovement;
    public Weapon weapon;

    [Space]
    public float regenStartDelay = 10f;
    private float timeTillRegenStarts;
    public float regenAmount = 5f;

    [Header("Unlocking Doors")]
    public string unlockButton = "UnlockDoors";
    public float unlockRadius = 1f;
    public LayerMask unlockLayers;

    [Header("Component References")]
    public new Camera camera;
    public new Rigidbody rigidbody;
    public Slowmo slowmo;

    [Header("Post-process")]
    public PostProcessVolume playerHealthIndication;
    public PostProcessVolume playerHurt;

    [Header("Player Hurt Sounds")]
    public AudioSource playerAudio;
    public AudioClip[] playerHurtClips;

    [Header("Particle Systems")]
    public ParticleSystem deathParticles;

    // Awake is called when the script instance is loaded
    private void Awake()
    {
        // automatically assign new inputs if not assigned
        if (!camera)
            camera = Camera.main;
        if (!rigidbody && !TryGetComponent(out rigidbody))
            Debug.LogError("No Rigidbody component attached!");

        playerPlane = new Plane(Vector3.up, transform.position);
        resultRotationY = rigidbody.rotation.eulerAngles.y;
        currentHealth = maxHealth;
        timeTillRegenStarts = regenStartDelay;
    }

    // Start is called once before Update
    private void Start()
    {
        slowmo.SlowmoEndEvent += OnSlowmoEnd;
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        switch (slowmo.CurrentSlowmoState)
        {
            case Slowmo.SlowmoState.Slowdown:
                resultMovement = Move();
                resultRotationY = LookAtCursor().eulerAngles.y;
                if (Input.GetMouseButtonDown(0))
                {
                    slowmo.MarkTarget(transform);
                }
                break;

            case Slowmo.SlowmoState.Executing:
                resultMovement = Vector3.zero;
                resultRotationY = Quaternion.LookRotation(slowmo.GetCurrentTagPosition() - transform.position, Vector3.up).eulerAngles.y;
                if (!weapon.IsFiring)
                {
                    weapon.StartFiring();
                }
                break;

            case Slowmo.SlowmoState.Inactive:
                resultMovement = Move();
                FireWeapon();
                resultRotationY = LookAtCursor().eulerAngles.y;
                break;
        }

        timeTillRegenStarts -= Time.deltaTime;

        if (timeTillRegenStarts <= 0)
        {
            Regenerate(regenAmount * Time.deltaTime);
        }

        if (Input.GetButtonDown(slowmo.inputButtonName))
        {
            if (weapon.IsFiring)
            {
                weapon.StopFiring();
            }

            slowmo.ToggleSlowmo();
        }

        if (Input.GetButtonDown(unlockButton) && slowmo.CurrentSlowmoState == Slowmo.SlowmoState.Inactive)
        {
            UnlockSurroundingDoors();
        }
    }

    private void FireWeapon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weapon.StartFiring();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weapon.StopFiring();
        }
    }

    // FixedUpdate is called at a regular interval
    private void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + resultMovement * Time.fixedDeltaTime);
        rigidbody.MoveRotation(Quaternion.Euler(0, resultRotationY, 0));
    }

    private Vector3 Move()
    {
        return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed;
    }

    private Quaternion LookAtCursor()
    {
        Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(cameraRay, out float hitDistance))
        {
            Vector3 hitPoint = cameraRay.GetPoint(hitDistance);
            return Quaternion.LookRotation(hitPoint - transform.position, Vector3.up);
        }

        return rigidbody.rotation;
    }

    public bool Damage(float damage, GameObject damager, WeaponType weaponType = WeaponType.None)
    {
        // a local method that is called every time the tween progresses (we will pass it to the Tween method)
        void hurtTransition(ITween<float> t) => playerHurt.weight = t.CurrentValue;

        currentHealth -= damage;

        playerHealthIndication.weight = Mathf.Lerp(1f, 0f, currentHealth / maxHealth);

        timeTillRegenStarts = regenStartDelay;

        // transit from 0 to 1 in 0.1 seconds, then from 1 to 0 in 0.3 seconds, linearly for all transitions
        playerHurt.gameObject.Tween("PlayerHurtFXTransit", 0f, 1f, 0.1f, TweenScaleFunctions.Linear, hurtTransition)
            .ContinueWith(new FloatTween().Setup(1f, 0f, 0.3f, TweenScaleFunctions.Linear, hurtTransition));

        if (currentHealth <= 0)
        {
            slowmo.ForceTerminate();
            GameManager.currentInstance.GameOver();
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);

            return true;
        }
        else
        {
            // play a random player hurt audio clip
            if (!playerAudio.isPlaying)
            {
                playerAudio.PlayOneShot(playerHurtClips[Random.Range(0, playerHurtClips.Length)], 1.0f);
            }
        }

        return false;
    }

    public void Heal(float restoreValue)
    {
        currentHealth += restoreValue;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        playerHealthIndication.weight = Mathf.Lerp(1f, 0f, currentHealth / maxHealth);

    }

    private void Regenerate(float heal)
    {
        currentHealth += heal;

        playerHealthIndication.weight = Mathf.Lerp(1f, 0f, currentHealth / maxHealth);

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void UnlockSurroundingDoors()
    {
        if (GameManager.currentInstance.Keys <= 0)
        {
            GameManager.currentInstance.FlashKeyCount(3, 0.15f, Color.red);
            return;
        }

        Collider[] doorColliders = Physics.OverlapSphere(transform.position, unlockRadius, unlockLayers);

        for (int i = 0; i < doorColliders.Length; i++)
        {
            Door doorComponent = doorColliders[i].GetComponentInParent<Door>();

            if (doorComponent)
            {
                if (doorComponent.CurrentlyLocked)
                {
                    if (GameManager.currentInstance.Keys <= 0)
                    {
                        return;
                    }

                    doorComponent.Unlock();
                    GameManager.currentInstance.Keys--;
                }
            }
        }
    }

    private void OnSlowmoEnd()
    {
        weapon.StopFiring();
    }
}
