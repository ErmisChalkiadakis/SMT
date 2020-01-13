using UnityEngine;

public class InputWindow
{
    private const double BEAT_INTERVAL = 1.848;

    public delegate void WindowShut(bool result);
    public event WindowShut OnWindowShutEvent;

    public delegate void ButtonPressed(bool result, double reactionTime);
    public event ButtonPressed OnButtonPressedEvent;

    public AudioClip playingAudioClip { get; private set; }
    public KeyCode requiredKeyCode { get; private set; }

    private InputSystem inputSystem;
    private UpdateCallback updateCallback;
    private double windowShutTime;
    private bool correctButtonPressed;
    private bool anyButtonPressed;

    public InputWindow(AudioClip playingAudioClip, KeyCode requiredKeyCode, double windowShutTime, InputSystem inputSystem, UpdateCallback updateCallback)
    {
        this.playingAudioClip = playingAudioClip;
        this.requiredKeyCode = requiredKeyCode;
        this.windowShutTime = windowShutTime;
        this.inputSystem = inputSystem;
        this.updateCallback = updateCallback;

        inputSystem.OnButtonPressedEvent += OnButtonPressed;
        updateCallback.OnUpdateEvent += Update;
    }

    public void Deactivate()
    {
        inputSystem.OnButtonPressedEvent -= OnButtonPressed;
        updateCallback.OnUpdateEvent -= Update;
    }

    private void Update()
    {
        if (AudioSettings.dspTime > windowShutTime - 0.1f)
        {
            if (!anyButtonPressed)
            {
                Debug.Log("Out of time");
            }
            OnWindowShutEvent?.Invoke(correctButtonPressed);
            OnButtonPressedEvent?.Invoke(false, AudioSettings.dspTime + BEAT_INTERVAL - windowShutTime);
            Deactivate();
        }
    }
    
    private void OnButtonPressed(KeyCode keyCode)
    {
        if (anyButtonPressed)
        {
            Debug.Log("Can't take more than one input, try to be precise");
            return;
        }

        if (keyCode == requiredKeyCode)
        {
            Debug.Log("Correct Input");
            OnButtonPressedEvent?.Invoke(true, AudioSettings.dspTime + BEAT_INTERVAL - windowShutTime);
            correctButtonPressed = true;
        }
        else
        {
            Debug.Log("Incorrect Input");
            OnButtonPressedEvent?.Invoke(true, AudioSettings.dspTime + BEAT_INTERVAL - windowShutTime);
            correctButtonPressed = false;
        }
        anyButtonPressed = true;
    }
}
