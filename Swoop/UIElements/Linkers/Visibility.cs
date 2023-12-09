using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.UIElements.Linkers {
    internal class VisibilityLinker : UIElementLinker {
        public VisibilityLinker(UIElementManager parent_manager) : base(parent_manager) { }

        public void link(UIElement element) => base.link(element);
        public void unlink(UIElement element) => base.unlink(element);

        public void toggle_on() {
            foreach (string key in linked_elements.Keys) {
                linked_elements[key].visible = true;
            }
        }

        public void toggle_off() {
            foreach (string key in linked_elements.Keys) {
                linked_elements[key].visible = true;
            }
        }
    }
}
