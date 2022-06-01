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
        private readonly Form1 form;
        public Functions(Form1 form)
        {
            this.form = form;
        }


        [ScriptableMethod]
        public AsyncMethod Print(object message)
        {
            return new AsyncMethod().Do(() => form.Output.Text += message + "\r\n");
        }

        [ScriptableMethod]
        public AsyncFunction<float> Add(float x, float y)
        {
            return new AsyncFunction<float>().Return(() => x + y);
        }

        [ScriptableMethod]
        public AsyncFunction<string> Concat(string x, string y)
        {
            return new AsyncFunction<string>().Return(() => x + y);
        }

        [ScriptableMethod]
        public AsyncFunction<int> Length(string x)
        {
            return new AsyncFunction<int>().Return(() => x.Length);
        }

        [ScriptableEvent]
        public void OnButtonClicked()
        {
        }

        [ScriptableEvent]
        public void OnStart()
        {
        }

        [ScriptableEvent]
        public void OnTest()
        {
        }
    }
}
