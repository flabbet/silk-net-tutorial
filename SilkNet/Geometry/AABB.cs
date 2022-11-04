﻿using System.Numerics;
using Plane = SilkNet.Rendering.Plane;

namespace SilkNet.Geometry;

public struct AABB
{
    public Vector3 Center { get; set; }
    public Vector3 Extents { get; set; }
    
    public AABB(Vector3 min, Vector3 max)
    {
        Center = (min + max) * 0.5f;
        Extents = new Vector3(max.X - Center.X, max.Y - Center.Y, max.Z - Center.Z);
    }

    public bool IsOnOrForwardPlane(Plane plane)
    {
        float r = Extents.X * Math.Abs(plane.Normal.X) + Extents.Y * Math.Abs(plane.Normal.Y) + Extents.Z * Math.Abs(plane.Normal.Z);
        return -r <= plane.GetSignedDistance(Center);
    }
}