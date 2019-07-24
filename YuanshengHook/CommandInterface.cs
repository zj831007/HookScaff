using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YuanshengHook
{
    [Serializable]
    public delegate void DecryptABEvent();

    public class CommandInterface : MarshalByRefObject
    {
        public event DecryptABEvent DecryptEvent;

        public void DoDecryptAB()
        {
            DecryptEvent();
        }
    }
}
