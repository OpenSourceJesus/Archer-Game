using UnityEngine;
using Extensions;
using System;

namespace ArcherGame
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundEffect : Spawnable
	{
		public AudioSource audioSource;

		public virtual void OnDisable ()
		{
			AudioManager.soundEffects = AudioManager.soundEffects.Remove(this);
		}

		[Serializable]
		public struct Settings
		{
			public AudioClip clip;
			public float volume;
			public float pitch;

			public Settings (AudioClip clip) : this (clip, 1, 1)
			{
			}

			public Settings (AudioClip clip, float volume, float pitch)
			{
				this.clip = clip;
				this.volume = volume;
				this.pitch = pitch;
			}
		}
	}
}