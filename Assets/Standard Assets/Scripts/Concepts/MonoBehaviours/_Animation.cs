using System;
using System.Collections.Generic;
using UnityEngine;
using ArcherGame;
using DialogAndStory;
using Extensions;

public class _Animation : MonoBehaviour
{
	public Transform trs;
	public float framesPerSecond;
	public int currentFrameIndex;
	[HideInInspector]
	public bool playback;
	public KeyFrame defaultKeyFrame;
	public List<KeyFrame> keyFrames = new List<KeyFrame>();
	int frameAtPlaybackStart;
	float playbackTimer;
	public int FrameCount
	{
		get
		{
			return keyFrames.Count;
		}
	}
	public float Duration
	{
		get
		{
			return 1f / framesPerSecond * FrameCount;
		}
	}
	public KeyFrame CurrentFrame
	{
		get
		{
			return keyFrames[currentFrameIndex];
		}
	}
	
	public virtual void DoUpdate ()
	{
		if (playback)
		{
			playbackTimer += Time.deltaTime;
			if (playbackTimer > 1f / framesPerSecond)
			{
				Playback ();
				playbackTimer -= 1f / framesPerSecond;
			}
		}
	}
	
	public virtual void StartPlayback ()
	{
		if (playback)
			return;
		playback = true;
		frameAtPlaybackStart = currentFrameIndex;
	}
	
	public virtual void Playback ()
	{
		if (CurrentFrame.usePosition)
			trs.position = CurrentFrame.position;
		if (CurrentFrame.useLocalPosition)
			trs.localPosition = CurrentFrame.localPosition;
		if (CurrentFrame.useRotation)
			trs.eulerAngles = CurrentFrame.rotation;
		if (CurrentFrame.useLocalRotation)
			trs.localEulerAngles = CurrentFrame.localRotation;
		if (CurrentFrame.useLocalScale)
			trs.localScale = CurrentFrame.localScale;
		if (CurrentFrame.useStartDialog)
			DialogManager.Instance.StartDialog (CurrentFrame.startDialog);
		if (CurrentFrame.useActivateButton)
			CurrentFrame.activateButton.onClick.Invoke();
		currentFrameIndex ++;
	}
	
	public virtual void StopPlayback ()
	{
		if (!playback)
			return;
		playback = false;
		currentFrameIndex = frameAtPlaybackStart;
	}
	
	public virtual void Delete ()
	{
		StopPlayback ();
		keyFrames.Clear();
		currentFrameIndex = 0;
	}

#if UNITY_EDITOR
	public virtual void Show ()
	{
		foreach (KeyFrame keyFrame in keyFrames)
			keyFrame.Show ();
	}
#endif

	[Serializable]
	public class _TimeSpan
	{
		public float startTime = 0;
		public float endTime = 0;
		public float duration;
		public int startFrame;
		public int endFrame;
		public int frameCount;
		public Type timeSpanType;
		
		public virtual void Update (_Animation animation)
		{
			switch (timeSpanType)
			{
				case Type.AnimStartAndEnd:
					startTime = 0;
					endTime = startTime + animation.Duration;
					duration = animation.Duration;
					startFrame = Mathf.RoundToInt(startTime / (1f / animation.framesPerSecond));
					endFrame = Mathf.RoundToInt(endTime  / (1f / animation.framesPerSecond));
					frameCount = endFrame - startFrame;
					break;
				case Type.AnimStartAndEndFrame:
					startFrame = Mathf.Clamp(startFrame, 0, startFrame + animation.FrameCount);
					startTime = startFrame * (1f / animation.framesPerSecond);
					endFrame = Mathf.Clamp(endFrame, startFrame, startFrame + animation.FrameCount);
					frameCount = animation.FrameCount;
					endTime = startTime + animation.Duration;
					duration = 1f / animation.framesPerSecond * frameCount;
					break;
				case Type.AnimStartAndEndTime:
					startTime = MathfExtensions.SnapToInterval(startTime, 1f / animation.framesPerSecond);
					startTime = Mathf.Clamp(startTime, 0, animation.Duration);
					endTime = MathfExtensions.SnapToInterval(endTime, 1f / animation.framesPerSecond);
					endTime = Mathf.Clamp(endTime, startTime, startTime + animation.Duration);
					startFrame = Mathf.RoundToInt(startTime / (1f / animation.framesPerSecond));
					startFrame = Mathf.Clamp(startFrame, 0, int.MaxValue);
					endFrame = Mathf.RoundToInt(endTime  / (1f / animation.framesPerSecond));
					endFrame = Mathf.Clamp(endFrame, 0, int.MaxValue);
					frameCount = endFrame - startFrame;
					duration = endTime - startTime;
					break;
				case Type.AnimStartFrameAndCount:
					startFrame = Mathf.Clamp(startFrame, 0, animation.FrameCount);
					frameCount = Mathf.Clamp(frameCount, 0, animation.FrameCount);
					startTime = 1f / animation.framesPerSecond * startFrame;
					endFrame = startFrame + frameCount;
					endTime = startTime + animation.Duration;
					duration = 1f / animation.framesPerSecond * frameCount;
					break;
				case Type.AnimStartTimeAndDuration:
					startTime = MathfExtensions.SnapToInterval(startTime, 1f / animation.framesPerSecond);
					startTime = Mathf.Clamp(startTime, 0, startTime + animation.Duration);
					duration = Mathf.Clamp(duration, 0, animation.Duration);
					endTime = startTime + duration;
					startFrame = Mathf.RoundToInt(startTime / (1f / animation.framesPerSecond));
					endFrame = Mathf.RoundToInt(endTime  / (1f / animation.framesPerSecond));
					frameCount = endFrame - startFrame;
					break;
				case Type.FrameCount:
					startTime = MathfExtensions.NULL_FLOAT;
					endTime = MathfExtensions.NULL_FLOAT;
					duration = MathfExtensions.NULL_FLOAT;
					startFrame = MathfExtensions.NULL_INT;
					endFrame = MathfExtensions.NULL_INT;
					break;
				case Type.StartAndEndTime:
					startTime = Mathf.Clamp(startTime, 0, Mathf.Infinity);
					endTime = Mathf.Clamp(endTime, startTime, Mathf.Infinity);
					startFrame = MathfExtensions.NULL_INT;
					endFrame = MathfExtensions.NULL_INT;
					frameCount = MathfExtensions.NULL_INT;
					duration = endTime - startTime;
					break;
				case Type.StartTimeAndDuration:
					startTime = Mathf.Clamp(startTime, 0, Mathf.Infinity);
					duration = Mathf.Clamp(duration, 0, Mathf.Infinity);
					endTime = startTime + duration;
					startFrame = MathfExtensions.NULL_INT;
					endFrame = MathfExtensions.NULL_INT;
					frameCount = MathfExtensions.NULL_INT;
					break;
				case Type.Infinite:
					duration = Mathf.Infinity;
					startTime = MathfExtensions.NULL_FLOAT;
					endTime = MathfExtensions.NULL_FLOAT;
					startFrame = MathfExtensions.NULL_INT;
					endFrame = MathfExtensions.NULL_INT;
					frameCount = MathfExtensions.NULL_INT;
					break;
			}
		}
		
		public enum Type
		{
			AnimStartAndEnd,
			AnimStartAndEndFrame,
			AnimStartAndEndTime,
			AnimStartFrameAndCount,
			AnimStartTimeAndDuration,
			FrameCount,
			StartAndEndTime,
			StartTimeAndDuration,
			Infinite
		}
	}
}