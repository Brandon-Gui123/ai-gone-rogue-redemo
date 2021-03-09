using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class TestAudio : MonoBehaviour, IPointerUpHandler
{
    public AudioClip testClip;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioSource.PlayOneShot(testClip, 1.0f);
    }
}
