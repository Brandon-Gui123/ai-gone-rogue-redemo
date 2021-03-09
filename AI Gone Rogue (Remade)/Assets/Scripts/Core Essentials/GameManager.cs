using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;    // for audio snapshot transitioning

using TMPro;                // for accessing TextMeshPro components
using DigitalRuby.Tween;    // for animating text

public class GameManager : MonoBehaviour
{
    public static GameManager currentInstance;

    private bool isRunning = false;
    [SerializeField] private uint keys = 0;
    [SerializeField] private int score = 0;
    private uint lifetimeKeysCollected = 0;
    private uint enemiesKilled = 0;
    private float timeTaken = 0;

    [Header("Text UI Component References")]
    public TextMeshProUGUI keyCountNumber;
    public TextMeshProUGUI scoreCountNumber;
    public TextMeshProUGUI timeTakenNumber;

    [Header("Background Music")]
    public float audioFadeTime = 3f;

    [Header("Audio Mixer Snapshots")]
    public AudioMixerSnapshot originalAudioMixerSnapshot;
    public AudioMixerSnapshot gameEndAudioMixerSnapshot;

    [Header("Game Over Stuff")]
    public GameObject gameOverCanvas;
    public AudioClip gameOverAudioClip;
    [Space]
    public CanvasGroup gameOverCanvasGroup;
    public CanvasGroup gameOverButtonsCanvasGroup;
    public float transparencyTweenDuration = 3f;
    [Space]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverKeysCollectedText;
    public TextMeshProUGUI gameOverEnemiesKilledText;
    public TextMeshProUGUI gameOverTimeTakenText;

    [Header("Game Completed Stuff")]
    public GameObject gameWinCanvas;
    public AudioClip gameSuccessAudioClip;
    [Space]
    public CanvasGroup gameWinCanvasGroup;
    public CanvasGroup gameWinButtonsCanvasGroup;
    public TextMeshProUGUI gameWinScoreText;
    public TextMeshProUGUI gameWinKeysCollectedText;
    public TextMeshProUGUI gameWinEnemiesKilledText;
    public TextMeshProUGUI gameWinTimeTakenText;

    [Header("External Component References")]
    public AudioSource bgmAudioSource;

    private Color keyCountNumber_OriginalColor;

    public uint Keys { get => keys; set { keys = value; keyCountNumber.text = keys.ToString(); } }
    public int Score { get => score; set { score = value; scoreCountNumber.text = value.ToString(); } }

    private void Awake()
    {
        SingletonCheck();
    }

    private void Start()
    {
        // ensure that we are on the normal audio snapshot at the start
        // so that audio plays back at the correct volume and pitch
        originalAudioMixerSnapshot.TransitionTo(0f);

        // updates all UI elements assigned to this script using the values above
        keyCountNumber.text = keys.ToString();
        scoreCountNumber.text = score.ToString();

        // remember the original key count number color so that when we do flashing
        // the text flashes back to its original color
        keyCountNumber_OriginalColor = keyCountNumber.color;

        // setup canvas groups to make them uninteractable during gameplay
        gameOverCanvas.SetActive(false);
        gameOverCanvasGroup.alpha = 0f;
        gameOverButtonsCanvasGroup.alpha = 0f;
        gameOverButtonsCanvasGroup.interactable = false;

        gameWinCanvas.SetActive(false);
        gameWinCanvasGroup.alpha = 0f;
        gameWinButtonsCanvasGroup.alpha = 0f;
        gameWinButtonsCanvasGroup.interactable = false;

        // transition to the original snapshot immediately
        originalAudioMixerSnapshot.TransitionTo(0f);
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        if (isRunning)
        {
            timeTaken += Time.deltaTime;
            if (!timeTakenNumber.text.Equals(Mathf.FloorToInt(timeTaken).ToString()))
            {
                timeTakenNumber.text = Mathf.FloorToInt(timeTaken).ToString();
            }
        }
    }

