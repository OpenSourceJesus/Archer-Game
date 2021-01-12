using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
	public static class TransformExtensions
	{
		public static Transform FindChild (this Transform trs, string childName)
		{
			List<Transform> remainingChildren = new List<Transform>();
			remainingChildren.Add(trs);
			while (remainingChildren.Count > 0)
			{
				foreach (Transform child in remainingChildren[0])
				{
					if (child.name.Equals(childName))
						return child;
					remainingChildren.Add(child);
				}
				remainingChildren.RemoveAt(0);
			}
			return null;
		}
		public static Transform[] FindChildren (this Transform trs, string childName)
		{
			List<Transform> output = new List<Transform>();
			List<Transform> remainingChildren = new List<Transform>();
			remainingChildren.Add(trs);
			while (remainingChildren.Count > 0)
			{
				foreach (Transform child in remainingChildren[0])
				{
					if (child.name.Equals(childName))
						output.Add(child);
					remainingChildren.Add(child);
				}
				remainingChildren.RemoveAt(0);
			}
			return output.ToArray();
		}

		public static Transform GetClosestTransform_2D (this Transform closestTo, Transform[] transforms)
		{
			// while (transforms.Contains(null))
			// 	transforms = transforms.Remove(null);
			if (transforms.Length == 0)
				return null;
			else if (transforms.Length == 1)
				return transforms[0];
			int closestOpponentIndex = 0;
			for (int i = 1; i < transforms.Length; i ++)
			{
				Transform checkOpponent = transforms[i];
				Transform currentClosestOpponent = transforms[closestOpponentIndex];
				if (((Vector2) (closestTo.position - checkOpponent.position)).sqrMagnitude < ((Vector2) (closestTo.position - currentClosestOpponent.position)).sqrMagnitude)
					closestOpponentIndex = i;
			}
			return transforms[closestOpponentIndex];
		}

		public static Transform GetClosestTransform_2D (Transform[] transforms, Vector2 position)
		{
			// while (transforms.Contains(null))
			// 	transforms = transforms.Remove(null);
			if (transforms.Length == 0)
				return null;
			else if (transforms.Length == 1)
				return transforms[0];
			int closestOpponentIndex = 0;
			for (int i = 1; i < transforms.Length; i ++)
			{
				Transform checkOpponent = transforms[i];
				Transform currentClosestOpponent = transforms[closestOpponentIndex];
				if ((position - (Vector2) checkOpponent.position).sqrMagnitude < (position - (Vector2) currentClosestOpponent.position).sqrMagnitude)
					closestOpponentIndex = i;
			}
			return transforms[closestOpponentIndex];
		}

		public static Rect GetUnrotatedRect (this Transform trs)
		{
			return Rect.MinMaxRect(trs.position.x - trs.lossyScale.x, trs.position.y - trs.lossyScale.y, trs.position.x + trs.lossyScale.x, trs.position.y + trs.lossyScale.y);
		}

		public static bool IsSameOrientationAndScale (this Transform trs, Transform other)
		{
			return trs.position == other.position && trs.rotation == other.rotation && trs.lossyScale == other.lossyScale;
		}

		public static Transform FindEquivalentChild (Transform root1, Transform child1, Transform root2)
		{
			TreeNode<Transform> childTree1 = root1.GetChildTree();
			int[] pathToChild1 = childTree1.GetPathToChild(child1);
			TreeNode<Transform> childTree2 = root2.GetChildTree().GetChildAtPath(pathToChild1);
			return childTree2.Value;
		}

		public static TreeNode<Transform> GetChildTree (this Transform root)
		{
			TreeNode<Transform> output = new TreeNode<Transform>(root);
			List<Transform> remainingChildren = new List<Transform>();
			remainingChildren.Add(root);
			Transform currentTrs;
			while (remainingChildren.Count > 0)
			{
				currentTrs = remainingChildren[0];
				foreach (Transform child in currentTrs)
				{
					output.GetRoot().GetChild(currentTrs).AddChild(child);
					remainingChildren.Add(child);
				}
				remainingChildren.RemoveAt(0);
			}
			return output;
		}

		public static void SetWorldScale (this Transform trs, Vector3 scale)
		{
			trs.localScale = trs.rotation * trs.InverseTransformDirection(scale);
		}
	}
}