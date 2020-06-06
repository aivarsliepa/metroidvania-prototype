using UnityEngine;

public class TriggerCheck : MonoBehaviour
{
    public BoolUnityEvent triggerEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggerEvent?.Invoke(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        triggerEvent?.Invoke(false);
    }
}
