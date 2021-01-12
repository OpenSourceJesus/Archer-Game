using UnityEngine;

namespace ArcherGame
{
    public class Boss1 : Boss
    {
        public override void Start ()
        {
            base.Start ();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
        }
    }
}