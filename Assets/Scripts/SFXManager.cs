using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    public void PlaySFX(AudioClip clip, Transform source)
    {
        if (clip == null || source == null)
        {
            Debug.LogWarning("SFX clip or source Transform is null!");
            return;
        }

        AudioSource audioSource = source.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning($"No AudioSource component found on {source.name}!");
            return;
        }

        audioSource.pitch = Random.Range(0.7f, 1f);
        audioSource.PlayOneShot(clip);
    }
}
