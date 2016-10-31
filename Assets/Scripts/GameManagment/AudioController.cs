using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

    public void PlaySound(AudioSource audioS, AudioClip clip, bool randomizePitch = false, float randomPitchMin = 1f, float randomPitchMax = 1f)
    {
        audioS.clip = clip;

        if (randomizePitch == true)
        {
            audioS.pitch = Random.Range(randomPitchMin, randomPitchMax);
        }

        audioS.Play();
    }

    public void InstantiateClip(Vector3 pos, AudioClip clip, float time = 2f, bool randomizePitch = false, float randomPitchMin = 1f, float randomPitchMax = 1f)
    {
        GameObject clone = new GameObject("One Shot audio");

        clone.transform.position = pos;

        AudioSource audio = clone.AddComponent<AudioSource>();
        audio.spatialBlend = 1;
        audio.clip = clip;
        audio.Play();
        Destroy(clone, time);
    }

}
