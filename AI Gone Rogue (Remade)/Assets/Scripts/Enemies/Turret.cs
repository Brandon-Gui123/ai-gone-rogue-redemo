using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Enemy
{
    public enum AIState { Idle, FacingTarget, Firing }

    [Space]
    public AIState currentAIState = AIState.Idle;

    public Transform target;
    public float detectionRange = 15f;
    public float turnSpeed = 1f;
    public float health = 80f;
    public int pointsWorth = 250;

    [Header("Movable Parts")]
    public GameObject turretHead;
    public GameObject turretBarrel;

    [Header("Other References")]
    public GoalPickup goalPickup;

    [Header("Death Explosion")]
    public Explosion deathExplosion;
    public Color explosionParticleColor;

    private float sqrDistanceToTarget;
    private bool aimedAtTarget;

    // Start is called before the first frame update
    private void Start()
    {
        timeTillNextGeneralCheck = generalCheckDelay;
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
            if (armedWeapon.IsFiring)
            {
                armedWeapon.StopFiring();
            }
            currentAIState = AIState.Idle;
        }
    }

    private void PerformActionsAtIntervals()
    {
        timeTillNextGeneralCheck -= Time.deltaTime;

        if (timeTillNextGeneralCheck <= 0)
        {
            // calculate the square distance between the enemy and its target
            // much faster than using distance since we are just comparing and
            // can thus, avoid the lengthy square root operation
            sqrDistanceToTarget = Vector3.SqrMagnitude(target.position - transform.position);

            // check for a line-of-sight to the target if it is within range
            // saves costs since we don't check when player is out of range
            if (sqrDistanceToTarget <= detectionRange * detectionRange)
            {
                isTargetWithinView = PerformLineOfSightCheck(target.GetInstanceID(), target.position, detectionRange);
            }

            if (isTargetWithinView)
            {
                // check if the turret's barrel is aligned with the target
                aimedAtTarget = IsAimedAtTarget();
            }

            timeTillNextGeneralCheck = generalCheckDelay;
        }
    }

    private void HandleAIStates()
    {
        switch (currentAIState)
        {
            case AIState.Idle:

                if (isTargetWithinView)
                {
                    currentAIState = AIState.FacingTarget;
                }
                break;

            case AIState.FacingTarget:
                LookAtTarget();

                if (aimedAtTarget)
                {
                    currentAIState = AIState.Firing;
                    armedWeapon.StartFiring();
                }
                else if (!isTargetWithinView)
                {
                    currentAIState = AIState.Idle;
                }
                break;

            case AIState.Firing:
                LookAtTarget();
                if (!aimedAtTarget)
                {
                    currentAIState = AIState.FacingTarget;
                    armedWeapon.StopFiring();
                }
                else if (!isTargetWithinView)
                {
                    currentAIState = AIState.Idle;
                    armedWeapon.StopFiring();
                }
                break;
        }
    }

    private bool IsAimedAtTarget()
    {
        if (Physics.Raycast(turretHead.transform.position, turretHead.transform.forward, out RaycastHit hitInfo, detectionRange, GetLayerMaskFromLayerInt(target.gameObject.layer)))
        {
            return hitInfo.transform.GetInstanceID() == target.GetInstanceID();
        }
        else
        {
            return false;
        }
    }

    private void LookAtTarget()
    {
        Vector3 targetDirection = Vector3.RotateTowards(turretHead.transform.forward, target.position - turretHead.transform.position, turnSpeed * Time.deltaTime, 0.0f);
        turretHead.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
    }

    public override bool Damage(float damage, GameObject damager, WeaponType weaponType = WeaponType.None)
    {
        if (weaponType == WeaponType.AntiTurret)
        {
            health -= damage;

            if (health <= 0)
            {
                Destroy(gameObject);
                GameManager.currentInstance.Score += pointsWorth;
                GameManager.currentInstance.IncrementEnemyKillCount();

                if (goalPickup)
                {
                    // each time we destroy the turret near the end goal, we are fulfilling a requirement to activate the pickup
                    goalPickup.FulfillRequirement();
                }

                Explosion deathExplosionInstance = Instantiate(deathExplosion, transform.position, Quaternion.identity);
                deathExplosionInstance.particleColor = explosionParticleColor;

                return true;
            }
        }

        return false;
    }

    private LayerMask GetLayerMaskFromLayerInt(int layerInt)
    {
        return 1 << layerInt;
    }
}
