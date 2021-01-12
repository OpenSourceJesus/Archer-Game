using UnityEngine;
using Extensions;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace ArcherGame
{
	[ExecuteInEditMode]
	public class ToggleTile : MonoBehaviour, ICopyable
	{
		public Transform trs;
		public SpriteRenderer spriteRenderer;
		public new Collider2D collider;
		public int chainDirection = 1;
		public ToggleTile[] correspondingToggleTiles = new ToggleTile[0];
		public int chainIndex;
		public LineRenderer lineRenderer;
		public float divideSpriteRendererAlphaOnDisable;
		public FloatRange lineRendererAlphaRange;
		public ToggleTile initActive;
		public ToggleTile currentActive;
		public bool isOn;
		public Timer undoTimer;
		public int numberOfTogglesAfterInit = 0;
		public float initSpriteRendererAlpha;
#if UNITY_EDITOR
		bool previousIsOn;
#endif

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				previousIsOn = isOn;
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			undoTimer.onFinished += Undo;
			if (this == initActive)
				UpdateLineRenderers (true);
		}

#if UNITY_EDITOR
		// public virtual void OnEnable ()
		// {
		// 	if (!Application.isPlaying)
		// 	{
		// 		if (GetComponent<ObjectInWorld>().IsInPieces)
		// 			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		// 		return;
		// 	}
		// }

		public virtual void DoEditorUpdate ()
		{
			if (isOn != previousIsOn)
			{
				isOn = previousIsOn;
				TurnOnOrOff (!isOn);
			}
			chainIndex = correspondingToggleTiles.IndexOf(this);
			foreach (ToggleTile toggleTile in correspondingToggleTiles)
			{
				if (toggleTile.isOn)
				{
					initActive = toggleTile;
					break;
				}
			}
			if (initActive != null)
			{
				foreach (ToggleTile toggleTile in initActive.correspondingToggleTiles)
				{
					toggleTile.currentActive = initActive;
					toggleTile.correspondingToggleTiles = initActive.correspondingToggleTiles;
					toggleTile.lineRenderer.SetPosition(0, toggleTile.trs.position);
					toggleTile.UpdateLineRenderers (false);
				}
			}
			previousIsOn = isOn;
		}
#endif

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				EditorApplication.update -= DoEditorUpdate;
				return;
			}
