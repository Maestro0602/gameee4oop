using System;

using System;

namespace Team2.SharedUtils
{

    [Serializable]
    public class OverrideMaskValue<T> : OverrideMaskValueBase where T : Enum
    {

        public bool IsEnabled;


        public T Value;
    }
}
