using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSequenceHelper : MonoBehaviour
{
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private InputSystem inputSystem;
    [SerializeField] private UpdateCallback updateCallback;

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
        correctnessPerformance.Add(1);
    }

    private void IncorrectResult()
    {
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
        
        if (!GameDataHelper.Instance.isStudyComplete)
        {
            SceneManager.LoadScene(SceneConstants.TUTORIAL_SCENE_HASH);
        }
        else
        {
            Debug.Log("Game Completed");
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
