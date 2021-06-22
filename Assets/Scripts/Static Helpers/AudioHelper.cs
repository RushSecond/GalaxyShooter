using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static AudioSource PlayClipAtCamera(AudioClip clip, float duration, float volume, float pitch)
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = Camera.main.transform.position; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.clip = clip; // define the clip
        aSource.volume = volume;
        aSource.pitch = pitch;
        aSource.Play(); // start the sound
        GameObject.Destroy(tempGO, duration); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}
