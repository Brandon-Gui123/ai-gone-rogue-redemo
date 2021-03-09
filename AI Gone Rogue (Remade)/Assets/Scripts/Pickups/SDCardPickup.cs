using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DigitalRuby.Tween;    // for animating the pickup upon picking it up

public class SDCardPickup : Pickup
{
    public int scoreGain = 150;
    [Space]
    public Vector3 expandToSize = new Vector3(2f, 2f, 2f);
    public float expandDuration = 0.5f;
    public float shrinkDuration = 0.5f;

    protected override void OnPickup(GameObject picker)
    {
        void ScalePickup(ITween<Vector3> t)
        {
            transform.localScale = t.CurrentValue;
        }

        gameObject.Tween(null, transform.localScale, expandToSize, expandDuration, TweenScaleFunctions.QuadraticEaseOut, ScalePickup)
                  .ContinueWith(new Vector3Tween().Setup(expandToSize, Vector3.zero, shrinkDuration, TweenScaleFunctions.QuadraticEaseIn, ScalePickup, (ITween<Vector3> t) => Destroy(gameObject)));

        if (picker.TryGetComponent(out PlayerController playerController))
        {
            GameManager.currentInstance.Score += scoreGain;
        }
    }
}
