using System.Collections;
using TMPro;
using UnityEngine;

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
    [SerializeField] private TextMeshProUGUI[] introTextsFirstTutorial;
    [SerializeField] private TextMeshProUGUI[] introTextsSecondTutorial;
    [SerializeField] private TextAsset[] tutorialSoundSequencesPitch;
    [SerializeField] private TextAsset[] tutorialSoundSequencesRhythm;

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
            audioMixer.Initialize(SoundSequenceParserHelper.ParseTextFile(activeSoundSequences[soundSequenceIndex]));
            soundSequenceIndex++;
        }
        else
        {
            Debug.Log("Tutorial Completed");
            StartCoroutine(CompleteTutorialAfterSeconds(4f));
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
            soundSequenceIndex++;
        }
    }

    private IEnumerator CompleteTutorialAfterSeconds(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        OnTutorialCompletedEvent?.Invoke();
    }
}
