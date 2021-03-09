using UnityEngine;
using UnityEngine.AI;

public class ChargeAndFireEnemy : Enemy
{
    private enum AIState { Idle, Pursuing, Firing, MovingToLastKnownPosition };

    [Space]
    public Transform target;
    public float health = 100f;
    public int pointsWorth = 50;
    private AIState currentAIState = AIState.Idle;
    private Quaternion resultRotation = (new Quaternion()).normalized;
    [Range(0f, 1f)] public float rotationInterpolation = 0.8f;

    [Header("AI Ranges")]
    public float pursueRange = 25f;
    public float firingRange = 15f;

    [Header("Death Explosion")]
    public Explosion deathExplosion;

    [Header("Component References")]
    public NavMeshAgent agent;
    public new Rigidbody rigidbody;
    public MeshRenderer meshRenderer;

    private float sqrDistanceToTarget = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        timeTillNextGeneralCheck = generalCheckDelay;
        agent.isStopped = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (target)
        {
            PerformActionsAtIntervals();
            HandleAIStates();
        }
        else
        {
            // there's no one to fire at so no point firing
            if (armedWeapon.IsFiring)
            {
                armedWeapon.StopFiring();
            }

            currentAIState = AIState.Idle;
        }
    }

    private void HandleAIStates()
    {
        switch (currentAIState)
        {
            case AIState.Idle:

                // no need to check distance here since our AI check above already checks that
                if (isTargetWithinView)
                {
                    currentAIState = AIState.Pursuing;
                    agent.enabled = true;
                }

                break;

            case AIState.Pursuing:

                if (isTargetWithinView && sqrDistanceToTarget <= firingRange * firingRange)
                {
                    currentAIState = AIState.Firing;

                    // disable agent to prevent race condition where aiming and agent's behaviour fight to control the rotation of this enemy
                    agent.enabled = false;

                    armedWeapon.StartFiring();          // start firing weapon
                    agent.velocity = Vector3.zero;
                }
                else if (!isTargetWithinView)
                {
                    currentAIState = AIState.MovingToLastKnownPosition;
                    agent.enabled = true;
                    agent.SetDestination(target.position);
                }

                break;

            case AIState.Firing:

                resultRotation = LookAtPosition(target.position);

                if (isTargetWithinView && sqrDistanceToTarget > firingRange * firingRange)
                {
                    currentAIState = AIState.Pursuing;
                    agent.enabled = true;
                    armedWeapon.StopFiring();
                }
                else if (!isTargetWithinView)
                {
                    currentAIState = AIState.MovingToLastKnownPosition;
                    agent.enabled = true;
                    agent.SetDestination(target.position);
                    armedWeapon.StopFiring();
                }

                break;

            case AIState.MovingToLastKnownPosition:

                if (isTargetWithinView)
                {
                    currentAIState = AIState.Pursuing;
                }
                break;
        }
    }

    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled
    private void FixedUpdate()
    {
        switch (currentAIState)
        {
            case AIState.Firing:
                rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, resultRotation, rotationInterpolation));
                break;
        }
    }

    private void PerformActionsAtIntervals()
    {
        // helps to countdown the variable, independent of frame rate
        timeTillNextGeneralCheck -= Time.deltaTime;

        if (timeTillNextGeneralCheck <= 0)
        {
            // calculate the square distance between the enemy and its target
            // much faster than using distance since we are just comparing and
            // can thus, avoid the lengthy square root operation
            sqrDistanceToTarget = Vector3.SqrMagnitude(target.position - transform.position);

            // only check if the target can be seen if it is near enough
            // saves on costs since we don't need to check line-of-sight
            // when the player is out of range
            if (sqrDistanceToTarget <= pursueRange * pursueRange)
            {
                isTargetWithinView = PerformLineOfSightCheck(target.GetInstanceID(), target.position, pursueRange);
            }

            if (isTargetWithinView && agent.enabled)
            {
                agent.SetDestination(target.position);
            }

            timeTillNextGeneralCheck = generalCheckDelay;
        }
    }

    private Quaternion LookAtPosition(Vector3 pos)
    {
        return Quaternion.LookRotation(pos - transform.position, Vector3.up);
    }

    public override bool Damage(float damage, GameObject damager, WeaponType weaponType = WeaponType.None)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
            if (damager.TryGetComponent(out PlayerController playerController))
            {
                playerController.slowmo.GainSlowmo();
                GameManager.currentInstance.Score += pointsWorth;
                GameManager.currentInstance.IncrementEnemyKillCount();
                Explosion deathExplosionInstance = Instantiate(deathExplosion, transform.position, Quaternion.identity);
                deathExplosionInstance.particleColor = meshRenderer.material.GetColor("_Color");
            }
            AttemptDropPickup();
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(agent.destination, Vector3.one);
    }
}
