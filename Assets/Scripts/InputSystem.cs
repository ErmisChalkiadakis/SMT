using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public delegate void ButtonPressed(KeyCode keyCode);
    public event ButtonPressed OnButtonPressedEvent;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnButtonPressedEvent?.Invoke(KeyCode.LeftArrow);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnButtonPressedEvent?.Invoke(KeyCode.UpArrow);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnButtonPressedEvent?.Invoke(KeyCode.RightArrow);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnButtonPressedEvent?.Invoke(KeyCode.DownArrow);
        }
        else if (Input.anyKeyDown)
        {
            OnButtonPressedEvent?.Invoke(KeyCode.None);
        }
    }
}
