using UnityEngine;

namespace ArcherGame
{
	public class DeathSound : SoundEffect
	{
		public AudioClip[] responseAudioClips = new AudioClip[0];
		public Transform killerTrs;

		public override void OnDisable ()
		{
			base.OnDisable ();
			if (killerTrs.GetComponentInChildren<SoundEffect>() == null)
			{
				Settings settings = new Settings(responseAudioClips[Random.Range(0, responseAudioClips.Length)]);
				SoundEffect responseSoundEffect = AudioManager.Instance.PlaySoundEffect(settings, killerTrs.position);
				responseSoundEffect.trs.SetParent(killerTrs);
			}
		}
	}
}