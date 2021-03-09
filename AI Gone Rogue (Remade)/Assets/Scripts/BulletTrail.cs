using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.Tween;    // for transitioning opacity of the trail

public class BulletTrail : MonoBehaviour
{
    public float speed = 80f;
    public Vector3 destination;
    private bool hasReachedDestination = false;
    public float fadeOutDuration = 1f;

    [Header("Component References")]
    public TrailRenderer trailRenderer;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        trailRenderer.time = fadeOutDuration;

        void FadeOutTrail(ITween<float> t)
        {
            Color trailColor = trailRenderer.startColor;
            trailRenderer.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, t.CurrentValue);
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, t.CurrentValue);
        }

        gameObject.Tween(null, 1f, 0f, fadeOutDuration, TweenScaleFunctions.Linear, FadeOutTrail);
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        if (!hasReachedDestination)
        {
            if (WillTrailExceedDestination(transform.forward * speed * Time.deltaTime, destination - transform.position))
            {
                // teleport this gameObject to its destination
                transform.position = destination;
                hasReachedDestination = true;
                trailRenderer.emitting = false;
            }
            else
            {
                // move like usual
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
    }

    private bool WillTrailExceedDestination(Vector3 toNextPosition, Vector3 toDestination)
    {
        // using square magnitudes here because it is much faster than finding the magnitude itself
        // since it avoids the square root operation
        return toNextPosition.sqrMagnitude > toDestination.sqrMagnitude;
    }
}
