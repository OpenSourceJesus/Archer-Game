using UnityEngine;
using Extensions;
using TMPro;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CompositeCollider2D))]
	public class EnemyBattle : MonoBehaviour
	{
		public bool defeated;
		public bool DefeatedAndSaved
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + ArchivesManager.currentAccountIndex + " " + name + " defeated and saved", 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("Account " + ArchivesManager.currentAccountIndex + " " + name + " defeated and saved", value.GetHashCode());
			}
		}
		public int money;
		public Enemy[] enemies = new Enemy[0];
		public AwakableEnemy[] awakableEnemies = new AwakableEnemy[0];
		public Transform trs;
		public CompositeCollider2D compositeCollider;
		public GameObject followRangesVisualizerGo;
		public Transform infoTrs;
		public TMP_Text moneyText;
		public GameObject defeatedIndicator;
		public GameObject moneyInfoParentGo;
		public float infoTrsPositionOffset;
#if UNITY_EDITOR
		public MakeCirclePolygonCollider2D[] makeCirclePolygonColliders;
		public bool update;
		public int pointsPerPolygonCollider;
#endif
		int enemiesDead;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (compositeCollider == null)
					compositeCollider = GetComponent<CompositeCollider2D>();
				makeCirclePolygonColliders = GetComponents<MakeCirclePolygonCollider2D>();
				if (GameManager.Instance.doEditorUpdates)
					EditorApplication.update += DoEditorUpdate;
				// List<RemovedComponent> removedComponents = PrefabUtility.GetRemovedComponents(gameObject);
				// foreach (RemovedComponent removedComponent in removedComponents)
				// 	removedComponent.Revert();
				// MakeSpriteFromCollider2D makeSpriteFromCollider = GetComponentInChildren<MakeSpriteFromCollider2D>();
				// if (makeSpriteFromCollider != null && makeSpriteFromCollider.enabled && makeSpriteFromCollider.spriteRenderer.sprite == null)
				// 	makeSpriteFromCollider.Do ();
				infoTrs = trs.Find("Info Canvas");
				Transform uiPanelTrs = infoTrs.Find("UI Panel");
				Transform moneyInfoParent = uiPanelTrs.Find("Horizontal Layout Group");
				moneyInfoParentGo = moneyInfoParent.gameObject;
				defeatedIndicator = uiPanelTrs.Find("Defeated Text").gameObject;
				moneyText = moneyInfoParent.Find("Money Text").GetComponent<TMP_Text>();
				moneyText.text = "" + money;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
#endif
			foreach (Enemy enemy in enemies)
				enemy.SetOnDeathListener (OnEnemyDeath);
		}

		public virtual void OnEnemyDeath ()
		{
			enemiesDead ++;
			if (enemiesDead == enemies.Length)
			{
				if (!DefeatedAndSaved)
				{
					Player.addToMoneyOnSave += money;
					Player.instance.DisplayMoney ();
					defeated = true;
				}
				followRangesVisualizerGo.SetActive(false);
			}
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			if (enemiesDead < enemies.Length)
			{
				foreach (AwakableEnemy awakableEnemy in awakableEnemies)
				{
					awakableEnemy.invulnerable = false;
					if (awakableEnemy.spriteSkin != null)
						awakableEnemy.spriteSkin.enabled = true;
				}
				SetInfoTrsPosition ();
				if (DefeatedAndSaved)
				{
					moneyInfoParentGo.SetActive(false);
					defeatedIndicator.SetActive(true);
				}
				infoTrs.gameObject.SetActive(true);
				StartCoroutine(SetInfoTrsPositionRoutine ());
			}
			// followRangesVisualizerGo.SetActive(true);
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			if (enemiesDead < enemies.Length)
			{
				enemiesDead = 0;
				foreach (AwakableEnemy awakableEnemy in awakableEnemies)
					awakableEnemy.Reset ();
			}
			followRangesVisualizerGo.SetActive(false);
			infoTrs.gameObject.SetActive(false);
			StopCoroutine(SetInfoTrsPositionRoutine ());
		}

		public IEnumerator SetInfoTrsPositionRoutine ()
		{
			do
			{
				SetInfoTrsPosition ();
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		void SetInfoTrsPosition ()
		{
			Vector2 position = ClosestPointInVisionRanges(Player.instance.trs.position);
			int indexOfClosestVisionRange = IndexOfClosestVisionRange(Player.instance.trs.position);
			position += ((Vector2) awakableEnemies[indexOfClosestVisionRange].visionCollider.bounds.center - position).normalized * infoTrsPositionOffset;
			infoTrs.position = position;
		}

		Vector2 ClosestPointInVisionRanges (Vector2 point)
		{
			Vector2[] closestPoints = new Vector2[awakableEnemies.Length];
			for (int i = 0; i < awakableEnemies.Length; i ++)
			{
				AwakableEnemy awakableEnemy = awakableEnemies[i];
				closestPoints[i] = awakableEnemy.visionCollider.ClosestPoint(point);
			}
			return point.GetClosestPoint(closestPoints);
		}

		int IndexOfClosestVisionRange (Vector2 point)
		{
			Vector2[] closestPoints = new Vector2[awakableEnemies.Length];
			for (int i = 0; i < awakableEnemies.Length; i ++)
			{
				AwakableEnemy awakableEnemy = awakableEnemies[i];
				closestPoints[i] = awakableEnemy.visionCollider.ClosestPoint(point);
			}
			return point.GetIndexOfClosestPoint(closestPoints);
		}

#if UNITY_EDITOR
		AwakableEnemy awakableEnemy;
		MakeCirclePolygonCollider2D makeCirclePolygonCollider;
		Transform followRangeTrs;
		public virtual void DoEditorUpdate ()
		{
			if (!update)
				return;
			update = false;
			PolygonCollider2D polygonCollider;
			foreach (MakeCirclePolygonCollider2D makeCirclePolygonCollider in makeCirclePolygonColliders)
			{
				polygonCollider = makeCirclePolygonCollider.polygonCollider;
				DestroyImmediate(makeCirclePolygonCollider);
				DestroyImmediate(polygonCollider);
			}
			makeCirclePolygonColliders = new MakeCirclePolygonCollider2D[0];
			foreach (AwakableEnemy awakableEnemy in awakableEnemies)
			{
				makeCirclePolygonCollider = gameObject.AddComponent<MakeCirclePolygonCollider2D>();
				if (awakableEnemy.followRangeCollider == null)
					awakableEnemy.followRangeCollider = awakableEnemy.GetComponent<Transform>().Find("Follow Range").GetComponent<CircleCollider2D>();
				followRangeTrs = awakableEnemy.followRangeCollider.GetComponent<Transform>();
				makeCirclePolygonCollider.circle.center = (Vector2) followRangeTrs.position + awakableEnemy.followRangeCollider.offset.Multiply(followRangeTrs.lossyScale);
				makeCirclePolygonCollider.circle.radius = awakableEnemy.followRangeCollider.radius * Mathf.Abs(followRangeTrs.lossyScale.x);
				makeCirclePolygonCollider.pointCount = pointsPerPolygonCollider;
				makeCirclePolygonCollider.polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
				makeCirclePolygonCollider.Do ();
				makeCirclePolygonCollider.polygonCollider.usedByComposite = true;
				makeCirclePolygonColliders = makeCirclePolygonColliders.Add(makeCirclePolygonCollider);
			}
			DestroyImmediate(GetComponent<PolygonCollider2D>());
			compositeCollider.GenerateGeometry();
			moneyText.text = "" + money;
		}

		public virtual void OnDestroy ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}
#endif
	}
}