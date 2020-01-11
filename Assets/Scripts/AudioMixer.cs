using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMixer : MonoBehaviour
{
    public delegate void SoundScheduled(AudioClip audioClip, double intervalUntilScheduledTime);
    public event SoundScheduled OnSoundScheduledEvent;

    public delegate void QueueFinished();
    public event QueueFinished OnQueueFinishedEvent;

    private const float FLIP_INTERVAL = 1f;

    [SerializeField] private float delayUntilFirstSound = 1.848f;
    [SerializeField] private float bpm = 130.0f;
    [SerializeField] private int numBeatsPerSegment = 4;
    [SerializeField] private TextAsset soundSequence;
    [SerializeField] private SoundLibrary soundLibrary;

    private bool hasStarted;
    private double nextEventTime;
    private int flip = 0;
    private AudioSource[] audioSources;
    private bool running = false;
    private List<float[]> samples;

    private int previousFrameSampleIndex;
    private float previousSampleIntensity;
    private AudioSource playingAudioSource;

    private int[] clipIDs;
    private Queue<AudioClip> clipQueue;
    private Dictionary<int, AudioClip> idToAudioClipDictionary;
    
    public void Start()
    {
        if (hasStarted)
        {
            return;
        }

        clipIDs = SoundSequenceParserHelper.ParseTextFile(soundSequence);
        audioSources = new AudioSource[2];
        samples = new List<float[]>();
        samples.Add(new float[0]);
        samples.Add(new float[0]);

        idToAudioClipDictionary = new Dictionary<int, AudioClip>();
        
        for (int i = 0; i < 2; i++)
        {
            GameObject child = new GameObject($"Player {i}" );
            child.transform.parent = gameObject.transform;
            audioSources[i] = child.AddComponent<AudioSource>();
        }

        hasStarted = true;
    }

    public void Update()
    {
        if (!running || clipQueue.Count == 0)
        {
            return;
        }

        double time = AudioSettings.dspTime;

        if (time + FLIP_INTERVAL > nextEventTime)
        {
            PlayScheduledAndFlipClips();
        }

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.isPlaying)
            {
                previousFrameSampleIndex = audioSource.timeSamples;
            }
        }
    }

    public void Initialize()
    {
        clipQueue = new Queue<AudioClip>();
        foreach (int clipID in clipIDs)
        {
            QueueClip(clipID);
        }

        nextEventTime = AudioSettings.dspTime + delayUntilFirstSound;
        running = true;
        flip = 0;
        previousFrameSampleIndex = 0;
        previousSampleIntensity = 0f;
    }

    public void Initialize(int[] newClipIDs)
    {
        if (!hasStarted)
        {
            Start();
        }

        clipIDs = newClipIDs;
        Initialize();
    }

    public float GetCurrentClipIntensity()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == playingAudioSource)
            {
                return GetIntensityFromSource(audioSource, Array.IndexOf(audioSources, audioSource));
            }
        }
        return 0f;
    }

    private void PlayScheduledAndFlipClips()
    {
        audioSources[flip].clip = clipQueue.Dequeue();
        samples[flip] = new float[audioSources[flip].clip.samples];
        audioSources[flip].clip.GetData(samples[flip], 0);
        audioSources[flip].PlayScheduled(nextEventTime);
        
        double timeUntilNextEvent = nextEventTime - AudioSettings.dspTime;
        StartCoroutine(FlipPlayingAudioSourceAfterDelay(timeUntilNextEvent, audioSources[flip]));
        OnSoundScheduledEvent?.Invoke(audioSources[flip].clip, timeUntilNextEvent);

        if (clipQueue.Count == 0)
        {
            OnQueueFinishedEvent?.Invoke();
        }

        nextEventTime += 60.0f / bpm * numBeatsPerSegment;

        flip = 1 - flip;
    }

    private void QueueClip(int audioClipID)
    {
        if (!idToAudioClipDictionary.ContainsKey(audioClipID))
        {
            idToAudioClipDictionary.Add(audioClipID, soundLibrary.GetClipFromID(audioClipID));
        }
        clipQueue.Enqueue(idToAudioClipDictionary[audioClipID]);
    }

    private float GetIntensityFromSource(AudioSource audioSource, int indexOfPlayingAudioSource)
    {
        int currentFrameSampleIndex = audioSource.timeSamples;
        float sum = 0f;
        for (int i = previousFrameSampleIndex; i < currentFrameSampleIndex; i++)
        {
            sum += samples[indexOfPlayingAudioSource][i];
        }

        // This solves a weird bug.
        if (currentFrameSampleIndex == previousFrameSampleIndex)
        {
            return previousSampleIntensity;
        }
        previousSampleIntensity = sum / (currentFrameSampleIndex - previousFrameSampleIndex);
        return previousSampleIntensity;
    }

    private IEnumerator FlipPlayingAudioSourceAfterDelay(double delay, AudioSource audioSource)
    {
        yield return new WaitForSecondsRealtime((float)delay);
        playingAudioSource = audioSource;
    }
}
