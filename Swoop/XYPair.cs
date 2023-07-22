﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib {
    public static class BaseExt {
        public static XYPair ToXYPair(this Point p) => new XYPair(p.X, p.Y);
        public static XYPair ToXYPair(this Vector2 p) => new XYPair(p.X, p.Y);
    }

    //disable warnings for equals/hashcode overrides - works fine without
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

    //this is basically the same thing as a Vector but with ints instead as I need to keep track of 2 ints per grid cell (for easily storing and accessing cells- also helps makes it extremely easy to only have cells in the cell list dictionary that actually contain game objects, identified by their location. infinite world, no caveats, spawn objects anywhere and they should just work!)
    public struct XYPair {
        public int X;
        public int Y;

        public XYPair(int XY) {
            this.X = XY;
            this.Y = XY;
        }
        public XYPair(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }
        public XYPair(float X, float Y) {
            this.X = (int)X;
            this.Y = (int)Y;
        }

        public XYPair(Vector2 vector) {
            this.X = (int)vector.X;
            this.Y = (int)vector.Y;
        }

        public XYPair(Point point) {
            this.X = point.X;
            this.Y = point.Y;
        }

        public bool within_rect(XYPair min, XYPair max) 
            => (this.X >= min.X && this.X <= max.X && this.Y >= min.Y && this.Y <= max.Y);

        public bool within_rect(Vector2 min, Vector2 max)
            => (this.X >= min.X && this.X <= max.X && this.Y >= min.Y && this.Y <= max.Y);

        public bool within_rect(Point min, Point max)
            => (this.X >= min.X && this.X <= max.X && this.Y >= min.Y && this.Y <= max.Y);

        public XYPair X_only => new XYPair(this.X, 0);
        public XYPair Y_only => new XYPair(0, this.Y);

        public int Length() {
            var y2 = this.Y;
            var x2 = this.X;

            y2 = (int)Math.Pow((double)y2, 2.0);
            x2 = (int)Math.Pow((double)x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static int Length(XYPair a, XYPair b) {
            var y2 = b.Y - a.Y;
            var x2 = b.X - a.X;

            y2 = (int)Math.Pow((double)y2, 2.0);
            x2 = (int)Math.Pow((double)x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static int Length(XYPair a, Vector2 b) {
            var y2 = (int)b.Y - a.Y;
            var x2 = (int)b.X - a.X;

            y2 = (int)Math.Pow((double)y2, 2.0);
            x2 = (int)Math.Pow((double)x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static int Length(XYPair a, Vector3 b) {
            var y2 = (int)b.Y - a.Y;
            var x2 = (int)b.Z - a.X;

            y2 = (int)Math.Pow((double)y2, 2.0);
            x2 = (int)Math.Pow((double)x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static explicit operator XYPair(Vector2 v) {
            return new XYPair((int)v.X, (int)v.Y);
        }

        #region int
        public static XYPair operator -(XYPair a, int b) => new XYPair() { X = a.X - b, Y = a.Y - b };
        public static XYPair operator +(XYPair a, int b) => new XYPair() { X = a.X + b, Y = a.Y + b };
        public static XYPair operator *(XYPair a, int b) => new XYPair() { X = a.X * b, Y = a.Y * b };
        public static XYPair operator *(XYPair a, float b) => new XYPair() { X = (int)(a.X * b), Y = (int)(a.Y * b) };
        public static XYPair operator /(XYPair a, int b) => new XYPair() { X = a.X / b, Y = a.Y / b };

        #endregion
        #region XYPair
        public static XYPair operator -(XYPair a, XYPair b) => new XYPair() { X = a.X - b.X, Y = a.Y - b.Y };
        public static XYPair operator +(XYPair a, XYPair b) => new XYPair() { X = a.X + b.X, Y = a.Y + b.Y };
        public static XYPair operator *(XYPair a, XYPair b) => new XYPair() { X = a.X * b.X, Y = a.Y * b.Y };
        public static XYPair operator /(XYPair a, XYPair b) => new XYPair() { X = a.X / b.X, Y = a.Y / b.Y };

        #endregion
        #region float
        public static Vector2 operator *(float b, XYPair a) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator /(XYPair a, float b) => new Vector2(a.X / b, a.Y / b);
        public static Vector2 operator -(XYPair a, float b) => new Vector2(a.X - b, a.Y - b);
        public static Vector2 operator +(XYPair a, float b) => new Vector2(a.X + b, a.Y + b);

        #endregion
        #region Vector2
        public static Vector2 operator *(Vector2 b, XYPair a) => new Vector2(a.X * b.X, a.Y * b.Y);
        public static Vector2 operator /(Vector2 b, XYPair a) => new Vector2(a.X / b.X, a.Y / b.Y);
        public static Vector2 operator -(Vector2 b, XYPair a) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator +(Vector2 b, XYPair a) => new Vector2(a.X + b.X, a.Y + b.Y);

        public static XYPair operator *(XYPair a, Vector2 b) => new XYPair(a.X * (int)b.X, a.Y * (int)b.Y);
        public static XYPair operator /(XYPair a, Vector2 b) => new XYPair(a.X / (int)b.X, a.Y / (int)b.Y);
        public static XYPair operator -(XYPair a, Vector2 b) => new XYPair(a.X - (int)b.X, a.Y - (int)b.Y);
        public static XYPair operator +(XYPair a, Vector2 b) => new XYPair(a.X + (int)b.X, a.Y + (int)b.Y);

        public Vector3 ToVector3XZ() => new Vector3(X, 0, Y);
        public Vector3 ToVector3XY() => new Vector3(X, Y, 0);

        public Vector2 ToVector2() => new Vector2(X, Y);
        #endregion
        #region Point
        public static Point operator *(Point b, XYPair a) => new Point(a.X * b.X, a.Y * b.Y);
        public static Point operator /(Point b, XYPair a) => new Point(a.X / b.X, a.Y / b.Y);
        public static Point operator -(Point b, XYPair a) => new Point(a.X - b.X, a.Y - b.Y);
        public static Point operator +(Point b, XYPair a) => new Point(a.X + b.X, a.Y + b.Y);

        public static XYPair operator *(XYPair a, Point b) => new XYPair(a.X * (int)b.X, a.Y * (int)b.Y);
        public static XYPair operator /(XYPair a, Point b) => new XYPair(a.X / (int)b.X, a.Y / (int)b.Y);
        public static XYPair operator -(XYPair a, Point b) => new XYPair(a.X - (int)b.X, a.Y - (int)b.Y);
        public static XYPair operator +(XYPair a, Point b) => new XYPair(a.X + (int)b.X, a.Y + (int)b.Y);

        public Point ToPoint() => new Point(X, Y);
        #endregion

        public static bool operator ==(XYPair a, XYPair b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(XYPair a, XYPair b) => (a.X != b.X || a.Y != b.Y);

        public override string ToString() => string.Format("{{ {0} : {1} }}", X, Y);
        public string ToXString() => string.Format("{0}x{1}", X, Y);

        public static string simple_string(XYPair input) {
            return string.Format("{0}, {1}", input.X, input.Y);
        }
        public static string simple_string_brackets(XYPair input) {
            return string.Format("[{0}, {1}]", input.X, input.Y);
        }
        public static bool TryParse(string input, out XYPair result) {
            result = XYPair.Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 2) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y)) {
                return true;
            }
            result = XYPair.Zero;
            return false;
        }

        private static XYPair _one = new XYPair { X = 1, Y = 1 };
        private static XYPair _zero = new XYPair { X = 0, Y = 0 };

        private static XYPair _unitX = new XYPair { X = 1, Y = 0 };
        private static XYPair _unitY = new XYPair { X = 0, Y = 1 };

        public static XYPair UnitX => _unitX;
        public static XYPair UnitY => _unitY;

        public static XYPair Width => _unitX;
        public static XYPair Height => _unitY;

        public static XYPair One => _one;
        public static XYPair Zero => _zero;

        public static XYPair Up => _unitY;
        public static XYPair Down => _zero - _unitY;

        public static XYPair Right => _unitX;
        public static XYPair Left => _zero - _unitX;        
    }

#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)

}
