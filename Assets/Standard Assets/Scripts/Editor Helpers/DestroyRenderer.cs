using UnityEngine;

[ExecuteInEditMode]
public class DestroyRenderer : MonoBehaviour
{
	public new Renderer renderer;

#if UNITY_EDITOR
	void OnEnable ()
	{
		if (!Application.isPlaying)
		{
			if (renderer == null)
				renderer = GetComponent<Renderer>();
			return;
		}
	}
#endif

	void Start ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		Destroy(renderer);
		Destroy(this);
	}
}