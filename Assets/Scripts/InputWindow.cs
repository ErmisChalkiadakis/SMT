using UnityEngine;

public class InputWindow
{
    public delegate void WindowShut(bool result);
    public event WindowShut OnWindowShutEvent;

    public delegate void ButtonPressed(bool result);
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
        if (AudioSettings.dspTime > windowShutTime - 0.15f)
        {
            if (!anyButtonPressed)
            {
                Debug.Log("Out of time");
            }
            OnWindowShutEvent?.Invoke(correctButtonPressed);
            Deactivate();
        }
    }
    
    private void OnButtonPressed(KeyCode keyCode)
    {
        if (keyCode == requiredKeyCode)
        {
            Debug.Log("Correct Input");
            OnButtonPressedEvent?.Invoke(true);
            correctButtonPressed = true;
        }
        else
        {
            Debug.Log("Incorrect Input");
            OnButtonPressedEvent?.Invoke(false);
            correctButtonPressed = false;
        }
        anyButtonPressed = true;
    }
}
