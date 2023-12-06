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

        public virtual void link(UIElement element) {
            if (linked_elements.ContainsKey(element.name)) return;
            linked_elements.Add(element.name, element);
        }

        public virtual void unlink(string name) {
            if (!linked_elements.ContainsKey(name)) return;
            linked_elements.Remove(name);
        }

        public virtual void unlink(UIElement element) {
            if (!linked_elements.ContainsKey(element.name)) return;
            linked_elements.Remove(element.name);
        }
    }

    internal class VisibilityLinker : UIElementLinker {
        public VisibilityLinker(UIElementManager parent_manager) : base(parent_manager) {}

        void toggle_on() {
            foreach (string key in linked_elements.Keys) {
                linked_elements[key].visible = true;
            }
        }

        void toggle_off() {
            foreach (string key in linked_elements.Keys) {
                linked_elements[key].visible = true;
            }
        }
    }
}
