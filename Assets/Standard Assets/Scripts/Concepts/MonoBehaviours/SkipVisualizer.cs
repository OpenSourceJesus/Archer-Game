using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

namespace ArcherGame
{
    public class SkipVisualizer : MonoBehaviour
    {
        public Transform trs;
        public LineRenderer lineRenderer;
        public SkipManager.Skip skip;

        public virtual void UseSkip ()
        {
            SkipManager.skipPoints --;
            SkipManager.skipsUsed = SkipManager.skipsUsed.Add(skip);
        }

        public virtual void GetBackSkip ()
        {
            SkipManager.skipPoints ++;
            SkipManager.skipsUsed = SkipManager.skipsUsed.Remove(skip);
        }

        public virtual void Make ()
        {
            if (skip.start != null)
                lineRenderer.SetPosition(0, skip.start.trs.position);
            else
                lineRenderer.SetPosition(0, Player.instance.trs.position);
            lineRenderer.SetPosition(1, skip.end.trs.position);
        }
    }
}