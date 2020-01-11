using UnityEngine;

public class UpdateCallback : MonoBehaviour
{
    public delegate void OnUpdate();
    public event OnUpdate OnUpdateEvent;
    
    void Update()
    {
        OnUpdateEvent?.Invoke();
    }
}
