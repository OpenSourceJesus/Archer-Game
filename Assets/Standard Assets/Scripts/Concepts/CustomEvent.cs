using UnityEngine.Events;
using System;

[Serializable]
public class CustomEvent
{
    public UnityEvent unityEvent;

    public void Do ()
    {
        unityEvent.Invoke();
    }
}