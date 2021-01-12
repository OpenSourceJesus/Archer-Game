using UnityEngine;
using Extensions;
using ArcherGame;

public class RotateTransform : MonoBehaviour, IUpdatable
{
    public float rate;
    public Transform trs;
    public bool PauseWhileUnfocused
    {
        get
        {
            return true;
        }
    }
    
    public virtual void Start ()
    {
        GameManager.updatables = GameManager.updatables.Add(this);
    }

    public virtual void OnDestroy ()
    {
        GameManager.updatables = GameManager.updatables.Remove(this);
    }

    public virtual void DoUpdate ()
    {
        trs.eulerAngles += Vector3.forward * rate * Time.deltaTime;
    }
}