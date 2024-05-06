using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace SwoopLib {
    //this is probably a sin
    //3 args is enough

    public class MultiAction {
        private Dictionary<string, Action> actions = new Dictionary<string, Action>();

        public void invoke_all() {
            foreach (Action action in actions.Values) {
                action();
            }
        }

        public void register_action(string name, Action action) {
            if (!actions.ContainsKey(name))
                actions.Add(name, action);
        }
        public void unregister_action(string name) {
            if (actions.ContainsKey(name))
                actions.Remove(name);
        }
    }

    public class MultiAction<T> {
        private Dictionary<string, Action<T>> actions = new Dictionary<string, Action<T>>();

        public void invoke_all(T param) {
            foreach (Action<T> action in actions.Values) {
                action(param);
            }
        }

        public void register_action(string name, Action<T> action) {
            if (!actions.ContainsKey(name))
                actions.Add(name, action);
        }
        public void unregister_action(string name) {
            if (actions.ContainsKey(name)) 
                actions.Remove(name);
        }
    }

    public class MultiAction<T1, T2> {
        private Dictionary<string, Action<T1, T2>> actions = new Dictionary<string, Action<T1, T2>>();

        public void invoke_all(T1 arg1, T2 arg2) {
            foreach (Action<T1, T2> action in actions.Values) {
                action(arg1, arg2);
            }
        }

        public void register_action(string name, Action<T1, T2> action) {
            if (!actions.ContainsKey(name))
                actions.Add(name, action);
        }
        public void unregister_action(string name) {
            if (actions.ContainsKey(name))
                actions.Remove(name);
        }
    }

    public class MultiAction<T1, T2, T3> {
        private Dictionary<string, Action<T1, T2, T3>> actions = new Dictionary<string, Action<T1, T2, T3>>();

        public void invoke_all(T1 arg1, T2 arg2, T3 arg3) {
            foreach (Action<T1, T2, T3> action in actions.Values) {
                action(arg1, arg2, arg3);
            }
        }

        public void register_action(string name, Action<T1, T2, T3> action) {
            if (!actions.ContainsKey(name))
                actions.Add(name, action);
        }
        public void unregister_action(string name) {
            if (actions.ContainsKey(name))
                actions.Remove(name);
        }
    }
}
