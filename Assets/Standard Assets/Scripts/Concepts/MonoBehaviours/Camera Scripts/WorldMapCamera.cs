using UnityEngine;
using Extensions;

namespace ArcherGame
{
	public class WorldMapCamera : CameraScript
	{
		public static WorldMapCamera instance;
		public static WorldMapCamera Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<WorldMapCamera>();
				return instance;
			}
		}
		public float sizeMultiplier;
		public float zoomRate;
		public FloatRange sizeMultiplierRange;
		[SerializeField]
		Vector2 initViewSize;
		Rect previousViewRect;
		float zoomInput;
		Vector2 worldMousePosition;
		Vector2 previousWorldMousePosition;

		public override void Awake ()
		{
			base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				initViewSize = viewSize;
				return;
			}
#endif
		}

		public override void DoUpdate ()
		{
			zoomInput = InputManager.GetZoomInput(MathfExtensions.NULL_INT);
			HandleViewSize ();
			base.DoUpdate ();
			previousViewRect = viewRect;
			// previousWorldMousePosition = InputManager.GetWorldMousePosition(MathfExtensions.NULL_INT);
			previousWorldMousePosition = camera.ScreenToWorldPoint(GameManager.activeCursorEntry.rectTrs.position);
		}

		public override void HandlePosition ()
		{
			if (zoomInput != 0 && sizeMultiplierRange.Contains(sizeMultiplier, false))
			{
				worldMousePosition = camera.ScreenToWorldPoint(GameManager.activeCursorEntry.rectTrs.position);
				trs.position -= (Vector3) ((worldMousePosition - viewRect.center) - (previousWorldMousePosition - previousViewRect.center));
				if (GameManager.Instance.worldMapTutorialConversation.lastStartedDialog == GameManager.Instance.worldMapZoomViewTutorialDialog)
					GameManager.Instance.DeactivateGoForever (GameManager.Instance.worldMapTutorialConversation.gameObject);
			}
			base.HandlePosition ();
		}

		public override void HandleViewSize ()
		{
			sizeMultiplier = Mathf.Clamp(sizeMultiplier + zoomInput * zoomRate * Time.unscaledDeltaTime, sizeMultiplierRange.min, sizeMultiplierRange.max);
			viewSize = initViewSize * sizeMultiplier;
			base.HandleViewSize ();
		}

		CameraView previousCameraView;
		public virtual void SetView (CameraView cameraView)
		{
			if (previousCameraView != null)
				previousCameraView.gameObject.SetActive(false);
			trs.position = cameraView.trs.position;
			viewSize = cameraView.trs.localScale;
			base.HandleViewSize ();
			cameraView.gameObject.SetActive(true);
			previousCameraView = cameraView;
		}

		public virtual void SetView (string cameraViewName)
		{
			SetView (CameraView.cameraViewDict[cameraViewName]);
		}

		public virtual void Enable ()
		{
			if (WorldMapCamera.Instance != this)
			{
				WorldMapCamera.instance.Enable ();
				return;
			}
			enabled = true;
		}

		public virtual void Disable ()
		{
			if (WorldMapCamera.Instance != this)
			{
				WorldMapCamera.instance.Disable ();
				return;
			}
			enabled = false;
		}
	}
}