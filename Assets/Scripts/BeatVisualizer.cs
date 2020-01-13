using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatVisualizer : MonoBehaviour
{
    private const double BEAT_INTERVAL = 1.848f;

    [SerializeField] private float scaleExpansion = 0.01f;
    [SerializeField] private RawImage innerCircle;
    [SerializeField] private RawImage outerCircle;
    [SerializeField] private AudioSource correctInputAudioSource;
    [SerializeField] private AudioSource incorrectInputAudioSource;
    [SerializeField] private AudioMixer audioMixer;

    private Vector3 innerCircleCachedLocalScale;
    private Vector3 outerCircleCachedLocalScale;
    private double timeOfNextSwitch;
    private double switchInterval = BEAT_INTERVAL / 8f;
    private bool isExpanding = true;

    public void Start()
    {
        audioMixer.OnSoundScheduledEvent += OnSoundScheduled;

        innerCircleCachedLocalScale = innerCircle.transform.localScale;
        outerCircleCachedLocalScale = outerCircle.transform.localScale;
    }
    
    public void Update()
    {
        if (timeOfNextSwitch != 0)
        {
            //UpdateCircleScale();
        }
    }
    
    public void CorrectInput()
    {
        correctInputAudioSource.Play();

        StartCoroutine(FlashGreen());
    }

    public void IncorrectInput()
    {
        incorrectInputAudioSource.Play();

        StartCoroutine(FlashRed());
    }

    private void OnSoundScheduled(AudioClip audioClip, double timeUntilNextEvent)
    {
        audioMixer.OnSoundScheduledEvent -= OnSoundScheduled;

        timeOfNextSwitch = AudioSettings.dspTime + timeUntilNextEvent;
    }

    private void UpdateCircleScale()
    {
        if (timeOfNextSwitch < AudioSettings.dspTime)
        {
            isExpanding = !isExpanding;
            timeOfNextSwitch += switchInterval;

            innerCircleCachedLocalScale = innerCircle.transform.localScale;
            outerCircleCachedLocalScale = outerCircle.transform.localScale;
        }

        double timeSincePreviousSwitch = AudioSettings.dspTime + switchInterval - timeOfNextSwitch;
        double timeRatio = timeSincePreviousSwitch / switchInterval;

        if (isExpanding)
        {
            innerCircle.transform.localScale = innerCircleCachedLocalScale + Vector3.one * 2 / 3 * (scaleExpansion * (float)timeRatio);
            outerCircle.transform.localScale = outerCircleCachedLocalScale + Vector3.one * (scaleExpansion * (float)timeRatio);
        }
        else
        {
            innerCircle.transform.localScale = innerCircleCachedLocalScale - Vector3.one * 2 / 3 * (scaleExpansion * (float)timeRatio);
            outerCircle.transform.localScale = outerCircleCachedLocalScale - Vector3.one * (scaleExpansion * (float)timeRatio);
        }
    }

    private IEnumerator FlashGreen()
    {
        Color flashColor = innerCircle.color;
        while (innerCircle.color.r > 0.1f)
        {
            flashColor.r -= 0.15f;
            flashColor.b -= 0.15f;
            innerCircle.color = flashColor;
            outerCircle.color = flashColor * Vector4.one * 0.9f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        while (innerCircle.color.r < 1f)
        {
            flashColor.r += 0.05f;
            flashColor.b += 0.05f;
            innerCircle.color = flashColor;
            outerCircle.color = flashColor * Vector4.one * 0.9f;
            yield return null;
        }
    }

    private IEnumerator FlashRed()
    {
        Color flashColor = innerCircle.color;
        while (innerCircle.color.g > 0.1f)
        {
            flashColor.g -= 0.15f;
            flashColor.b -= 0.15f;
            innerCircle.color = flashColor;
            outerCircle.color = flashColor * Vector4.one * 0.9f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        while (innerCircle.color.g < 1f)
        {
            flashColor.g += 0.05f;
            flashColor.b += 0.05f;
            innerCircle.color = flashColor;
            outerCircle.color = flashColor * Vector4.one * 0.9f;
            yield return null;
        }
    }
}
