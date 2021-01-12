using UnityEngine;
using System.Collections;

namespace ArcherGame
{
	public class ElectricMistTile : MonoBehaviour
	{
		public float onDelay;
		public float onDuration;
		float onFraction;
		public Hazard hazard;

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			StopCoroutine(TurnOffRoutine ());
			StopCoroutine(TurnOnRoutine ());
			StartCoroutine(TurnOnRoutine ());
		}

		public virtual IEnumerator TurnOnRoutine ()
		{
			float startTime = Time.time;
			do
			{
				onFraction = (Time.time - startTime) / onDelay;
				if (onFraction >= 1)
					break;
				yield return new WaitForEndOfFrame();
			} while (true);
			onFraction = 1;
			TurnOn ();
		}

		public virtual IEnumerator TurnOffRoutine ()
		{
			float startTime = Time.time;
			do
			{
				onFraction = onDelay - (Time.time - startTime) / onDelay;
				if (onFraction <= 0)
					break;
				yield return new WaitForEndOfFrame();
			} while (true);
			onFraction = 0;
			TurnOff ();
		}

		public virtual void TurnOn ()
		{
			hazard.enabled = true;
			StartCoroutine(TurnOffRoutine ());
		}

		public virtual void TurnOff ()
		{
			hazard.enabled = false;
		}
	}
}