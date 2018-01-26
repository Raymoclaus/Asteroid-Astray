using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Point : IShape
    {
        public Vector2 Position = Vector2.zero;
        public Vector2 RefCenter = Vector2.zero;
        private Transform _target;

        public Point(Vector2? p = null)
        {
            Position = p != null ? (Vector2) p : Position;
        }

        public bool IsPoint()
        {
            return true;
        }

        public bool IsLine()
        {
            return false;
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
                return Geometry2D.PointOnPoint(Position, ((Point) s).Position);
            }

            if (s.IsLine())
            {
                return Geometry2D.PointOnLineSeg(Position, (LineSeg) s);
            }

            if (s.IsCircle())
            {
                return Geometry2D.PointInCircle(Position, (Circle) s);
            }

            if (s.IsPoly())
            {
                return Geometry2D.PointInPoly(Position, (Poly) s);
            }

            return false;
        }

        public List<Vector2> GetVerts()
        {
            return new List<Vector2> {Position};
        }

        public List<Vector2> GetOffsetVerts()
        {
            return new List<Vector2> {Position + GetRefCenter()};
        }

        public Bounds GetBounds()
        {
            return new Bounds(Position, Vector2.zero);
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
            Position += move;
        }
    }
}