    private void SingletonCheck()
    {
        if (currentInstance == null)
        {
            currentInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void FlashKeyCount(uint flashTimes, float durationPerFlash, Color colorToFlash)
    {
        FloatTween flashKeyCountTween;

        void FlashText(ITween<float> t)
        {
            int keyframe = ((int) t.CurrentValue) % 2;

            if (keyframe == 0)
            {
                keyCountNumber.color = colorToFlash;
            }
            else if (keyframe == 1)
            {
                keyCountNumber.color = keyCountNumber_OriginalColor;
            }
        }

        flashKeyCountTween = gameObject.Tween("FlashKeyCount", 0f, flashTimes * 2, durationPerFlash * flashTimes * 2, TweenScaleFunctions.Linear, FlashText, (ITween<float> t) => keyCountNumber.color = keyCountNumber_OriginalColor, false);
    }

    public void GameOver()
    {
        isRunning = false;
        gameOverCanvas.SetActive(true);
        gameEndAudioMixerSnapshot.TransitionTo(audioFadeTime);

        // fade out the previous bgm, swap audio clips, then fade in and play the new bgm
        gameObject.Tween(null, bgmAudioSource.volume, 0f, audioFadeTime, TweenScaleFunctions.Linear,
            (ITween<float> t) => bgmAudioSource.volume = t.CurrentValue,
            (ITween<float> t) => { bgmAudioSource.clip = gameOverAudioClip; bgmAudioSource.Play(); },
            false
        ).ContinueWith(new FloatTween().Setup(0f, 1f, audioFadeTime, TweenScaleFunctions.Linear,
            (ITween<float> t) => bgmAudioSource.volume = t.CurrentValue
        ));

        // fade in the canvas group for the game over screen
        gameObject.Tween(null, 0f, 1f, transparencyTweenDuration, TweenScaleFunctions.Linear, (ITween<float> t) => gameOverCanvasGroup.alpha = t.CurrentValue, null, false)
                  .ContinueWith(new FloatTween().Setup(0f, 1f, transparencyTweenDuration, TweenScaleFunctions.Linear, 
                    (ITween<float> t) => gameOverButtonsCanvasGroup.alpha = t.CurrentValue, 
                    (ITween<float> t) => gameOverButtonsCanvasGroup.interactable = true, 
                    false
                )
        );

        // set the text of all statistics in the game over screen
        gameOverScoreText.text = score.ToString();
        gameOverKeysCollectedText.text = lifetimeKeysCollected.ToString();
        gameOverEnemiesKilledText.text = enemiesKilled.ToString();
        gameOverTimeTakenText.text = Mathf.Floor(timeTaken).ToString();
    }

    public void GameSuccess()
    {
        isRunning = false;
        gameWinCanvas.SetActive(true);
        gameEndAudioMixerSnapshot.TransitionTo(audioFadeTime);

        // fade out the previous bgm, swap audio clips, then fade in and play the new bgm
        gameObject.Tween(null, bgmAudioSource.volume, 0f, audioFadeTime, TweenScaleFunctions.Linear,
            (ITween<float> t) => bgmAudioSource.volume = t.CurrentValue,
            (ITween<float> t) => { bgmAudioSource.clip = gameSuccessAudioClip; bgmAudioSource.Play(); },
            false
        ).ContinueWith(new FloatTween().Setup(0f, 1f, audioFadeTime, TweenScaleFunctions.Linear,
            (ITween<float> t) => bgmAudioSource.volume = t.CurrentValue
        ));

        // fade in the canvas group for the game win screen
        gameObject.Tween(null, 0f, 1f, transparencyTweenDuration, TweenScaleFunctions.Linear, (ITween<float> t) => gameWinCanvasGroup.alpha = t.CurrentValue, null, false)
                  .ContinueWith(new FloatTween().Setup(0f, 1f, transparencyTweenDuration, TweenScaleFunctions.Linear,
                    (ITween<float> t) => gameWinButtonsCanvasGroup.alpha = t.CurrentValue,
                    (ITween<float> t) => gameWinButtonsCanvasGroup.interactable = true,
                    false
                )
        );

        // set the text of all statistics in the game win screen
        gameWinScoreText.text = score.ToString();
        gameWinKeysCollectedText.text = lifetimeKeysCollected.ToString();
        gameWinEnemiesKilledText.text = enemiesKilled.ToString();
        gameWinTimeTakenText.text = Mathf.Floor(timeTaken).ToString();
    }

    public void IncrementKeys()
    {
        Keys++;
        lifetimeKeysCollected++;
    }

    public void IncrementEnemyKillCount()
    {
        enemiesKilled++;
    }

    public void StartGameTime()
    {
        isRunning = true;
        timeTaken = 0;
    }
}
