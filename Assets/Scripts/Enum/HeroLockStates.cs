using System;

namespace GlobalEnums
{
    // Token: 0x0200083B RID: 2107
    [Flags]
    public enum HeroLockStates
    {

        None = 0,

        AnimationLocked = 1,

        ControlLocked = 2,

        GravityLocked = 4,

        All = -1
    }
}
