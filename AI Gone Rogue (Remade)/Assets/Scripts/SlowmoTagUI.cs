using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DigitalRuby.Tween;    // for animations

public class SlowmoTagUI : MonoBehaviour
{
    public Image image;
    public Vector2 startScale;
    public float shrinkTime = 0.25f;
    [Space]
    public Vector2 expandScale;
    public float expandTime = 0.5f;

    // Start is called before the first frame update
    private void Start()
    {
        transform.localScale = startScale;

        void decrementSize(ITween<Vector3> t)
        {
            transform.localScale = t.CurrentValue;
        }

        gameObject.Tween(null, transform.localScale, Vector3.one, shrinkTime, TweenScaleFunctions.CubicEaseOut, decrementSize, null, false);

    }

    public void Remove()
    {
        void incrementSize(ITween<Vector3> t)
        {
            transform.localScale = t.CurrentValue;
        }

        gameObject.Tween(null, transform.localScale, expandScale, expandTime, TweenScaleFunctions.CubicEaseOut, incrementSize, (ITween<Vector3> t) => Destroy(gameObject), false);

        image.CrossFadeAlpha(0f, expandTime, true);
    }
}
