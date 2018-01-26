using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Circle : IShape
    {
        private float _radius;
        [HideInInspector] public Vector2 RefCenter = Vector2.zero;
	    public Vector2 Center = Vector2.zero;

        public Vector2 WorldCenter
        {
            get { return GetRefCenter() + Center; }
            set { Center = value - GetRefCenter(); }
        }

        public Bounds Bounds;
        private Transform _target;

        public Circle(Transform target = null, Vector2? center = null, float radius = 1f)
        {
            _target = target;
            Center = center ?? Center;
            _radius = radius;
            CalculateBounds();
        }

        public bool IsPoint()
        {
            return false;
        }

        public bool IsLine()
        {
            return false;
        }

        public bool IsCircle()
        {
            return true;
        }

        public bool IsPoly()
        {
            return false;
        }

        public bool Intersects(IShape s)
        {
            if (s.IsPoint())
            {
                return Geometry2D.PointInCircle(((Point) s).Position, this);
            }

            if (s.IsLine())
            {
                return Geometry2D.LineSegIntersectsCircle((LineSeg) s, this);
            }

            if (s.IsCircle())
            {
                return Geometry2D.CircleIntersectsCircle((Circle) s, this);
            }

            if (s.IsPoly())
            {
                return Geometry2D.CircleIntersectsPoly(this, (Poly) s);
            }

            return false;
        }

        public List<Vector2> GetVerts()
        {
            return new List<Vector2> {Center};
        }

        public List<Vector2> GetOffsetVerts()
        {
            return new List<Vector2> {WorldCenter};
        }

        public Bounds GetBounds()
        {
            return new Bounds((Vector2) Bounds.center + GetRefCenter(), Bounds.size);
        }

        private void CalculateBounds()
        {
            Bounds = new Bounds(GetRefCenter(), Vector2.one * 2 * _radius);
        }

        public float GetRadius()
        {
            return _radius;
        }

        public void SetRadius(float change)
        {
            _radius = change;
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
            Center += move;
            Bounds.center = (Vector2)Bounds.center + move;
        }
    }
}