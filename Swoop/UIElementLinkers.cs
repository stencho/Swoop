using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.UIElements.Linkers {
    internal class UIElementLinker {
        internal Dictionary<string, UIElement> linked_elements = new Dictionary<string, UIElement>();
        internal UIElementManager parent;

        public UIElementLinker(UIElementManager parent_manager) {
            this.parent = parent_manager;
        }

        internal virtual void link(UIElement element) {
            if (linked_elements.ContainsKey(element.name)) return;
            linked_elements.Add(element.name, element);
        }

        internal virtual void unlink(string name) {
            if (!linked_elements.ContainsKey(name)) return;
            linked_elements.Remove(name);
        }

        internal virtual void unlink(UIElement element) {
            if (!linked_elements.ContainsKey(element.name)) return;
            linked_elements.Remove(element.name);
        }
    }

}
