using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	//[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	public class CameraScript : SingletonMonoBehaviour<CameraScript>
	{
		public Transform trs;
		public new Camera camera;
		public Vector2 viewSize;
		protected Rect normalizedScreenViewRect;
		protected float screenAspect;
		[HideInInspector]
		public Rect viewRect;
		
		public override void Awake ()
		{
			// base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (camera == null)
					camera = GetComponent<Camera>();
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			// trs.SetParent(null);
			// trs.localScale = Vector3.one;
			viewRect.size = viewSize;
			HandlePosition ();
			// Canvas.ForceUpdateCanvases();
			StartCoroutine(InitRoutine ());
		}

		IEnumerator InitRoutine ()
		{
			yield return new WaitUntil(() => (GameManager.Instance.gameViewCanvas != null));
			GameManager.instance.gameViewCanvas.enabled = false;
			GameManager.instance.gameViewCanvas.enabled = true;
			HandleViewSize ();
		}

#if UNITY_EDITOR
		public virtual void OnEnable ()
		{
			// if (!Application.isPlaying)
			// 	return;
			HandleViewSize ();
		}

		public virtual void DoEditorUpdate ()
		{
			// HandleViewSize ();
		}

		public virtual void OnDestroy ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}

		public virtual void OnDisable ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}
#endif

		public virtual void DoUpdate ()
		{
			HandlePosition ();
		}
		
		public virtual void HandlePosition ()
		{
			viewRect.center = trs.position;
		}
		
		public virtual void HandleViewSize ()
		{
			screenAspect = GameManager.Instance.gameViewRectTrs.rect.size.x / GameManager.instance.gameViewRectTrs.rect.size.y;
			// screenAspect = Screen.width / Screen.height;
			camera.aspect = viewSize.x / viewSize.y;
			camera.orthographicSize = Mathf.Min(viewSize.x / 2 / camera.aspect, viewSize.y / 2);
			normalizedScreenViewRect = new Rect();
			normalizedScreenViewRect.size = new Vector2(camera.aspect / screenAspect, Mathf.Min(1, screenAspect / camera.aspect));
			normalizedScreenViewRect.center = Vector2.one / 2;
			camera.rect = normalizedScreenViewRect;
		}
	}
}