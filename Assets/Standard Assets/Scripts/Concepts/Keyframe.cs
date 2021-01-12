using System;
using UnityEngine;
using DialogAndStory;
using UnityEngine.UI;

[Serializable]
public class KeyFrame
{
	public bool usePosition;
	public Vector3 position;
	public bool useLocalPosition;
	public Vector3 localPosition;
	public bool useRotation;
	public Vector3 rotation;
	public bool useLocalRotation;
	public Vector3 localRotation;
	public bool useLocalScale;
	public Vector3 localScale;
	public bool useStartDialog;
	public Dialog startDialog;
	public bool useActivateButton;
	public Button activateButton;
	public Color visualsColor;
	public float visualsSize;
	[HideInInspector]
	public _Animation animation;

	public virtual KeyFrame Copy ()
	{
		KeyFrame output = new KeyFrame();
		output.usePosition = usePosition;
		output.position = position;
		output.useLocalPosition = useLocalPosition;
		output.localPosition = localPosition;
		output.useRotation = useRotation;
		output.rotation = rotation;
		output.useLocalRotation = useLocalRotation;
		output.localRotation = localRotation;
		output.useLocalScale = useLocalScale;
		output.localScale = localScale;
		output.useStartDialog = useStartDialog;
		output.startDialog = startDialog;
		output.useActivateButton = useActivateButton;
		output.activateButton = activateButton;
		return output;
	}

#if UNITY_EDITOR
	public virtual void Show ()
	{
		Gizmos.color = visualsColor;
		if (usePosition || useLocalPosition)
			Gizmos.DrawSphere(animation.trs.position, visualsSize);
	}
#endif
}