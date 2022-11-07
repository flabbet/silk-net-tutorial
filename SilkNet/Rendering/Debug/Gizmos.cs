using System.Numerics;
using SilkNet.Geometry;
using SilkNet.Geometry.Primitives;

namespace SilkNet.Rendering.Debug;

public static class Gizmos
{
    public static Line[] GetFrustumLines(Camera camera)
    {
        Frustum frustum = camera.Frustum;
        Vector3[] corners = frustum.Corners;

        return new[]
        {
            new Line(corners[0], corners[1], 1),
            new Line(corners[1], corners[2], 1),
            new Line(corners[2], corners[3], 1),
            new Line(corners[3], corners[0], 1),
            new Line(corners[4], corners[5], 1),
            new Line(corners[5], corners[6], 1),
            new Line(corners[6], corners[7], 1),
            new Line(corners[7], corners[4], 1),
            new Line(corners[0], corners[4], 1),
            new Line(corners[1], corners[5], 1),
            new Line(corners[2], corners[6], 1),
            new Line(corners[3], corners[7], 1)
        };
    }
}