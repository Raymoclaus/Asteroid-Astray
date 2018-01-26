using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public interface IShape
    {
        bool IsPoint();

        bool IsLine();

        bool IsCircle();

        bool IsPoly();

        bool Intersects(IShape s);

        List<Vector2> GetVerts();

        List<Vector2> GetOffsetVerts();

        Bounds GetBounds();

        void AttachToTransform(Transform t);

        Vector2 GetRefCenter();

        void Translate(Vector2 move);
    }
}