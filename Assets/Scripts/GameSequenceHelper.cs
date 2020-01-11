using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSequenceHelper : MonoBehaviour
{
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private InputSystem inputSystem;
    [SerializeField] private UpdateCallback updateCallback;

    private List<int> correctnessPerformance = new List<int>();

    private InputWindow activeInputWindow;
    private int playSessionID;

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

    public void SetPlaySessionID(int playSessionID)
    {
        this.playSessionID = playSessionID;
    }

    private void OnSoundScheduled(AudioClip audioClip, double intervalUntilScheduledTime)
    {
        StartCoroutine(CreateNextInputWindow(audioClip, intervalUntilScheduledTime));
    }

    private void OnQueueFinished()
    {
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
        DataHelper.Instance.SubmitPlayData(GetStudyGroup(), correctnessPerformance.ToArray());
    }

    private string GetStudyGroup()
    {
        if (playSessionID % 2 == 0)
        {
            return "AB";
        }
        return "BA";
    }

    private IEnumerator CompletePlaySession()
    {
        yield return new WaitForSecondsRealtime(4f);

        Debug.Log("Game Completed");
        SubmitPlayData();
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
