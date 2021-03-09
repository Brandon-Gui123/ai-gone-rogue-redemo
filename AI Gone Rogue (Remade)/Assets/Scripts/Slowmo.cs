using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // for post-processing effects like the blue tint during slowmo
using UnityEngine.Audio;                    // for transitioning between auio mixer snapshots

using DigitalRuby.Tween;    // for transitions

public class Slowmo : MonoBehaviour
{
    private class SlowmoTag
    {
        public Transform TaggedTransform { get; private set; }
        public Vector3 MarkingPoint { get; private set; }

        private SlowmoTagUI slowmoTagUI;
        private Camera camera;
        private Vector3 positionOffset;

        public SlowmoTag(Transform taggedTransform, Vector3 markingPoint, SlowmoTagUI slowmoTagUI, Camera camera)
        {
            TaggedTransform = taggedTransform;
            MarkingPoint = markingPoint;
            this.slowmoTagUI = slowmoTagUI;
            this.camera = camera;
            positionOffset = markingPoint - TaggedTransform.position;

            this.slowmoTagUI.transform.position = this.camera.WorldToScreenPoint(markingPoint);
        }

        public void UpdateTagPosition()
        {
            if (TaggedTransform == null)
                return;
            slowmoTagUI.transform.position = camera.WorldToScreenPoint(TaggedTransform.position + positionOffset);
        }

        public void DestroyUITag()
        {
            slowmoTagUI.Remove();
        }

        public Vector3 GetSlowmoTagUIPosition()
        {
            return slowmoTagUI.transform.position;
        }
    }

    private FloatTween slowmoPostProcessTween = new FloatTween();
    private FloatTween timeScaleTween = new FloatTween();

    public enum SlowmoState
    {
        /// <summary>
        /// The slowmo ability is not active and isn't currently triggered.
        /// </summary>
        Inactive,

        /// <summary>
        /// The slowmo ability is actively slowing down time and allowing marking of damageables.
        /// </summary>
        Slowdown,

        /// <summary>
        /// The slowmo ability is iterating through the tags and letting the player destroy marked enemies.
        /// </summary>
        Executing
    };

    public string inputButtonName = "Slowmo Toggle";
    public SlowmoState CurrentSlowmoState { get; private set; } = SlowmoState.Inactive;

    [Header("Tagging")]
    public uint maxSlowmoTags;
    public SlowmoTagUI slowmoTagUI;
    public Transform slowmoTagParent;
    public float tagVisibleDuration = 1f;
    private float timeTillNextTagVanish;

    [Header("Transitions")]
    [Tooltip("Time scale when slowmo is active")] public float slowmoTimeScale = 0.25f;
    [Tooltip("How long to take when transiting to slowmo")] public float transitToSlowmoDuration = 0.5f;
    [Tooltip("How long to take to transit out of slowmo")] public float transitFromSlowmoDuration = 0.25f;

    [Space]
    public LayerMask markableLayers;
    public float markingDistance;

    [Header("Slowmo Duration")]
    public float maxSlowmoDuration = 10f;
    private float currentSlowmoDuration;
    public float gainSlowmo = 1f;
    public float slowmoDurationWarn = 5f;

    [Header("Component References")]
    public new Camera camera;

    [Header("Audio Sources")]
    public AudioSource slowmoAudioSource;
    public AudioSource clockTickingAudioSource;

    [Header("Audio Clips")]
    public AudioClip slowmoSound;
    public AudioClip speedUpSound;
    public AudioClip slowmoMarkSound;

