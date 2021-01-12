using UnityEngine;
using UnityEngine.UI;
using Extensions;
using ArcherGame;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[ExecuteInEditMode]
public class _Selectable : MonoBehaviour
{
	public Canvas canvas;
	public RectTransform canvasRectTrs;
#if UNITY_EDITOR
	public bool updateCanvas = true;
#endif
	public RectTransform rectTrs;
	public Selectable selectable;
	public float priority;
	public bool Interactable
	{
		get
		{
			return selectable.interactable;
		}
		set
		{
			if (value != selectable.interactable)
			{
				if (value)
					OnEnable ();
				else
					OnDisable ();
			}
			selectable.interactable = value;
		}
	}
	public Scrollbar scrollbarThatMovesMe;
	public RectTransform container;
	
	public virtual void UpdateCanvas ()
	{
		canvas = GetComponent<Canvas>();
		canvasRectTrs = GetComponent<RectTransform>();
		while (canvas == null)
		{
			canvasRectTrs = canvasRectTrs.parent.GetComponent<RectTransform>();
			canvas = canvasRectTrs.GetComponent<Canvas>();
		}
	}
	
	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (rectTrs == null)
				rectTrs = GetComponent<RectTransform>();
			if (selectable == null)
				selectable = GetComponent<Selectable>();
			if (GameManager.Instance.doEditorUpdates)
				EditorApplication.update += DoEditorUpdate;
			return;
		}
		else
			EditorApplication.update -= DoEditorUpdate;
#endif
		UIControlManager.selectables = UIControlManager.selectables.Add(this);
		UpdateCanvas ();
	}
	
	public virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			EditorApplication.update -= DoEditorUpdate;
			return;
		}
#endif
		UIControlManager.selectables = UIControlManager.selectables.Remove(this);
	}
	
#if UNITY_EDITOR
	public virtual void DoEditorUpdate ()
	{
		if (updateCanvas)
		{
			updateCanvas = false;
			UpdateCanvas ();
		}
	}
#endif
}
