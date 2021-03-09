using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using DigitalRuby.Tween;    // for tweening between colors for emission

public class KamikazeEnemy : Enemy
{
    private enum AIState { Idle, Pursuing, AboutToExplode, MovingToLastKnownPosition };

    [Space]
    public Transform target;
    public float health = 100f;
    public int pointsWorth = 25;
    private AIState currentAIState = AIState.Idle;

    [Header("AI Ranges")]
    public float pursueRange = 12f;         // distance from target where enemy starts chasing
    public float toExplodeRange = 3f;       // distance from target where enemy starts fuse
    public float getOutRange = 5f;          // distance from target where enemy stops fuse and chases target

    [Header("Timings")]
    public float explodingDuration = 2.5f;
    private float timeLeftTillExplosion;

    [Header("Explosive Properties")]
    public float explosionRange = 5f;
    public float minDamage = 10f;
    public float maxDamage = 40f;

    [Header("Explosion Object")]
    public GameObject explosion;
    [Range(0f, 2f)] public float minPitch_Explosion = 0.8f;
    [Range(0f, 2f)] public float maxPitch_Explosion = 1.2f;

    [Header("Death Explosion Object")]
    public Explosion deathExplosion;

    [Header("Emission")]
    public Color transitToColor = Color.red;
    public float intensity = 5f;

    [Header("Component References")]
    public NavMeshAgent agent;
    public MeshRenderer meshRenderer;

    private float sqrDistanceToTarget = 0f;
    private ColorTween emissionColorTween;

    // Start is called before the first frame update
    private void Start()
    {
        agent.isStopped = true;
        timeLeftTillExplosion = explodingDuration;
        timeTillNextGeneralCheck = generalCheckDelay;

        // enables the keyword to be used in this script
        meshRenderer.material.EnableKeyword("_EMISSION");
    }

    // Update is called once per frame
    private void Update()
    {
        if (target)
        {
            // perform stuff at intervals similar to how frequent a line-of-sight check is done
            // do line-of-sight check and set agent's destination to target
            PerformActionsAtIntervals();

            // executes code to handle what happens to the AI in its state
            // and when to switch to a new state
            HandleAIStates();
        }
        else
        {
            if (emissionColorTween != null)
            {
                emissionColorTween.Stop(TweenStopBehavior.DoNotModify);
            }
            currentAIState = AIState.Idle;
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(agent.destination, Vector3.one);
    }

    private void PerformActionsAtIntervals()
    {
        // helps to countdown the variable, independent of frame rate
        timeTillNextGeneralCheck -= Time.deltaTime;

        if (timeTillNextGeneralCheck <= 0)
        {
            // calculate the square distance between the enemy and the target
            // we use square distance instead of distance because
            // we are just comparing distances
            // so we are allowed to skip the lengthy square root operation
            sqrDistanceToTarget = Vector3.SqrMagnitude(target.position - transform.position);

            if (sqrDistanceToTarget <= pursueRange * pursueRange)
            {
                // perform raycast checks first, then agent set destination
                isTargetWithinView = PerformLineOfSightCheck(target.GetInstanceID(), target.position, pursueRange);
            }

            if (isTargetWithinView)
            {
                // sets the destination of the agent to the target's position
                // this causes the agent to first, calculate a path to the target
                // if a path is found, the agent moves to its destination
                agent.SetDestination(target.position);
            }

            timeTillNextGeneralCheck = generalCheckDelay;
        }
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
            }

            Explosion deathExplosionInstance = Instantiate(deathExplosion, transform.position, Quaternion.identity);
            deathExplosionInstance.particleColor = meshRenderer.material.GetColor("_Color");

            if (emissionColorTween != null)
            {
                emissionColorTween.Stop(TweenStopBehavior.DoNotModify);
            }

            AttemptDropPickup();

            return true;
        }

        return false;
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
                    agent.isStopped = false;
                }
                break;

            case AIState.Pursuing:

                if (isTargetWithinView && sqrDistanceToTarget <= toExplodeRange * toExplodeRange)
                {
                    currentAIState = AIState.AboutToExplode;
                    agent.isStopped = true; // stop moving

                    //emissionColorTween.Start();
                    emissionColorTween = gameObject.Tween(null, Color.black, transitToColor * intensity, explodingDuration, TweenScaleFunctions.Linear, (ITween<Color> t) => meshRenderer.material.SetColor("_EmissionColor", t.CurrentValue));
                }
                else if (!isTargetWithinView)
                {
                    currentAIState = AIState.MovingToLastKnownPosition;
                    agent.SetDestination(target.position);  // the destination here will be the position where we last saw the player
                }

                break;

            case AIState.AboutToExplode:

                timeLeftTillExplosion -= Time.deltaTime;

                if (timeLeftTillExplosion <= 0)
                {
                    if (sqrDistanceToTarget <= explosionRange * explosionRange)
                    {
                        if (target.TryGetComponent(out IDamageable damageable))
                        {
                            damageable.Damage(Mathf.Lerp(maxDamage, minDamage, sqrDistanceToTarget / (explosionRange * explosionRange)), gameObject);
                        }
                    }

                    GameObject explosionInstance = Instantiate(explosion, transform.position, Quaternion.identity);
                    explosionInstance.GetComponent<AudioSource>().pitch = Random.Range(minPitch_Explosion, maxPitch_Explosion);

                    // TODO: Use a better way to check how long to wait before destroying the explosion, since GetComponent isn't entirely reliable
                    Destroy(explosionInstance, Mathf.Max(explosionInstance.GetComponent<ParticleSystem>().main.duration, explosionInstance.GetComponent<AudioSource>().clip.length));

                    Destroy(gameObject);
                }
                else
                {
                    if (isTargetWithinView && sqrDistanceToTarget > getOutRange * getOutRange)
                    {
                        currentAIState = AIState.Pursuing;
                        agent.isStopped = false;    // start moving to chase target
                        timeLeftTillExplosion = explodingDuration;
                        emissionColorTween.Stop(TweenStopBehavior.DoNotModify);
                        meshRenderer.material.SetColor("_EmissionColor", Color.black);
                    }
                    else if (!isTargetWithinView)
                    {
                        emissionColorTween.Stop(TweenStopBehavior.DoNotModify);
                        meshRenderer.material.SetColor("_EmissionColor", Color.black);
                        currentAIState = AIState.MovingToLastKnownPosition;
                        agent.isStopped = false;    // start moving to move to position
                        timeLeftTillExplosion = explodingDuration;
                    }
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
}
