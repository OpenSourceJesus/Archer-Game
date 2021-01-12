using UnityEngine;
using Extensions;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace ArcherGame
{
	//[ExecuteInEditMode]
	public class AwakableEnemy : Enemy
	{
		public LayerMask whatBlocksVision;
		[HideInInspector]
		public bool awake;
		public CircleCollider2D visionCollider;
		public CircleCollider2D followRangeCollider;
		public float VisionRange
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
					return Mathf.Abs(visionCollider.radius * visionCollider.GetComponent<Transform>().lossyScale.x);
#endif
				return visionCollider.bounds.extents.x;
			}
		}
		public float FollowRange
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
					return Mathf.Abs(followRangeCollider.radius * followRangeCollider.GetComponent<Transform>().lossyScale.x);
#endif
				return followRangeCollider.bounds.extents.x;
			}
		}
		public AttackEntry[] attackEntries = new AttackEntry[0];
		public Dictionary<string, AttackEntry> attackEntriesDict = new Dictionary<string, AttackEntry>();
		public float minChaseDistFromPlayer;
		[HideInInspector]
		public float minChaseDistFromPlayerSqr;
		public float maxEsacpeDistFromPlayer;
		[HideInInspector]
		public float maxEsacpeDistFromPlayerSqr;
		public bool invulnerable = true;
		AwakableEnemy[] otherAwakableEnemies = new AwakableEnemy[0];

		public override void Start ()
		{
			base.Start ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (visionCollider == null)
					visionCollider = GetComponent<Transform>().Find("Vision Range").GetComponent<CircleCollider2D>();
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				else
					Init ();
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			if (followRangeCollider != null)
				Destroy(followRangeCollider.gameObject);
			attackEntriesDict.Clear();
			foreach (AttackEntry attackEntry in attackEntries)
				attackEntriesDict.Add(attackEntry.name, attackEntry);
		}

		public override void Init ()
		{
			base.Init ();
			minChaseDistFromPlayerSqr = minChaseDistFromPlayer * minChaseDistFromPlayer;
			maxEsacpeDistFromPlayerSqr = maxEsacpeDistFromPlayer * maxEsacpeDistFromPlayer;
		}

		public override void Reset ()
		{
			invulnerable = true;
			awake = false;
			visionCollider.gameObject.SetActive(true);
			base.Reset ();
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			OnTriggerStay2D (other);
		}

		public virtual void OnTriggerStay2D (Collider2D other)
		{
			if (Physics2D.Raycast(trs.position, Player.instance.trs.position - trs.position, Mathf.Min(VisionRange, Vector2.Distance(Player.instance.trs.position, trs.position)), whatBlocksVision).transform == Player.instance.trs)
			{
				battleIAmPartOf.followRangesVisualizerGo.SetActive(true);
				Awaken ();
			}
		}

		public virtual void Awaken ()
		{
			awake = true;
			if (battleIAmPartOf != null)
			{
				foreach (AwakableEnemy awakableEnemy in battleIAmPartOf.awakableEnemies)
				{
					if (!awakableEnemy.awake)
						awakableEnemy.Awaken ();
				}
				battleIAmPartOf.followRangesVisualizerGo.SetActive(true);
				battleIAmPartOf.infoTrs.gameObject.SetActive(false);
				battleIAmPartOf.StopCoroutine(battleIAmPartOf.SetInfoTrsPositionRoutine ());
			}
			visionCollider.gameObject.SetActive(false);
			if (shooter != null)
				shooter.shootTimer.Start ();
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public override void DoUpdate ()
		{
			if (!awake || GameManager.paused || isDead)// || this == null)
				return;
			base.DoUpdate ();
			HandleAttacking ();
		}

		public override void Move (Vector2 move)
		{
			if (((Vector2) (Player.instance.trs.position - trs.position)).sqrMagnitude > minChaseDistFromPlayerSqr && ((Vector2) Player.instance.trs.position - ((Vector2) trs.position + (move * velocityEffectors_floatDict["Move Speed"].effect * Time.deltaTime))).sqrMagnitude > minChaseDistFromPlayerSqr)
				base.Move (move);
			else if (((Vector2) (Player.instance.trs.position - trs.position)).sqrMagnitude < maxEsacpeDistFromPlayerSqr && ((Vector2) Player.instance.trs.position - ((Vector2) trs.position - (move * velocityEffectors_floatDict["Move Speed"].effect * Time.deltaTime))).sqrMagnitude < maxEsacpeDistFromPlayerSqr)
				base.Move (-move);
			else
				base.Move (Vector2.zero);
		}

		public override void TakeDamage (float damage)
		{
			if (invulnerable)
				return;
			base.TakeDamage (damage);
			if (!awake && damage > 0)
				Awaken ();
		}

		public virtual void HandleAttacking ()
		{
			foreach (AttackEntry attackEntry in attackEntries)
			{
				if (attackEntry.attack.ShouldDo(attackEntry.data))
					attackEntry.attack.Do (attackEntry.data);
			}
		}

		public override void Death ()
		{
			base.Death ();
			foreach (AttackEntry attackEntry in attackEntries)
				attackEntry.data.tempActiveObject.obj.SetActive(false);
		}

		[Serializable]
		public class AttackEntry
		{
			public string name;
			public Attack attack;
			public Attack.InstanceData data;
		}
		
#if UNITY_EDITOR
		public override void OnDisable ()
		{
			base.OnDisable ();
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}

		public virtual void DoEditorUpdate ()
		{
			Init ();
			if (battleIAmPartOf == null)
			{
				foreach (EnemyBattle enemyBattle in World.Instance.enemyBattles)
				{
					if (enemyBattle.enemies.Contains(this))
					{
						battleIAmPartOf = enemyBattle;
						break;
					}
				}
			}
		}

 		// [MenuItem("World/Set All Enemy Battles")]
		// public static void SetAllEnemyBattles ()
		// {
		// 	SetEnemyBattles (FindObjectsOfType<AwakableEnemy>());
		// }
		
 		// [MenuItem("World/Set Selected Enemy Battles")]
		// public static void SetSelectedEnemyBattles ()
		// {
		// 	SetEnemyBattles (SelectionExtensions.GetSelected<AwakableEnemy>());
		// }

		[MenuItem("World/Set Selected Enemy Battles And Skip Pathfinding")]
		public static void SetSelectedEnemyBattlesSkipPathfinding ()
		{
			WorldMakerWindow.UpdateEnemyBattles ();
			Enemy[] enemies = SelectionExtensions.GetSelected<Enemy>();
			Enemy enemy;
			for (int i = 0; i < enemies.Length; i ++)
 			{
				enemy = enemies[i];
				if (!enemy.worldObject.IsInPieces && enemy.worldObject.duplicateGo != null)
					enemies[i] = enemy.worldObject.duplicateGo.GetComponent<Enemy>();
			}
			AwakableEnemy[] awakableEnemies = SelectionExtensions.GetSelected<AwakableEnemy>();
			// EnemyBattle enemyBattle = PrefabUtilityExtensions.ClonePrefabInstance(World.Instance.enemyBattlePrefab.gameObject).GetComponent<EnemyBattle>();
			EnemyBattle enemyBattle = Instantiate(World.Instance.enemyBattlePrefab);
			enemyBattle.enemies = enemies;
			enemyBattle.awakableEnemies = awakableEnemies;
			for (int i = 0; i < enemies.Length; i ++)
 			{
				enemy = enemies[i];
				if (enemy.battleIAmPartOf != null)
				{
					World.Instance.enemyBattles = World.Instance.enemyBattles.Remove(enemy.battleIAmPartOf);
					DestroyImmediate(enemy.battleIAmPartOf.gameObject);
				}
				enemy.battleIAmPartOf = enemyBattle;
 			}
			enemyBattle.update = true;
			World.Instance.enemyBattles = World.Instance.enemyBattles.Add(enemyBattle);
		}
		
 		// public static void SetEnemyBattles (AwakableEnemy[] awakableEnemies)
 		// {
		// 	EnemyBattle enemyBattle;
		// 	Path path;
 		// 	foreach (AwakableEnemy awakableEnemy in awakableEnemies)
 		// 	{
		// 		if (awakableEnemy.battleIInitialize != null)
		// 		{
		// 			World.Instance.enemyBattles = World.Instance.enemyBattles.Remove(awakableEnemy.battleIInitialize);
		// 			DestroyImmediate(awakableEnemy.battleIInitialize.gameObject);
		// 		}
		// 		// enemyBattle = PrefabUtilityExtensions.ClonePrefabInstance(World.Instance.enemyBattlePrefab.gameObject).GetComponent<EnemyBattle>();
		// 		enemyBattle = Instantiate(World.Instance.enemyBattlePrefab);
		// 		enemyBattle.initialEnemy = awakableEnemy;
		// 		enemyBattle.enemies = enemyBattle.enemies.Add(awakableEnemy);
		// 		enemyBattle.awakableEnemies = enemyBattle.awakableEnemies.Add(awakableEnemy);
 		// 		foreach (AwakableEnemy otherAwakableEnemy in awakableEnemies)
 		// 		{
 		// 			if (awakableEnemy != otherAwakableEnemy)
 		// 			{
 		// 				path = awakableEnemy.GetPath(otherAwakableEnemy);
 		// 				if (path != null)
		// 				{
		// 					enemyBattle.enemies = enemyBattle.enemies.Add(otherAwakableEnemy);
		// 					enemyBattle.awakableEnemies = enemyBattle.awakableEnemies.Add(otherAwakableEnemy);
		// 					enemyBattle.warnEnemies = enemyBattle.warnEnemies.Add(otherAwakableEnemy);
		// 				}
 		// 			}
 		// 		}
		// 		awakableEnemy.battleIInitialize = enemyBattle;
		// 		awakableEnemy.battlesIAmPartOf = awakableEnemy.battlesIAmPartOf.Add(enemyBattle);
		// 		enemyBattle.update = true;
		// 		World.Instance.enemyBattles = World.Instance.enemyBattles.Add(enemyBattle);
 		// 	}
 		// }

 		// public LayerMask whatBlocksMe;
 		// public virtual Path GetPath (AwakableEnemy awakableEnemy)
 		// {
 		// 	if (Vector2.Distance(trs.position, awakableEnemy.trs.position) > VisionRange + awakableEnemy.VisionRange)
 		// 		return null;
 		// 	Vector2Int start = World.Instance.tilemaps[0].WorldToCell(trs.position).ToVec2Int();
 		// 	Vector2Int end = World.Instance.tilemaps[0].WorldToCell(awakableEnemy.trs.position).ToVec2Int();
 		// 	TreeNode<Vector2Int> pathTree = new TreeNode<Vector2Int>(start);
 		// 	Vector2Int[] neighboringPoints = new Vector2Int[4];
 		// 	List<Vector2Int> unvisitedPoints = new List<Vector2Int>();
 		// 	unvisitedPoints.Add(start);
 		// 	do
 		// 	{
 		// 		if (unvisitedPoints[0] == end)
 		// 			break;
 		// 		neighboringPoints[0] = unvisitedPoints[0] + Vector2Int.up;
 		// 		neighboringPoints[1] = unvisitedPoints[0] + Vector2Int.right;
 		// 		neighboringPoints[2] = unvisitedPoints[0] + Vector2Int.down;
 		// 		neighboringPoints[3] = unvisitedPoints[0] + Vector2Int.left;
 		// 		foreach (Vector2Int neighboringPoint in neighboringPoints)
 		// 			ExplorePoint (neighboringPoint, ref pathTree, ref unvisitedPoints, awakableEnemy);
 		// 		unvisitedPoints.RemoveAt(0);
 		// 	} while (unvisitedPoints.Count > 0);
 		// 	if (unvisitedPoints.Count == 0)
 		// 		return null;
 		// 	List<Vector2Int> points = new List<Vector2Int>();
 		// 	points.Add(end);
 		// 	Path path = new Path();
 		// 	TreeNode<Vector2Int> node = pathTree.GetRoot().GetChild(end);
 		// 	while (node.Parent != null)
 		// 	{
 		// 		node = node.Parent;
 		// 		points.Insert(0, node.Value);
 		// 	}
 		// 	path.points = points.ToArray();
 		// 	return path;
 		// }

 		// public virtual void ExplorePoint (Vector2Int point, ref TreeNode<Vector2Int> pathTree, ref List<Vector2Int> unvisitedPoints, AwakableEnemy awakableEnemy)
 		// {
 		// 	if ((Vector2.Distance(point, trs.position) <= VisionRange || Vector2.Distance(point, awakableEnemy.trs.position) <= awakableEnemy.VisionRange) && PointIsOpen(point) && !pathTree.GetRoot().Contains(point))
 		// 	{
 		// 		pathTree.GetRoot().GetChild(unvisitedPoints[0]).AddChild(point);
 		// 		unvisitedPoints.Add(point);
 		// 	}
 		// }

		// public virtual bool PointIsOpen (Vector2Int point)
		// {
		// 	if (Application.isPlaying)
		// 		return Physics2D.OverlapPoint(World.Instance.tilemaps[0].GetCellCenterWorld(point.ToVec3Int()), whatBlocksMe) == null;
		// 	else
		// 	{
		// 		foreach (ObjectInWorld worldObject in World.Instance.worldObjects)
		// 		{
		// 			if (worldObject.trs.GetUnrotatedRect().Contains(point) && whatBlocksMe.MaskContainsLayer(worldObject.go.layer))
		// 				return false;
		// 		}
		// 		return !World.Instance.tilemaps[0].HasTile(point.ToVec3Int());
		// 	}
		// }

 		// public class Path
 		// {
 		// 	public Vector2Int[] points;
 		// }
#endif
	}
}