using UnityEngine;

// Note: This requires the HutongGames PlayMaker asset to compile.
// If you do not have PlayMaker, comment out this entire file.

#if PLAYMAKER
using HutongGames.PlayMaker;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Custom Boss")]
    [Tooltip("Fires a Foxball projectile.")]
    public class FireFoxBall : FsmStateAction
    {
        [RequiredField]
        public FsmGameObject foxballPrefab;
        
        public FsmGameObject spawnPoint;
        public FsmFloat force;

        public override void Reset()
        {
            foxballPrefab = null;
            spawnPoint = null;
            force = 15f;
        }

        public override void OnEnter()
        {
            if (foxballPrefab.Value != null)
            {
                Vector3 spawnPos = spawnPoint.Value != null ? spawnPoint.Value.transform.position : Owner.transform.position;
                GameObject go = Object.Instantiate(foxballPrefab.Value, spawnPos, Quaternion.identity);
                
                FoxballControl control = go.GetComponent<FoxballControl>();
                if (control != null)
                {
                    control.force = force.Value;
                }
            }
            Finish();
        }
    }
}
#endif
