using UnityEngine;
using Extensions;
using ArcherGame;

public class ImageEffectApplier<TImageEffect> : MonoBehaviour, IUpdatable where TImageEffect : ImageEffect
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public float[] applyAmounts = new float[0]; 
	public TImageEffect imageEffect;

	public virtual void OnEnable ()
	{
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public virtual bool SetApplyAmounts ()
	{
		for (int i = 0; i < applyAmounts.Length; i ++)
		{
			float applyAmount = applyAmounts[i];
			if (applyAmount > 0)
				return true;
		}
		return false;
	}

	public virtual void DoUpdate ()
	{
		Texture2D image = imageEffect.image;
		Color[] pixelColors = image.GetPixels();
		for (int x = 0; x < image.width; x ++)
		{
			for (int y = 0; y < image.height; y ++)
			{
				imageEffect.applyAmount = applyAmounts[x + y * image.width];
				imageEffect.Apply (new Vector2Int(x, y), ref pixelColors);
			}
		}
		image.SetPixels(pixelColors);
		image.Apply();
	}

	public virtual void OnDisable ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}
}