#endif
			undoTimer.onFinished -= Undo;
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			Toggle ();
		}

		// public virtual void OnCollisionStay2D (Collision2D coll)
		// {
		// 	Toggle ();
		// }

		public virtual void Toggle ()
		{
			if (chainIndex + initActive.chainDirection == correspondingToggleTiles.Length || chainIndex + initActive.chainDirection == -1)
				initActive.chainDirection *= -1;
			UpdateLineRenderers (false);
			undoTimer.Stop ();
			ToggleTile nextToggleTile = correspondingToggleTiles[chainIndex + initActive.chainDirection];
			initActive.currentActive = nextToggleTile;
			Player.instance.SetIsGrounded ();
			if (Player.instance.isGrounded)
				Player.instance.velocityEffectors_Vector2Dict["Falling"].effect = Vector2.zero;
			Player.instance.HandleJumping ();
			nextToggleTile.TurnOnOrOff (!nextToggleTile.isOn);
			TurnOnOrOff (!isOn);
			nextToggleTile.undoTimer.Reset ();
			nextToggleTile.undoTimer.Start ();
			initActive.numberOfTogglesAfterInit ++;
		}

		public virtual void Undo (params object[] args)
		{
			if (initActive.numberOfTogglesAfterInit == 0)
				return;
			if (chainIndex - initActive.chainDirection == correspondingToggleTiles.Length || chainIndex - initActive.chainDirection == -1)
				initActive.chainDirection *= -1;
			UpdateLineRenderers (true);
			TurnOnOrOff (!isOn);
			ToggleTile nextToggleTile = correspondingToggleTiles[chainIndex - initActive.chainDirection];
			initActive.currentActive = nextToggleTile;
			nextToggleTile.TurnOnOrOff (!nextToggleTile.isOn);
			initActive.numberOfTogglesAfterInit --;
			if (initActive.numberOfTogglesAfterInit > 0)
			{
				nextToggleTile.undoTimer.Reset ();
				nextToggleTile.undoTimer.Start ();
			}
		}

		public virtual void TurnOnOrOff (bool turnOn)
		{
			if (!isOn && turnOn)
				spriteRenderer.color = spriteRenderer.color.SetAlpha(initSpriteRendererAlpha);
			else if (isOn && !turnOn)
				spriteRenderer.color = spriteRenderer.color.DivideAlpha(divideSpriteRendererAlphaOnDisable);
			collider.enabled = turnOn;
			isOn = turnOn;
#if UNITY_EDITOR
			previousIsOn = turnOn;
#endif
		}

		public virtual void UpdateLineRenderers (bool isUndoing)
		{
			if (initActive.chainDirection > 0)
			{
				if (isUndoing)
				{
					for (int i = 0; i < correspondingToggleTiles.Length; i ++)
					{
						ToggleTile toggleTile = correspondingToggleTiles[i];
						LineRenderer _lineRenderer = toggleTile.lineRenderer;
						float lineRednererColorAlpha = Mathf.Lerp(lineRendererAlphaRange.min, lineRendererAlphaRange.max, (float) (i + 1) / correspondingToggleTiles.Length);
						_lineRenderer.startColor = _lineRenderer.startColor.SetAlpha(lineRednererColorAlpha);
						_lineRenderer.endColor = _lineRenderer.endColor.SetAlpha(lineRednererColorAlpha);
						if (i < correspondingToggleTiles.Length - 1 && i >= chainIndex - 1)
						{
							_lineRenderer.SetPosition(1, correspondingToggleTiles[i + 1].trs.position);
							_lineRenderer.enabled = true;
						}
						else
							_lineRenderer.enabled = false;
					}
				}
				else
				{
					for (int i = 0; i < correspondingToggleTiles.Length; i ++)
					{
						ToggleTile toggleTile = correspondingToggleTiles[i];
						LineRenderer _lineRenderer = toggleTile.lineRenderer;
						float lineRednererColorAlpha = Mathf.Lerp(lineRendererAlphaRange.min, lineRendererAlphaRange.max, (float) (i + 1) / correspondingToggleTiles.Length);
						_lineRenderer.startColor = _lineRenderer.startColor.SetAlpha(lineRednererColorAlpha);
						_lineRenderer.endColor = _lineRenderer.endColor.SetAlpha(lineRednererColorAlpha);
						if (i < correspondingToggleTiles.Length - 1 && i > chainIndex)
						{
							_lineRenderer.SetPosition(1, correspondingToggleTiles[i + 1].trs.position);
							_lineRenderer.enabled = true;
						}
						else
						{
							if (i == correspondingToggleTiles.Length - 1)
							{
								_lineRenderer.SetPosition(1, correspondingToggleTiles[i - 1].trs.position);
								_lineRenderer.enabled = true;
							}
							else
								_lineRenderer.enabled = false;
						}
					}
				}
			}
			else
			{
				if (isUndoing)
				{
					for (int i = correspondingToggleTiles.Length - 1; i >= 0; i --)
					{
						ToggleTile toggleTile = correspondingToggleTiles[i];
						LineRenderer _lineRenderer = toggleTile.lineRenderer;
						float lineRednererColorAlpha = Mathf.Lerp(lineRendererAlphaRange.min, lineRendererAlphaRange.max, (float) (i + 1) / correspondingToggleTiles.Length);
						_lineRenderer.startColor = _lineRenderer.startColor.SetAlpha(lineRednererColorAlpha);
						_lineRenderer.endColor = _lineRenderer.endColor.SetAlpha(lineRednererColorAlpha);
						if (i > 0 && i <= chainIndex + 1)
						{
							_lineRenderer.SetPosition(1, correspondingToggleTiles[i - 1].trs.position);
							_lineRenderer.enabled = true;
						}
						else
							_lineRenderer.enabled = false;
					}
				}
				else
				{
					for (int i = correspondingToggleTiles.Length - 1; i >= 0; i --)
					{
						ToggleTile toggleTile = correspondingToggleTiles[i];
						LineRenderer _lineRenderer = toggleTile.lineRenderer;
						float lineRednererColorAlpha = Mathf.Lerp(lineRendererAlphaRange.min, lineRendererAlphaRange.max, (float) (i + 1) / correspondingToggleTiles.Length);
						_lineRenderer.startColor = _lineRenderer.startColor.SetAlpha(lineRednererColorAlpha);
						_lineRenderer.endColor = _lineRenderer.endColor.SetAlpha(lineRednererColorAlpha);
						if (i > 0 && i < chainIndex)
						{
							_lineRenderer.SetPosition(1, correspondingToggleTiles[i - 1].trs.position);
							_lineRenderer.enabled = true;
						}
						else
						{
							if (i == 0)
							{
								_lineRenderer.SetPosition(1, correspondingToggleTiles[i + 1].trs.position);
								_lineRenderer.enabled = true;
							}
							else
								_lineRenderer.enabled = false;
						}
					}
				}
			}
		}

		public virtual void Copy (object copy)
		{
#if UNITY_EDITOR
			EditorCoroutineUtility.StartCoroutine(CopyRoutine (copy as ToggleTile), this);
#endif
		}

#if UNITY_EDITOR
		public virtual IEnumerator CopyRoutine (ToggleTile toggleTile)
		{
			EditorApplication.update -= DoEditorUpdate;
			trs = GetComponent<Transform>().GetChild(0);
			lineRenderer = GetComponent<LineRenderer>();
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			collider = GetComponentInChildren<Collider2D>();
			TurnOnOrOff (toggleTile.isOn);
			divideSpriteRendererAlphaOnDisable = toggleTile.divideSpriteRendererAlphaOnDisable;
			lineRendererAlphaRange = toggleTile.lineRendererAlphaRange;
			chainDirection = toggleTile.chainDirection;
			correspondingToggleTiles = new ToggleTile[toggleTile.correspondingToggleTiles.Length];
			ObjectInWorld worldObject = GetComponent<ObjectInWorld>();
			ObjectInWorld otherWorldObject;
			for (int i = 0; i < correspondingToggleTiles.Length; i ++)
			{
				otherWorldObject = toggleTile.correspondingToggleTiles[i].GetComponent<ObjectInWorld>();
				if (worldObject.IsInPieces != otherWorldObject.IsInPieces)
				{
					yield return new WaitUntil(() => (otherWorldObject.duplicateGo != null));
					correspondingToggleTiles[i] = otherWorldObject.duplicateGo.GetComponent<ToggleTile>();
				}
			}
			DoEditorUpdate ();
		}
#endif
	}
}