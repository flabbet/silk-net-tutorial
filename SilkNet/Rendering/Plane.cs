﻿using System.Numerics;

namespace SilkNet.Rendering;

public struct Plane
{
    public Vector3 Normal { get; set; }
    public Vector3 Point { get; set; }
    public float Distance { get; set; }
    
    public float GetSignedDistance(Vector3 point)
    {
        return Vector3.Dot(Normal, point) - Distance;
    }

    public Plane(Vector3 point, Vector3 normal)
    {
        Normal = Vector3.Normalize(normal);
        Distance = Vector3.Dot(normal, point);
        Point = point;
    }
}