using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static void PlaySound(AudioClip clip, Vector3 position, float minDistance = 1f, float maxDistance = 50f)
    {
        if (clip == null) return;

        GameObject soundObject = new GameObject("TempSound");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1.0f; // 3DÉTÉEÉìÉhÇ∆ÇµÇƒçƒê∂
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.transform.position = position;
        audioSource.Play();

        Destroy(soundObject, clip.length);
    }
}
