using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "ScriptableObjects/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [SerializeField] private int indexOfFirstCue = 2;
    [SerializeField] private AudioClip[] audioClips;

    public int IndexOfFirstCue => indexOfFirstCue;

    public AudioClip GetClipFromID(int id)
    {
        return audioClips[id];
    }

    public int GetIDFromClip(AudioClip audioClip)
    {
        return Array.IndexOf(audioClips, audioClip);
    }
}
