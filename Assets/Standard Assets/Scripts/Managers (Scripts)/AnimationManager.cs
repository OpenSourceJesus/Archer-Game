using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using DialogAndStory;
using ArcherGame;

//[ExecuteInEditMode]
public class AnimationManager : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public _Animation[] animations;
	public Dictionary<string, _Animation> animationsDict = new Dictionary<string, _Animation>();
	public _Animation CurrentAnim
	{
		get
		{
			return animations[currentAnimEntryIndex];
		}
	}
	public int currentAnimEntryIndex;
	public Transform trs;
	public int recordStartFrame;
	[HideInInspector]
	public int startFrameForPreviousRecord;
	[HideInInspector]
	public bool record;
	public RecordSettings[] recordSettings;
	public int currentRecordSettingsIndex;
	public RecordSettings CurrentRecordSettings
	{
		get
		{
			return recordSettings[currentRecordSettingsIndex];
		}
	}
	float recordTimer;
	public static AnimationManager[] chosenInstances = new AnimationManager[0];
	public AnimationOperation animOperation;
	
	public virtual void OnEnable ()
	{
		animationsDict.Clear();
		foreach (_Animation animation in animations)
			animationsDict.Add(animation.name, animation);
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

#if UNITY_EDITOR
	public virtual void Update ()
	{
		if (true)
			return;
		CurrentRecordSettings.Update (CurrentAnim);
		if (record)
		{
			recordTimer += Time.deltaTime;
			if (recordStartFrame >= CurrentRecordSettings.timeSpan.endFrame)
				StopRecording ();
			else if (recordTimer > 1f / CurrentAnim.framesPerSecond)
			{
				Record ();
				recordTimer -= 1f / CurrentAnim.framesPerSecond;
			}
		}
		if (!Application.isPlaying)
		{
			return;
		}
			for (int i = 0; i < animations.Length; i ++)
				animations[i].DoUpdate ();
			if (animOperation != null)
			{
				animOperation.Do ();
				animOperation = null;
			}
		}
#endif

        public virtual void DoUpdate ()
        {
        }

        // public virtual void DoUpdate ()
        // {
        // 	for (int i = 0; i < animations.Length; i ++)
        // 		animations[i].DoUpdate ();
        // }
        
        public virtual void Play (string animName)
        {
        	animationsDict[animName].StartPlayback ();
        }
        
        public virtual void Stop (string animName)
        {
        	animationsDict[animName].StopPlayback ();
        }
        
#if UNITY_EDITOR
        [MenuItem("Animation/Start Recording %#r")]
        public static void StartRecording ()
        {
        	foreach (AnimationManager animationManager in SelectionExtensions.GetSelected<AnimationManager>())
        	{
        		if (!animationManager.record)
        		{
        			if (animationManager.CurrentRecordSettings.timeSpan.startFrame != MathfExtensions.NULL_INT)
        				animationManager.recordStartFrame = animationManager.CurrentRecordSettings.timeSpan.startFrame;
        			else
        				animationManager.recordStartFrame = animationManager.startFrameForPreviousRecord;
        			animationManager.startFrameForPreviousRecord = animationManager.recordStartFrame;
        			animationManager.record = true;
        		}
        	}
        }
#endif
        
        public virtual void Record ()
        {
            KeyFrame currentFrame = CurrentAnim.defaultKeyFrame.Copy();
            currentFrame.position = CurrentAnim.trs.position;
            currentFrame.localPosition = CurrentAnim.trs.localPosition;
            currentFrame.rotation = CurrentAnim.trs.eulerAngles;
            currentFrame.localRotation = CurrentAnim.trs.localEulerAngles;
            currentFrame.localScale = CurrentAnim.trs.localScale;
            currentFrame.animation = CurrentAnim;
            if (CurrentRecordSettings.overwrite && recordStartFrame < CurrentAnim.keyFrames.Count)
                CurrentAnim.keyFrames[recordStartFrame] = currentFrame;
            else
                CurrentAnim.keyFrames.Insert(recordStartFrame, currentFrame);
            recordStartFrame ++;
        }
        
#if UNITY_EDITOR
        [MenuItem("Animation/Stop Recording %#&r")]
        public static void StopRecording ()
        {
            foreach (AnimationManager animationManager in SelectionExtensions.GetSelected<AnimationManager>())
            {
                if (animationManager.record)
                {
                    animationManager.record = false;
                    animationManager.recordStartFrame = animationManager.startFrameForPreviousRecord;
                }
            }
        }
        
        [MenuItem("Animation/Start Playback %#w")]
        public static void StartPlayback ()
        {
            foreach (AnimationManager animationManager in SelectionExtensions.GetSelected<AnimationManager>())
                animationManager.CurrentAnim.StartPlayback ();
        }
        
        [MenuItem("Animation/Stop Playback %#&w")]
        public static void StopPlayback ()
        {
            foreach (AnimationManager animationManager in SelectionExtensions.GetSelected<AnimationManager>())
                animationManager.CurrentAnim.StopPlayback ();
        }
        
        [MenuItem("Animation/Delete Recording %#d")]
        public static void DeleteRecording ()
        {
            foreach (AnimationManager animationManager in SelectionExtensions.GetSelected<AnimationManager>())
            {
                animationManager.CurrentAnim.Delete ();
                animationManager.recordStartFrame = 0;
            }
        }
        
        [MenuItem("Animation/Select Chosen Animation Manager %#&s")]
        public static void SelectChosenInstances ()
        {
            Selection.objects = chosenInstances;
        }

        [MenuItem("Animation/Chose Animation Managers %#&c")]
        public static void SetChosenInstances ()
        {
            chosenInstances = SelectionExtensions.GetSelected<AnimationManager>();
        }
#endif
	}

	[Serializable]
	public class RecordSettings
	{
		public string name;
		public _Animation._TimeSpan timeSpan;
		public bool overwrite;
		public StopAction[] stopActions;
		
		public virtual void Update (_Animation animation)
		{
			timeSpan.Update (animation);
		}
	}

	[Serializable]
	public class PlaybackSettings
	{
		public string name;
		public _Animation._TimeSpan timeSpan;
		public StopAction[] stopActions;
		
		public virtual void Update (_Animation animation)
		{
			timeSpan.Update (animation);
		}
	}

[Serializable]
public class StopAction
{
	public Type type;

	public enum Type
	{
		GoToAnimEnd,
		GoToAnimBeginning,
		GoToRecordStart,
		GoToTimeSpanStart,
		GoToTimeSpanEnd
	}
}
