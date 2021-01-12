using UnityEngine;
using System.Collections.Generic;
using Extensions;

public class MoveLineSegmentImageEffectsOverImage<TImageEffect> : ApplyImageEffectOverLineSegmentWithWidth<TImageEffect> where TImageEffect : ImageEffect
{
	public List<LineSegment2D> lineSegments = new List<LineSegment2D>();
	public Timer spawnTimer;
	public float lineSegmentLength;
	public float lineSegmentWidth;
	public float moveSpeed;
	public CombineMode combineMode;

	public override void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		base.OnEnable ();
		spawnTimer.onFinished += SpawnLineSegmentFilter;
		spawnTimer.Reset ();
		spawnTimer.Start ();
	}

	public override void DoUpdate ()
	{
		SetApplyAmounts ();
		base.DoUpdate ();
	}

	public override bool SetApplyAmounts ()
	{
		bool output = false;
		Texture2D image = imageEffect.image;
		float[] _applyAmounts = new float[image.width * image.height];
		for (int i = 0; i < lineSegments.Count; i ++)
		{
			lineSegment = lineSegments[i];
			lineSegment = lineSegment.Move(lineSegment.GetDirection() * moveSpeed * Time.deltaTime);
			lineSegments[i] = lineSegment;
			output |= base.SetApplyAmounts ();
			for (int x = 0; x < image.width; x ++)
			{
				for (int y = 0; y < image.height; y ++)
				{
					float applyAmount = applyAmounts[x + y * image.width];
					if (applyAmount > 0)
					{
						if (combineMode == CombineMode.Min)
							_applyAmounts[x + y * image.width] = Mathf.Min(_applyAmounts[x + y * image.width], applyAmount);
						else if (combineMode == CombineMode.Max)
							_applyAmounts[x + y * image.width] = Mathf.Max(_applyAmounts[x + y * image.width], applyAmount);
						else if (combineMode == CombineMode.Add)
							_applyAmounts[x + y * image.width] += applyAmount;
						else
							_applyAmounts[x + y * image.width] = applyAmount;
					}
				}
			}
		}
		applyAmounts = _applyAmounts;
		return output;
	}

	public virtual void SpawnLineSegmentFilter (params object[] args)
	{
		Texture2D image = imageEffect.image;
		RectInt imageRect = new RectInt(0, 0, image.width, image.height);
		Vector2 startPosition = imageRect.ToRect().RandomPointOnPerimeter();
		LineSegment2D lineSegment = new LineSegment2D(startPosition + (startPosition - imageRect.center).normalized * lineSegmentLength, startPosition);
		lineSegments.Add(lineSegment);
	}

	public override void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		base.OnDisable ();
		spawnTimer.Stop ();
		spawnTimer.onFinished -= SpawnLineSegmentFilter;
	}

	public enum CombineMode
	{
		Min,
		Max,
		Add,
		Override
	}
}