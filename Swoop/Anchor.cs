using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib {

    public class Anchor {
        [Flags] public enum AnchorTo {
            Right, 
            Bottom
        }

        public static class Manager {
            static List<Anchor> anchors = new List<Anchor>();

            internal static void update_positions(UIElementManager element_manager, XYPair old_resolution, XYPair new_resolution) {
                foreach (Anchor a in anchors) {
                    if (element_manager != a.parent_manager) continue;
                    XYPair res_change = new_resolution - old_resolution;

                    if (a.anchor.HasFlag(AnchorTo.Right)) 
                        a.parent_element.position_actual += res_change.X_only;
                    if (a.anchor.HasFlag(AnchorTo.Bottom))
                        a.parent_element.position_actual += res_change.Y_only;
                }
            }

            internal static void add(Anchor anchor) {
                if (anchors.Contains(anchor)) return;
                anchors.Add(anchor);
            }

            internal static void remove(Anchor anchor) { 
                if (anchors.Contains(anchor)) return;
                anchors.Remove(anchor);
            }
        }

        public AnchorTo anchor = AnchorTo.Right;

        UIElement parent_element;
        UIElementManager parent_manager;

        public Anchor(UIElement parent_element, AnchorTo anchor) {
            this.anchor = anchor;

            this.parent_element = parent_element;
            this.parent_manager = parent_element.parent;

            Manager.add(this);
        }
        ~Anchor() {
            Manager.remove(this);
        }

    }
}
