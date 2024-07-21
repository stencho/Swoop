using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace SwoopLib {
    /// <summary>
    /// Instead of using normal min and max, we use A and B, which are agnostic of orientation
    /// when A or B are modified, the AABB takes note of which of them holds the min and max X/Y coords
    /// and uses that information for calculating actual min/max positions
    /// </summary>
    public struct AABB {
        XYPair _A; XYPair _B;

        enum AABB_P { A, B };

        public XYPair A { get { return _A; } set { _A = value; update_min_max(); } }
        public XYPair B { get { return _B; } set { _B = value; update_min_max(); } }

        AABB_P min_X, min_Y, max_X, max_Y;

        public int left => min_X == AABB_P.A ? A.X : B.X;
        public int right => max_X == AABB_P.A ? A.X : B.X;
        public int top => min_Y == AABB_P.A ? A.Y : B.Y;
        public int bottom => max_Y == AABB_P.A ? A.Y : B.Y;

        public XYPair top_left => new XYPair(left, top);
        public XYPair top_right => new XYPair(right, top);
        public XYPair bottom_left => new XYPair(left, bottom);
        public XYPair bottom_right => new XYPair(right, bottom);

        public XYPair min => top_left; public XYPair max => bottom_right;

        public bool intersects(AABB vs) {
            if ((right < vs.left || left > vs.right) || (bottom <  vs.top || top > vs.bottom)) return false;
            return true;
        }

        public static bool intersects(AABB A, AABB B) => A.intersects(B);

        void update_min_max() {
            if (A.X <= B.X) { min_X = AABB_P.A; max_X = AABB_P.B; } else { min_X = AABB_P.B; max_X = AABB_P.A; }
            if (A.Y <= B.Y) { min_Y = AABB_P.A; max_Y = AABB_P.B; } else { min_Y = AABB_P.B; max_Y = AABB_P.A; }
        }

        public AABB expand_to_include_AABB(AABB aabb) {
            AABB tmp = new AABB(A, B);
            tmp.expand_to_include_point_in_place(aabb.min);
            tmp.expand_to_include_point_in_place(aabb.max);
            return tmp;
        }

        public AABB expand_to_include_point(XYPair point) {
            XYPair a=A, b=B;

            if (point.X < left) {
                if (min_X == AABB_P.A) a.X = point.X;
                if (min_X == AABB_P.B) b.X = point.X;
            }
            if (point.X > right) {
                if (max_X == AABB_P.A) a.X = point.X;
                if (max_X == AABB_P.B) b.X = point.X;
            }

            if (point.Y < top) {
                if (min_Y == AABB_P.A) a.Y = point.Y;
                if (min_Y == AABB_P.B) b.Y = point.Y;
            }
            if (point.Y > bottom) {
                if (max_Y == AABB_P.A) a.Y = point.Y;
                if (max_Y == AABB_P.B) b.Y = point.Y;
            }

            return new AABB(a, b);
        }

        public void expand_to_include_AABB_in_place(AABB aabb) {
            expand_to_include_point_in_place(aabb.min);
            expand_to_include_point_in_place(aabb.max);
        }

        public void expand_to_include_point_in_place(XYPair point) {
            if (point.X < left) {
                if (min_X == AABB_P.A) _A.X = point.X;
                if (min_X == AABB_P.B) _B.X = point.X;
            }
            if (point.X > right) {
                if (max_X == AABB_P.A) _A.X = point.X;
                if (max_X == AABB_P.B) _B.X = point.X;
            }

            if (point.Y < top) {
                if (min_Y == AABB_P.A) _A.Y = point.Y;
                if (min_Y == AABB_P.B) _B.Y = point.Y;
            }
            if (point.Y > bottom) {
                if (max_Y == AABB_P.A) _A.Y = point.Y;
                if (max_Y == AABB_P.B) _B.Y = point.Y;
            }

            update_min_max();
        }

        public static AABB expand_to_include_point(AABB aabb, XYPair point) {
            AABB tmp_aabb = new AABB(aabb.A, aabb.B);
            tmp_aabb.expand_to_include_point(point);
            return tmp_aabb;
        }

        public AABB(XYPair A, XYPair B) { this.A = A; this.B = B; }

        public void draw(Color color) { Drawing.rect(min, max, color, 1f); }
        public void fill(Color color) { Drawing.fill_rect(min, max, color); }
        public void fill_with_outline(Color background, Color outline, int outline_size) {
            Drawing.fill_rect(min, max, background);
            Drawing.fill_rect(min + outline_size.ToXY(), max - outline_size.ToXY(), outline);
        }
    }
}
