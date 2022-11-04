using System.Numerics;

namespace SilkNet.Rendering;

public struct Frustum
{
    public Plane Near { get; private set; }
    public Plane Far { get; private set; }
    public Plane Left { get; private set; }
    public Plane Right { get; private set; }
    public Plane Top { get; private set; }
    public Plane Bottom { get; private set; }
    
    public Vector3[] Corners { get; private set; } = new Vector3[8];

    public Frustum(Camera camera, float fovY, float zNear, float zFar) : this()
    {
        Recalculate(camera, fovY, zNear, zFar);
    }

    public void Recalculate(Camera camera, float fovY, float zNear, float zFar)
    {
        float halfVerticalSize = zFar * (float)Math.Tan(fovY / 2);
        float halfHorizontalSize = halfVerticalSize * camera.AspectRatio;
        Vector3 frontMultFar = zFar * camera.Forward;
        
        Near = new Plane(camera.Position + zNear * camera.Forward, camera.Forward);
        Far = new Plane(camera.Position + frontMultFar, -camera.Forward);
        Right = new Plane(camera.Position, Vector3.Cross(camera.Up, frontMultFar + camera.Right * halfHorizontalSize));
        Left = new Plane(camera.Position, Vector3.Cross(frontMultFar - camera.Right * halfHorizontalSize, camera.Up));
        Bottom = new Plane(camera.Position, Vector3.Cross(camera.Right, frontMultFar - camera.Up * halfVerticalSize));
        Top = new Plane(camera.Position, Vector3.Cross(frontMultFar + camera.Up * halfVerticalSize, camera.Right));
        
        // Top Left Far
        Corners[0] = new Vector3(-halfHorizontalSize, halfVerticalSize, zFar);
        // Top Right Far
        Corners[1] = new Vector3(halfHorizontalSize, halfVerticalSize, zFar);
        // Bottom Left Far
        Corners[2] = new Vector3(-halfHorizontalSize, -halfVerticalSize, zFar);
        // Bottom Right Far
        Corners[3] = new Vector3(halfHorizontalSize, -halfVerticalSize, zFar);
        
        float halfNearHorizontalSize = zNear * (float)Math.Tan(fovY / 2) * camera.AspectRatio / 2f;
        
        // Top Left Near
        Corners[4] = new Vector3(-halfNearHorizontalSize, zNear * (float)Math.Tan(fovY / 2), zNear);
        // Top Right Near
        Corners[5] = new Vector3(halfNearHorizontalSize, zNear * (float)Math.Tan(fovY / 2), zNear);
        // Bottom Left Near
        Corners[6] = new Vector3(-halfNearHorizontalSize, -zNear * (float)Math.Tan(fovY / 2), zNear);
        // Bottom Right Near
        Corners[7] = new Vector3(halfNearHorizontalSize, -zNear * (float)Math.Tan(fovY / 2), zNear);
    }
}