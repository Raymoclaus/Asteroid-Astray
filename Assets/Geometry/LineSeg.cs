using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class LineSeg : IShape
    {
        private Vector2 _a, _b;
        public Vector2 RefCenter = Vector2.zero;
        public Bounds Bounds;
        private Transform _target;

        public Vector2 CenterA
        {
            get { return _a + GetRefCenter(); }
        }

        public Vector2 CenterB
        {
            get { return _b + GetRefCenter(); }
        }

        public float Length
        {
            get { return Vector2.Distance(_a, _b); }
        }

        public float Slope
        {
            get
            {
                if (_a.x == _b.x)
                {
                    return float.PositiveInfinity;
                }

                return (_b.y - _a.y) / (_b.x - _a.x);
            }
        }

        public float Offset
        {
            get
            {
                if (Slope == float.PositiveInfinity)
                {
                    return _a.x;
                }

                return _a.y - Slope * _a.x;
            }
        }

        public LineSeg()
        {
            _a = _b = Vector2.zero;
            CalculateBounds();
        }

        public LineSeg(Vector2 a, Vector2 b)
        {
            _a = a;
            _b = b;
            CalculateBounds();
        }

        public LineSeg(Vector2 center, Vector2 a, Vector2 b)
        {
            RefCenter = center;
            _a = a;
            _b = b;
            CalculateBounds();
        }

        public LineSeg(Transform target, Vector2 a, Vector2 b)
        {
            _target = target;
            _a = a;
            _b = b;
            CalculateBounds();
        }

        public bool IsPoint()
        {
            return false;
        }

        public bool IsLine()
        {
            return true;
        }

        public bool IsCircle()
        {
            return false;
        }

        public bool IsPoly()
        {
            return false;
        }

        public bool Intersects(IShape s)
        {
            if (s.IsPoint())
            {
                return Geometry2D.PointOnLineSeg(((Point) s).Position, this);
            }

            if (s.IsLine())
            {
                return Geometry2D.LineSegIntersectsLineSeg(this, (LineSeg) s);
            }

            if (s.IsCircle())
            {
                return Geometry2D.LineSegIntersectsCircle(this, (Circle) s);
            }

            if (s.IsPoly())
            {
                return Geometry2D.LineSegIntersectsPoly(this, (Poly) s);
            }

            return false;
        }

        public List<Vector2> GetVerts()
        {
            return new List<Vector2> {_a, _b};
        }

        public List<Vector2> GetOffsetVerts()
        {
            return new List<Vector2> {_a + GetRefCenter(), _b + GetRefCenter()};
        }

        public Bounds GetBounds()
        {
            return new Bounds((Vector2) Bounds.center + GetRefCenter(), Bounds.size);
        }

        private void CalculateBounds()
        {
            Bounds = new Bounds(_a, Vector2.zero);
            Bounds.Encapsulate(_b);
        }

        public Vector2 GetA()
        {
            return _a;
        }

        public Vector2 GetB()
        {
            return _b;
        }

        public void SetA(Vector2 change)
        {
            _a = change;
            CalculateBounds();
        }

        public void SetB(Vector2 change)
        {
            _b = change;
            CalculateBounds();
        }

        public void AttachToTransform(Transform t)
        {
            _target = t;
        }

        public Vector2 GetRefCenter()
        {
            return _target != null ? (Vector2) _target.position : RefCenter;
        }

        public void Translate(Vector2 move)
        {
            _a += move;
            _b += move;
            Bounds.center = (Vector2)Bounds.center + move;
        }

        public float GetRotation()
        {
            return _target != null ? _target.eulerAngles.z : 0f;
        }
    }
}