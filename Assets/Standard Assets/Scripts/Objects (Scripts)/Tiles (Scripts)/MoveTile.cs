using UnityEngine;
using Extensions;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class MoveTile : ObjectWithWaypoints
	{
		public Rigidbody2D rigid;
		public bool loopWaypoints;
		[HideInInspector]
		[SerializeField]
		LineSegment2D[] allLines;
		LineSegment2D stuckOnLine;
		public float moveSpeed;
		public bool moveTowardsEnd;
		[HideInInspector]
		public Vector2 previousPosition;
		public ObjectInWorld worldObject;
		public bool switchDirectionsWhenHit;

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				if (worldObject == null)
					worldObject = GetComponent<ObjectInWorld>();
				if (line == null)
					line = GetComponentInChildren<LineRenderer>();
				line.loop = loopWaypoints;
				return;
			}
#endif
			int lineCount = wayPoints.Length - 1;
			if (loopWaypoints)
				lineCount ++;
			allLines = new LineSegment2D[lineCount];
			for (int i = 0; i < wayPoints.Length - 1; i ++)
				allLines[i] = new LineSegment2D(wayPoints[i].position, wayPoints[i + 1].position);
			if (loopWaypoints)
				allLines[allLines.Length - 1] = new LineSegment2D(wayPoints[wayPoints.Length - 1].position, wayPoints[0].position);
			base.OnEnable ();
		}
		
		public override void DoUpdate ()
		{
			if (GameManager.paused)
				return;
			if ((Vector2) trs.position == previousPosition)
				moveTowardsEnd = !moveTowardsEnd;
			previousPosition = trs.position;
			float distanceToClosestPoint = (allLines[0].ClosestPoint(trs.position) - (Vector2) trs.position).sqrMagnitude;
			float closestDistance = distanceToClosestPoint;
			LineSegment2D _stuckOnLine = allLines[0];
			for (int i = 1; i < allLines.Length; i ++)
			{
				distanceToClosestPoint = (allLines[i].ClosestPoint(trs.position) - (Vector2) trs.position).sqrMagnitude;
				if (distanceToClosestPoint < closestDistance)
				{
					closestDistance = distanceToClosestPoint;
					_stuckOnLine = allLines[i];
				}
			}
			stuckOnLine = _stuckOnLine;
			if (moveTowardsEnd)
				trs.position = stuckOnLine.GetPointWithDirectedDistance(stuckOnLine.GetDirectedDistanceAlongParallel(trs.position) + moveSpeed * Time.deltaTime);
			else
				trs.position = stuckOnLine.GetPointWithDirectedDistance(stuckOnLine.GetDirectedDistanceAlongParallel(trs.position) - moveSpeed * Time.deltaTime);
			trs.position = stuckOnLine.ClosestPoint(trs.position);
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			if (switchDirectionsWhenHit)
				moveTowardsEnd = !moveTowardsEnd;
			OnCollisionStay2D (coll);
		}

		public virtual void OnCollisionStay2D (Collision2D coll)
		{
			if (WorldMap.isOpen || PauseMenu.Instance.gameObject.activeSelf)
				return;
			WorldMapIcon worldMapIcon = coll.gameObject.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			PlatformerEntity platformerEntity = coll.collider.GetComponent<PlatformerEntity>();
			if (platformerEntity != null && coll.GetContact(0).normal.y < -Vector2.one.normalized.x)// && Mathf.Abs(coll.GetContact(0).point.y - platformerEntity.ColliderRect.yMin) <= Physics2D.defaultContactOffset)
			{
				Vector2 moveDirection;
				if (moveTowardsEnd)
					moveDirection = stuckOnLine.GetDirection();
				else
					moveDirection = -stuckOnLine.GetDirection();
				if (platformerEntity.velocityEffectors_Vector2Dict["Move Tile"].effect.magnitude <= moveSpeed)
					platformerEntity.velocityEffectors_Vector2Dict["Move Tile"].effect = moveDirection.normalized * moveSpeed;
			}
		}

		public virtual void OnCollisionExit2D (Collision2D coll)
		{
			if (PauseMenu.Instance.gameObject.activeSelf)
				return;
			if (coll.gameObject.layer == LayerMask.NameToLayer("Map"))
				return;
			PlatformerEntity platformerEntity = coll.collider.GetComponent<PlatformerEntity>();
			if (platformerEntity != null)// && Mathf.Abs(coll.GetContact(0).point.y - platformerEntity.ColliderRect.yMin) <= Physics2D.defaultContactOffset)
				platformerEntity.velocityEffectors_Vector2Dict["Move Tile"].effect = Vector2.zero;
		}

		public override void Copy (object copy)
		{
			// base.Copy (copy);
			// MoveTile moveTile = copy as MoveTile;
			// loopWaypoints = moveTile.loopWaypoints;
			// moveSpeed = moveTile.moveSpeed;
			// moveTowardsEnd = moveTile.moveTowardsEnd;
		}

// #if UNITY_EDITOR
// 		public virtual void OnEnable ()
// 		{
// 			if (Application.isPlaying)
// 				return;
// 			if (GetComponent<ObjectInWorld>().IsInPieces)
// 				PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
// 		}
// #endif
	}
}