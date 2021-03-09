using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [Range(0f, 1f)] public float followInterpolant;
    public bool forceZeroOffset = false;

    // Start is called before the first frame update
    private void Start()
    {
        if (forceZeroOffset)
        {
            offset = Vector3.zero;
        }
        else
        {
            // calculate the position difference between the camera and the target
            CalculateOffset();
        }
    }

    // LateUpdate is called every frame, if the Behaviour is enabled
    private void LateUpdate()
    {
        if (!target)
        {
            enabled = false;
            return;
        }

        // calculate the camera's destination
        Vector3 camDestination = target.position - offset;

        // linearly interpolate positions between the camera's destination and the camera's current position
        transform.position = Vector3.Lerp(transform.position, camDestination, followInterpolant);

    }

    public void CalculateOffset()
    {
        offset = target.position - transform.position;
    }

}