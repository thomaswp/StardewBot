using BlocklyBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocklyTest
{
    [ScriptableBehavior("Functions", 200)]
    public class Functions
    {

        [ScriptableMethod]
        public AsyncMethod Print(int message)
        {
            return new AsyncMethod().Do(() => Logger.Log(message));
        }

        [ScriptableMethod]
        public AsyncFunction<int> Add(int x, int y)
        {
            return new AsyncFunction<int>().Return(() => x + y);
        }

        [ScriptableEvent]
        public void OnButtonClicked()
        {
        }
    }
}
