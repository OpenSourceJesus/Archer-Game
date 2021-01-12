using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	[ExecuteInEditMode]
	public class ObjectWithWaypoints : UpdateWhileEnabled, ICopyable
	{
		public Transform trs;
		public Transform[] wayPoints = new Transform[0];
		public LineRenderer line;
		public Transform lineTrs;
		public WaypointPath path;
		public virtual bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
#if UNITY_EDITOR
		public bool makeLineRenderer;
		public bool autoSetWaypoints;
		public Transform wayPointsParent;
		public new Collider2D collider;
		Vector2 fromPreviousPosition;
#endif

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (collider == null)
					collider = GetComponent<Collider2D>();
				if (wayPointsParent == null)
					wayPointsParent = trs;
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			foreach (Transform waypoint in wayPoints)
				waypoint.SetParent(null);
		}

#if UNITY_EDITOR
		public override void OnEnable ()
		{
			if (Application.isPlaying)
			{
				base.OnEnable ();
				return;
			}
			if (autoSetWaypoints)
				wayPoints = wayPointsParent.GetComponentsInChildren<Transform>().Remove(wayPointsParent);
			if (makeLineRenderer)
			{
				makeLineRenderer = false;
				MakeLineRenderer ();
			}
		}

		public override void OnDestroy ()
		{
			if (Application.isPlaying)
			{
				base.OnDestroy ();
				return;
			}
			EditorApplication.update -= DoEditorUpdate;
		}
		
		// TODO: Optimzie this function
		public virtual void DoEditorUpdate ()
		{
			if (line == null)
				return;
			fromPreviousPosition = wayPoints[0].position - wayPoints[1].position;
			line.SetPosition(0, (Vector2) wayPoints[0].position + (collider.GetSize() / 2).Multiply(fromPreviousPosition.normalized));
			for (int i = 1; i < wayPoints.Length; i ++)
			{
				fromPreviousPosition = wayPoints[i].position - wayPoints[i - 1].position;
				line.SetPosition(i, (Vector2) wayPoints[i].position + (collider.GetSize() / 2).Multiply(fromPreviousPosition.normalized));
			}
		}
		
		// TODO: Optimzie this function
		public virtual void MakeLineRenderer ()
		{
			if (line != null)
				DestroyImmediate(line.gameObject);
			line = new GameObject().AddComponent<LineRenderer>();
			line.positionCount = wayPoints.Length;
			fromPreviousPosition = wayPoints[0].position - wayPoints[1].position;
			line.SetPosition(0, (Vector2) wayPoints[0].position + (collider.GetSize() / 2).Multiply(fromPreviousPosition.normalized));
			for (int i = 1; i < wayPoints.Length; i ++)
			{
				fromPreviousPosition = wayPoints[i].position - wayPoints[i - 1].position;
				line.SetPosition(i, (Vector2) wayPoints[i].position + (collider.GetSize() / 2).Multiply(fromPreviousPosition.normalized));
			}
			line.material = path.material;
			line.startColor = path.color;
			line.endColor = path.color;
			float lineWidth; // TODO: Make this work with a path that "turns" or "bends"
			if (wayPoints[0].position.x != wayPoints[1].position.x)
			{
				if (wayPoints[0].position.y != wayPoints[1].position.y)
					lineWidth = Mathf.Max(collider.GetSize().x, collider.GetSize().y);
				else
					lineWidth = collider.GetSize().y;
			}
			else
				lineWidth = collider.GetSize().x;
			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
			line.sortingLayerName = path.sortingLayerName;
			line.sortingOrder = path.sortingOrder;
			lineTrs = line.GetComponent<Transform>();
			lineTrs.SetParent(trs);
		}
#endif

		// TODO: Optimize this function
		public virtual void Copy (object copy)
		{
#if UNITY_EDITOR
			ObjectWithWaypoints objectWithWaypoints = copy as ObjectWithWaypoints;
			Transform newWaypoint;
			for (int i = 0; i < objectWithWaypoints.wayPoints.Length; i ++)
			{
				if (wayPoints.Length < objectWithWaypoints.wayPoints.Length)
				{
					newWaypoint = Instantiate(objectWithWaypoints.wayPoints[i], wayPointsParent);
					newWaypoint.position = objectWithWaypoints.wayPoints[i].position;
				}
				else if (wayPoints.Length > objectWithWaypoints.wayPoints.Length)
					Destroy(wayPoints[i].gameObject);
				else
					wayPoints[i].position = objectWithWaypoints.wayPoints[i].position;
			}
			wayPoints = wayPointsParent.GetComponentsInChildren<Transform>().Remove(wayPointsParent);
			MakeLineRenderer ();
#endif
		}
	}
}