    [Header("Audio Mixer Snapshots")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot onSlowmoSnapshot;

    [Header("Post-processing")]
    public PostProcessVolume slowmoPostProcess;

    private List<SlowmoTag> slowmoTags;

    public event System.Action SlowmoStartEvent;
    public event System.Action SlowmoEndEvent;
    public event System.Action<SlowmoState> SlowmoStateChangedEvent;

    // Awake is called when the script instance is loaded
    private void Awake()
    {
        slowmoTags = new List<SlowmoTag>((int) maxSlowmoTags);

        timeTillNextTagVanish = tagVisibleDuration;
        currentSlowmoDuration = maxSlowmoDuration;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateSlowmoUITags();

        if (CurrentSlowmoState == SlowmoState.Slowdown)
        {
            currentSlowmoDuration -= Time.unscaledDeltaTime;

            if (currentSlowmoDuration <= slowmoDurationWarn && !clockTickingAudioSource.isPlaying)
            {
                clockTickingAudioSource.Play();
            }
            else if (currentSlowmoDuration <= 0)
            {
                ToggleSlowmo();
            }
        }

        if (CurrentSlowmoState == SlowmoState.Executing)
        {
            if (clockTickingAudioSource.isPlaying)
            {
                clockTickingAudioSource.Stop();
            }

            if (slowmoTags.Count <= 0)
            {
                EndSlowmo();
            }
            else
            {
                timeTillNextTagVanish -= Time.unscaledDeltaTime;

                if (timeTillNextTagVanish <= 0)
                {
                    slowmoTags[0].DestroyUITag();
                    slowmoTags.RemoveAt(0);
                    timeTillNextTagVanish = tagVisibleDuration;
                }
            }
        }
    }

    private void EndSlowmo(bool playSoundEffect = true)
    {
        if (clockTickingAudioSource.isPlaying)
        {
            clockTickingAudioSource.Stop();
        }
        normalSnapshot.TransitionTo(1f * slowmoTimeScale);
        void doTimeScaleTween(ITween<float> t)
        {
            Time.timeScale = t.CurrentValue;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        // stop previous tweens
        if (slowmoPostProcessTween.State == TweenState.Running)
        {
            slowmoPostProcessTween.Stop(TweenStopBehavior.DoNotModify);
        }

        if (timeScaleTween.State == TweenState.Running)
        {
            timeScaleTween.Stop(TweenStopBehavior.DoNotModify);
        }

        slowmoPostProcessTween = slowmoPostProcess.gameObject.Tween("SlowmoFXTween", slowmoPostProcess.weight, 0f, transitFromSlowmoDuration, TweenScaleFunctions.Linear, (ITween<float> t) => slowmoPostProcess.weight = t.CurrentValue, null, false);
        timeScaleTween = gameObject.Tween("TimeScaleTween", Time.timeScale, 1f, transitFromSlowmoDuration, TweenScaleFunctions.Linear, doTimeScaleTween, null, false);

        // end slowmo and invoke events
        CurrentSlowmoState = SlowmoState.Inactive;
        SlowmoStateChangedEvent?.Invoke(CurrentSlowmoState);
        SlowmoEndEvent?.Invoke();

        if (playSoundEffect)
        {
            // play speed up sound effect
            slowmoAudioSource.PlayOneShot(speedUpSound, 1.0f);
        }
    }

    public void ToggleSlowmo()
    {
        switch (CurrentSlowmoState)
        {
            case SlowmoState.Inactive:
                if (currentSlowmoDuration > 0)
                {
                    // toggling slowmo while it is inactive will activate slowmo
                    CurrentSlowmoState = SlowmoState.Slowdown;
                    StartSlowmoTransition();
                    onSlowmoSnapshot.TransitionTo(1f * slowmoTimeScale);

                    // raise event for starting slowmo and for changing slowmo state
                    // we use ? for null checking, since if there are no handlers registered, a NullReferenceException is thrown
                    SlowmoStartEvent?.Invoke();
                    SlowmoStateChangedEvent?.Invoke(CurrentSlowmoState);
                }
                break;

            case SlowmoState.Slowdown:

                if (slowmoTags.Count <= 0)
                {
                    EndSlowmo();
                }
                else
                {
                    CurrentSlowmoState = SlowmoState.Executing;
                    SlowmoStateChangedEvent?.Invoke(CurrentSlowmoState);
                }

                break;
        }
    }

    private void StartSlowmoTransition()
    {
        void doTimeScaleTween(ITween<float> t)
        {
            Time.timeScale = t.CurrentValue;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        // stop previous tweens
        if (slowmoPostProcessTween.State == TweenState.Running)
        {
            slowmoPostProcessTween.Stop(TweenStopBehavior.DoNotModify);
        }

        if (timeScaleTween.State == TweenState.Running)
        {
            timeScaleTween.Stop(TweenStopBehavior.DoNotModify);
        }

        // these tweens use unscaled time to progress and hence, are unaffected by time scale
        // the 6th paramater of the Tween method contains an inline function and is called when the tween progresses
        // it is inlined so that it is local only in the method that calls it
        slowmoPostProcessTween = slowmoPostProcess.gameObject.Tween("SlowmoFXTween", slowmoPostProcess.weight, 1f, transitToSlowmoDuration, TweenScaleFunctions.Linear, (ITween<float> t) => slowmoPostProcess.weight = t.CurrentValue, null, false);
        timeScaleTween = gameObject.Tween("TimeScaleTween", Time.timeScale, slowmoTimeScale, transitToSlowmoDuration, TweenScaleFunctions.Linear, doTimeScaleTween, null, false);

        // play slowdown sound effect
        slowmoAudioSource.PlayOneShot(slowmoSound, 1.0f);
    }

    public void MarkTarget(Transform user)
    {
        if (Physics.Raycast(user.position, user.forward, out RaycastHit hitInfo, markingDistance))
        {
            if (markableLayers == (markableLayers | (1 << hitInfo.collider.gameObject.layer)))
            {
                SlowmoTag slowmoTag = new SlowmoTag(
                    hitInfo.transform,
                    hitInfo.point,
                    Instantiate(slowmoTagUI, Vector3.zero, Quaternion.identity, slowmoTagParent),
                    camera
                );

                slowmoTags.Add(slowmoTag);

                slowmoAudioSource.PlayOneShot(slowmoMarkSound, 1.0f);
            }

        }

        if (slowmoTags.Count >= maxSlowmoTags)
        {
            CurrentSlowmoState = SlowmoState.Executing;
        }
    }

    private void UpdateSlowmoUITags()
    {
        for (int i = 0; i < slowmoTags.Count; i++)
        {
            if (slowmoTags[i].TaggedTransform == null)
            {
                slowmoTags[i].DestroyUITag();
                slowmoTags.RemoveAt(i);
                i--; // go backwards by one since elements are re-numbered upon removal of an element
                continue;
            }

            slowmoTags[i].UpdateTagPosition();
        }
    }

    public Vector3 GetCurrentTagPosition()
    {
        if (slowmoTags.Count <= 0)
        {
            return Vector3.zero;
        }
        else
        {
            return camera.ScreenToWorldPoint(slowmoTags[0].GetSlowmoTagUIPosition());
        }
    }

    public void GainSlowmo()
    {
        currentSlowmoDuration += gainSlowmo;

        if (currentSlowmoDuration > maxSlowmoDuration)
        {
            currentSlowmoDuration = maxSlowmoDuration;
        }
    }

    public void ForceTerminate()
    {
        for (int i = 0; i < slowmoTags.Count; i++)
        {
            slowmoTags[i].DestroyUITag();
        }

        EndSlowmo(false);
    }
}
