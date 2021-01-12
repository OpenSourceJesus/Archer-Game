using UnityEngine;

//[ExecuteInEditMode]
public class PlayAnimation : MonoBehaviour
{
	public AnimationManager animationManager;
	public string animationName;

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		animationManager.Play (animationName);
	}

	public virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		animationManager.Stop (animationName);
	}
}