﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSequenceHelper : MonoBehaviour
{
    public delegate void TutorialCompleted();
    public event TutorialCompleted OnTutorialCompletedEvent;

    [SerializeField] [Range(0.01f, 0.2f)] private float textFadeSpeed = 0.15f;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameObject audioVisualizer;
    [SerializeField] private InputSystem inputSystem;
    [SerializeField] private UpdateCallback updateCallback;
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private TextMeshProUGUI tutorialCompletedText;
    [SerializeField] private TextMeshProUGUI[] introTextsFirstTutorial;
    [SerializeField] private TextMeshProUGUI[] introTextsSecondTutorial;
    [SerializeField] private TextAsset[] tutorialSoundSequencesPitch;
    [SerializeField] private TextAsset[] tutorialSoundSequencesRhythm;
    [SerializeField] private RawImage[] arrowKeys;

    private InputWindow activeInputWindow;
    private bool isPitchTutorial;
    private TextAsset[] activeSoundSequences;
    private TextMeshProUGUI[] activeIntroTexts;
    private int soundSequenceIndex = 0;

    private int introTextIndex = 0;

    public void Start()
    {
        audioMixer.OnSoundScheduledEvent += OnSoundScheduled;
        if (GameDataHelper.Instance.isFirstSession)
        {
            activeIntroTexts = introTextsFirstTutorial;
        }
        else
        {
            activeIntroTexts = introTextsSecondTutorial;
        }
        StartCoroutine(ShowIntroText());
    }

    public void OnDestroy()
    {
        audioMixer.OnSoundScheduledEvent -= OnSoundScheduled;
    }
    
    public void SetTutorial(bool isPitchTutorial)
    {
        this.isPitchTutorial = isPitchTutorial;
        activeSoundSequences = GetCurrentSoundSequences();
    }

    private void SetAudioActive(bool value)
    {
        audioMixer.gameObject.SetActive(value);
        audioVisualizer.SetActive(value);
    }
    
    private void OnSoundScheduled(AudioClip audioClip, double intervalUntilScheduledTime)
    {
        StartCoroutine(CreateNextInputWindow(audioClip, intervalUntilScheduledTime));
    }

    private void OnWindowShut(bool result)
    {
        activeInputWindow.OnWindowShutEvent -= OnWindowShut;
        
        if (result)
        {
            CorrectResult();
        }
        else
        {
            IncorrectResult();
        }
    }

    private void CorrectResult()
    {
        if (soundSequenceIndex < activeSoundSequences.Length)
        {
            HighlightArrowKey(soundSequenceIndex);
            audioMixer.Initialize(SoundSequenceParserHelper.ParseTextFile(activeSoundSequences[soundSequenceIndex]));
            soundSequenceIndex++;
        }
        else
        {
            Debug.Log("Tutorial Completed");
            StartCoroutine(CompleteTutorialAfterSeconds(1f));
        }
    }

    private void IncorrectResult()
    {
        audioMixer.Initialize();
    }

    private TextAsset[] GetCurrentSoundSequences()
    {
        if (isPitchTutorial)
        {
            return tutorialSoundSequencesPitch;
        }
        return tutorialSoundSequencesRhythm;
    }

    private void HighlightArrowKey(int index)
    {
        for (int i = 0; i < arrowKeys.Length; i++)
        {
            if (i == index)
            {
                arrowKeys[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                arrowKeys[i].color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    private IEnumerator CreateNextInputWindow(AudioClip audioClip, double delayUntilClipStarts)
    {
        yield return new WaitForSecondsRealtime((float)delayUntilClipStarts);
        
        int clipID = soundLibrary.GetIDFromClip(audioClip);
        KeyCode keyCode = AudioKeyCodeConstants.GetKeyCodeFromID(clipID);
        
        if (activeInputWindow != null)
        {
            activeInputWindow.OnWindowShutEvent -= OnWindowShut;
            activeInputWindow.Deactivate();
        }

        if (clipID >= soundLibrary.IndexOfFirstCue)
        {
            Debug.Log("Creating Input Window");
            activeInputWindow = new InputWindow(audioClip, keyCode, AudioSettings.dspTime + audioClip.length, inputSystem, updateCallback);
            activeInputWindow.OnWindowShutEvent += OnWindowShut;
        }
        else
        {
            activeInputWindow = null;
        }
    }

    private IEnumerator ShowIntroText()
    {
        Color fadeColor = activeIntroTexts[introTextIndex].color;
        while (activeIntroTexts[introTextIndex].color.a < 1f)
        {
            fadeColor.a += textFadeSpeed;
            activeIntroTexts[introTextIndex].color = fadeColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(6.5f);

        while (activeIntroTexts[introTextIndex].color.a > 0f)
        {
            fadeColor.a -= textFadeSpeed;
            activeIntroTexts[introTextIndex].color = fadeColor;
            yield return null;
        }

        if (introTextIndex + 1 < activeIntroTexts.Length)
        {
            introTextIndex++;
            StartCoroutine(ShowIntroText());
        }
        else
        {
            SetAudioActive(true);
            audioMixer.Initialize(SoundSequenceParserHelper.ParseTextFile(activeSoundSequences[soundSequenceIndex]));
            StartCoroutine(ShowArrowKeys());
            soundSequenceIndex++;
        }
    }

    private IEnumerator CompleteTutorialAfterSeconds(float delay)
    {
        yield return new WaitForSecondsRealtime(1.848f);

        StartCoroutine(HideArrowKeys());

        Color fadeColor = tutorialCompletedText.color;
        while (tutorialCompletedText.color.a < 1f)
        {
            fadeColor.a += textFadeSpeed;
            tutorialCompletedText.color = fadeColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(6.5f);

        while (tutorialCompletedText.color.a > 0f)
        {
            fadeColor.a -= textFadeSpeed;
            tutorialCompletedText.color = fadeColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delay);

        OnTutorialCompletedEvent?.Invoke();
    }
    
    private IEnumerator ShowArrowKeys()
    {
        Color arrowAlpha = new Color(1f, 1f, 1f, 0f);
        while (arrowKeys[0].color.a < 0.3f)
        {
            foreach(RawImage arrowKey in arrowKeys)
            {
                arrowAlpha.a += textFadeSpeed / 5f;
                arrowKey.color = arrowAlpha;
            }
            yield return null;
        }
        HighlightArrowKey(0);
    }

    private IEnumerator HideArrowKeys()
    {
        Color arrowAlpha = new Color(1f, 1f, 1f, 0.3f);
        while (arrowKeys[0].color.a > 0f)
        {
            foreach (RawImage arrowKey in arrowKeys)
            {
                arrowAlpha.a -= textFadeSpeed / 5f;
                arrowKey.color = arrowAlpha;
            }
            yield return null;
        }
    }
}
