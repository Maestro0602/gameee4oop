using UnityEngine;

// Note: This requires the HutongGames PlayMaker asset to compile.
// If you do not have PlayMaker, comment out this entire file.

#if PLAYMAKER
using HutongGames.PlayMaker;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Custom")]
    [Tooltip("Adds Currency to the PlayerData via CurrencyManager.")]
    public class AddCurrency : FsmStateAction
    {
        public FsmOwnerDefault Target;
        
        [ObjectType(typeof(CurrencyType))]
        public FsmEnum CurrencyType;
        
        public FsmInt Amount;

        public override void Reset()
        {
            CurrencyType = null;
            Amount = 1;
        }

        public override void OnEnter()
        {
            if (!this.CurrencyType.IsNone && this.Amount.Value > 0)
            {
                CurrencyManager.AddCurrency(this.Amount.Value, (CurrencyType)this.CurrencyType.Value, true);
            }
            Finish();
        }
    }
}
#endif
