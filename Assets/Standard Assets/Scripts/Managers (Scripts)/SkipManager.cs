using UnityEngine;
using ArcherGame;
using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcherGame
{
	public class SkipManager : SingletonMonoBehaviour<SkipManager>, ISaveableAndLoadable
	{
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		[SaveAndLoadValue(false)]
		public static int skipPoints;
		[SaveAndLoadValue(false)]
		public static Skip[] skipsUsed = new Skip[0];
		public Skip[] skips;
		public SkipVisualizer skipVisualizerPrefab;
		public SkipVisualizer[] skipVisualizers = new SkipVisualizer[0];
		public Transform skipVisualizersParent;
		
#if UNITY_EDITOR
		[MenuItem("Skips/Make Skips")]
#endif
		public static void _MakeSkips ()
		{
			SkipManager.Instance.MakeSkips ();
		}

#if UNITY_EDITOR
		[MenuItem("Skips/Destroy Skips")]
#endif
		public static void _DestroySkips ()
		{
			SkipManager.Instance.DestroySkips ();
		}

		public virtual void DestroySkips ()
		{
			skipVisualizers = skipVisualizersParent.GetComponentsInChildren<SkipVisualizer>();
			foreach (SkipVisualizer skipVisualizer in skipVisualizers)
				DestroyImmediate(skipVisualizer.gameObject);
			skipVisualizers = new SkipVisualizer[0];
		}

		public virtual void MakeSkips ()
		{
			DestroySkips ();
			bool containsSkipVisualizer = false;
			foreach (Skip skip in skips)
			{
				foreach (SkipVisualizer skipVisualizer in skipVisualizers)
				{
					if (skipVisualizer.skip == skip)
					{
						containsSkipVisualizer = true;
						break;
					}
				}
				if (!containsSkipVisualizer)
				{
					SkipVisualizer skipVisualizer = Instantiate(skipVisualizerPrefab, skipVisualizersParent);
					skipVisualizer.skip = skip;
					skipVisualizer.Make ();
					skipVisualizers = skipVisualizers.Add(skipVisualizer);
				}
			}
		}

		public virtual void ShowSkips ()
		{
			foreach (SkipVisualizer skipVisualizer in skipVisualizers)
				skipVisualizer.gameObject.SetActive(true);
		}

		public virtual void HideSkips ()
		{
			foreach (SkipVisualizer skipVisualizer in skipVisualizers)
				skipVisualizer.gameObject.SetActive(false);
		}

		public virtual void UseSkip (Skip skip)
		{
			if (skipPoints >= skip.skipCost && skip.start.found && (skip.requirement == null || skip.requirement.collectedAndSaved))
			{
				skipPoints -= skip.skipCost;
				skip.end.found = true;
			}
		}

		public virtual void RedeemSkip (Skip skip)
		{
			if (!skipsUsed.Contains(skip))
				return;
			skipPoints += skip.skipCost;
			skipsUsed = skipsUsed.Remove(skip);
		}

		[Serializable]
		public class Skip
		{
			public Obelisk start;
			public Obelisk end;
			public int skipCost = 1;
			public ArrowCollectible requirement;

			public Skip (Obelisk start, Obelisk end, int skipCost = 1, ArrowCollectible requirement = null)
			{
				this.start = start;
				this.end = end;
				this.skipCost = skipCost;
				if (requirement != null)
					this.requirement = requirement;
			}
		}
	}
}