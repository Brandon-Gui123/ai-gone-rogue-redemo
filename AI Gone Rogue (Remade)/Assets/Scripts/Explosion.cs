using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class Explosion : MonoBehaviour
{
    public Color particleColor;
    private new ParticleSystem particleSystem;
    private AudioSource audioSource;

    [Header("Audio")]
    [Range(-3f, 3f)] public float minPitch = 0.8f;
    [Range(-3f, 3f)] public float maxPitch = 1.2f;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        SetParticleStartColor(particleColor);
        Destroy(gameObject, Mathf.Max(particleSystem.main.duration, audioSource.clip.length));
        audioSource.pitch = Random.Range(minPitch, maxPitch);
    }

    private void SetParticleStartColor(Color color)
    {
        ParticleSystem.MainModule particleSystemMainModule = GetComponent<ParticleSystem>().main;
        particleSystemMainModule.startColor = color;
    }

    // This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
    private void OnValidate()
    {
        SetParticleStartColor(particleColor);
    }


}
