using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSequenceHelper : MonoBehaviour
{
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private BeatVisualizer beatVisualizer;
    [SerializeField] private InputSystem inputSystem;
    [SerializeField] private UpdateCallback updateCallback;
    [SerializeField] private TextMeshProUGUI sessionFinishedText;
    [SerializeField] private TextMeshProUGUI studyFinishedText;
    [SerializeField] private RawImage fader;

    private List<int> correctnessPerformance = new List<int>();

    private InputWindow activeInputWindow;

    public void Start()
    {
        audioMixer.OnSoundScheduledEvent += OnSoundScheduled;
        audioMixer.OnQueueFinishedEvent += OnQueueFinished;
    }

    public void OnDestroy()
    {
        audioMixer.OnSoundScheduledEvent -= OnSoundScheduled;
        audioMixer.OnQueueFinishedEvent -= OnQueueFinished;
    }

    private void OnSoundScheduled(AudioClip audioClip, double intervalUntilScheduledTime)
    {
        StartCoroutine(CreateNextInputWindow(audioClip, intervalUntilScheduledTime));
    }

    private void OnQueueFinished()
    {
        GameDataHelper.Instance.SessionFinished();

        StartCoroutine(CompletePlaySession());
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

        correctnessPerformance.Add(1);
    }

    private void IncorrectResult()
    {
        beatVisualizer.IncorrectInput();

        correctnessPerformance.Add(0);
    }

    private void SubmitPlayData()
    {
        GameDataHelper.Instance.SubmitPlayData(correctnessPerformance.ToArray());
    }
    
    private IEnumerator CompletePlaySession()
    {
        SubmitPlayData();
        yield return new WaitForSecondsRealtime(4f);

        beatVisualizer.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(0.5f);

        TextMeshProUGUI textToShow;
        if (GameDataHelper.Instance.isStudyComplete)
        {
            textToShow = studyFinishedText;
        }
        else
        {
            textToShow = sessionFinishedText;
        }

        Color textColor = new Color(1f, 1f, 1f, 0f);
        while (textToShow.color.a < 1f)
        {
            textColor.a += 0.05f;
            textToShow.color = textColor;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(4f);

        if (!GameDataHelper.Instance.isStudyComplete)
        {
            fader.gameObject.SetActive(true);
            Color faderColor = new Color(0f, 0f, 0f, 0f);
            fader.color = faderColor;
            while (fader.color.a < 1)
            {
                faderColor.a += 2f * Time.deltaTime;
                fader.color = faderColor;
                yield return null;
            }

            SceneManager.LoadScene(SceneConstants.TUTORIAL_SCENE_HASH);
        }
        else
        {
            fader.gameObject.SetActive(true);
            Color faderColor = new Color(0f, 0f, 0f, 0f);
            fader.color = faderColor;
            while (fader.color.a < 1)
            {
                faderColor.a += 2f * Time.deltaTime;
                fader.color = faderColor;
                yield return null;
            }

            GameDataHelper.Instance.RestartInstance();
            SceneManager.LoadScene(SceneConstants.INTRO_SCENE_HASH);
        }
    }

    private IEnumerator CreateNextInputWindow(AudioClip audioClip, double intervalUntilScheduledTime)
    {
        yield return new WaitForSecondsRealtime((float)intervalUntilScheduledTime);

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
}
