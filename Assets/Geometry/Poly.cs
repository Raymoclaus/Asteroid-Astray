using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Poly : IShape
    {
        private List<Vector2> _verts;
        public Vector2 RefCenter = Vector2.zero;
        public Bounds Bounds;
        private Transform _target;

        public int VertsCount
        {
            get { return _verts.Count; }
        }

        public Poly(List<Vector2> vertArr = null, Transform target = null)
        {
            _verts = vertArr != null ? vertArr : new List<Vector2>(3);
            while (_verts.Count < 3)
            {
                _verts.Add(Vector2.zero);
            }

            _target = target;
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
            return false;
        }

        public bool IsPoly()
        {
            return true;
        }

        public bool Intersects(IShape s)
        {
            if (s.IsPoint())
            {
                return Geometry2D.PointInPoly(((Point) s).Position, this);
            }

            if (s.IsLine())
            {
                return Geometry2D.LineSegIntersectsPoly((LineSeg) s, GetVerts());
            }

            if (s.IsCircle())
            {
                return Geometry2D.CircleIntersectsPoly((Circle) s, GetVerts());
            }

            if (s.IsPoly())
            {
                return Geometry2D.PolyIntersectsPoly(this, (Poly) s);
            }

            return false;
        }

        /// Returns the list of vertices of the polygon relative to its center.
        /// Changes to this list will affect the polygon because it is not a copy.
        public List<Vector2> GetVerts()
        {
            return _verts;
        }

        /// Returns a copy of the list of vertices of the polygon offset by its center and the given offset.
        public List<Vector2> GetOffsetVerts()
        {
            List<Vector2> offsetVerts = new List<Vector2>(_verts);
            for (int i = 0; i < _verts.Count; i++)
            {
                offsetVerts[i] += GetRefCenter();
                if (_target != null && !Mathf.Approximately(_target.eulerAngles.z, 0f))
                {
                    offsetVerts[i] = RotatePointAroundPivot(offsetVerts[i], _target.eulerAngles.z, _target.position);
                }
            }

            return offsetVerts;
        }

        private Vector2 RotatePointAroundPivot(Vector2 p, float angle, Vector2? pivot = null)
        {
            Vector2 pvot = pivot != null ? (Vector2) pivot : Vector2.zero;
            angle *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle) * (p.x - pvot.x) - Mathf.Sin(angle) * (p.y - pvot.y) + pvot.x,
                Mathf.Sin(angle) * (p.x - pvot.x) + Mathf.Cos(angle) * (p.y - pvot.y) + pvot.y);
        }

        /// Returns the bounds of the polygon in world space
        public Bounds GetBounds()
        {
            return new Bounds((Vector2) Bounds.center + GetRefCenter(), Bounds.size);
        }

        private void CalculateBounds()
        {
            Bounds = new Bounds(_verts[0], Vector2.zero);
            for (int i = 1; i < _verts.Count; i++)
            {
                Bounds.Encapsulate(_verts[i]);
            }
        }

        public void AddVertex(Vector2 vert)
        {
            _verts.Add(vert);
            Bounds.Encapsulate(vert);
        }

        public void SetVertexPosition(int vertId, Vector2 pos)
        {
            _verts[vertId] = pos;
            CalculateBounds();
        }

        public void SetVertexPosition(int vertId, float x, float y)
        {
            _verts[vertId] = new Vector2(x, y);
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
            for (int i = 0; i < _verts.Count; i++)
            {
                _verts[i] += move;
            }

            Bounds.center = (Vector2)Bounds.center + move;
        }
    }
}