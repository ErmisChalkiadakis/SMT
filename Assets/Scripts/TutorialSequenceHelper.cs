using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSequenceHelper : MonoBehaviour
{
    public delegate void TutorialCompleted();
    public event TutorialCompleted OnTutorialCompletedEvent;

    [SerializeField] [Range(0.01f, 0.2f)] private float textFadeSpeed = 0.15f;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private BeatVisualizer beatVisualizer;
    [SerializeField] private InputSystem inputSystem;
    [SerializeField] private UpdateCallback updateCallback;
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private TextMeshProUGUI tutorialCompletedText;
    [SerializeField] private TextMeshProUGUI[] introTextsFirstTutorial;
    [SerializeField] private TextMeshProUGUI[] introTextsSecondTutorial;
    [SerializeField] private TextAsset[] tutorialSoundSequencesPitch;
    [SerializeField] private TextAsset[] tutorialSoundSequencesRhythm;
    [SerializeField] private RawImage[] arrowKeys;
    [SerializeField] private TextMeshProUGUI[] pressIndicators;
    [SerializeField] private TextMeshProUGUI tooLate;

    private InputWindow activeInputWindow;
    private bool isPitchTutorial;
    private TextAsset[] activeSoundSequences;
    private TextMeshProUGUI[] activeIntroTexts;
    private int soundSequenceIndex = 0;
    private bool correctInputOnce = false;

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
        beatVisualizer.gameObject.SetActive(value);
    }
    
    private void OnSoundScheduled(AudioClip audioClip, double intervalUntilScheduledTime)
    {
        StartCoroutine(CreateNextInputWindow(audioClip, intervalUntilScheduledTime));
    }

    private void OnButtonPressed(bool result, double reactionTime)
    {
        activeInputWindow.OnButtonPressedEvent -= OnButtonPressed;

        if (!result)
        {
            StartCoroutine(TooLate());
        }
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
        beatVisualizer.CorrectInput();

        if (!correctInputOnce)
        {
            correctInputOnce = !correctInputOnce;
            audioMixer.Initialize();
            return;
        }

        if (soundSequenceIndex < activeSoundSequences.Length)
        {
            correctInputOnce = !correctInputOnce;
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
        beatVisualizer.IncorrectInput();

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
            activeInputWindow.OnButtonPressedEvent -= OnButtonPressed;
            activeInputWindow.Deactivate();
        }

        if (clipID >= soundLibrary.IndexOfFirstCue)
        {
            Debug.Log("Creating Input Window");

            beatVisualizer.WindowOpened();

            activeInputWindow = new InputWindow(audioClip, keyCode, AudioSettings.dspTime + audioClip.length, inputSystem, updateCallback);
            activeInputWindow.OnWindowShutEvent += OnWindowShut;
            activeInputWindow.OnButtonPressedEvent += OnButtonPressed;
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


        beatVisualizer.gameObject.SetActive(false);
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
            foreach (TextMeshProUGUI indicator in pressIndicators)
            {
                indicator.color = arrowAlpha * 3f;
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
            foreach (TextMeshProUGUI indicator in pressIndicators)
            {
                indicator.color = arrowAlpha / 4f;
            }
            yield return null;
        }
    }

    private IEnumerator TooLate()
    {
        Color textColor = new Color(1f, 1f, 1f, 0f);
        while (tooLate.color.a < 1f)
        {
            textColor.a += 0.15f;
            tooLate.color = textColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        textColor = new Color(1f, 1f, 1f, 1f);
        while (tooLate.color.a > 0f)
        {
            textColor.a -= 0.05f;
            tooLate.color = textColor;
            yield return null;
        }
    }
}
