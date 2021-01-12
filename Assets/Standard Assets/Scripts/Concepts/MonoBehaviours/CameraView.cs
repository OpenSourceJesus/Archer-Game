using Extensions;
using ArcherGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    public Transform trs;
    public static Dictionary<string, CameraView> cameraViewDict = new Dictionary<string, CameraView>();

    public virtual void Start ()
    {
        gameObject.SetActive(false);
        cameraViewDict.Add(name, this);
    }

    public virtual void OnDestroy ()
    {
        cameraViewDict.Remove(name);
    }
}