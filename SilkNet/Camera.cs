using System.Numerics;

namespace SilkNet;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Front { get; private set; }
    
    public Vector3 Up { get; private set; }
    public float AspectRatio { get; set; }

    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; }

    private float _zoom = 45f;
    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(Zoom - value, 1f, 45f);
    }

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    public Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Zoom), AspectRatio, 0.1f, 100f);
    
    public Camera(Vector3 position, Vector3 front, Vector3 up, float aspectRatio)
    {
        Position = position;
        Front = front;
        Up = up;
        AspectRatio = aspectRatio;
    }

    public void SetDirection(float xOffset, float yOffset)
    {
        Yaw += xOffset;
        Pitch -= yOffset;

        Pitch = Math.Clamp(Pitch, -89f, 89f);

        var cameraDirection = Vector3.Zero;
        cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)));
        cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
        cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)));

        Front = Vector3.Normalize(cameraDirection);
    }
}