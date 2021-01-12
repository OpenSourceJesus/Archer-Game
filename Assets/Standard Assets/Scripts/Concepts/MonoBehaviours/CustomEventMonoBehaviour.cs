using UnityEngine;

public class CustomEventMonoBehaviour : MonoBehaviour
{
    public CustomEvent customEvent;

    public virtual void Do ()
    {
        customEvent.Do ();
    }
}