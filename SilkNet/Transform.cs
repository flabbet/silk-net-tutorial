using System.Numerics;

namespace SilkNet;

public class Transform
{
    private Vector3 _position = Vector3.Zero;
    private float _scale = 1f;
    private Quaternion _rotation = Quaternion.Identity;
    private Matrix4x4 _cachedMatrix = Matrix4x4.Identity;
    
    private bool _isDirty = true;
    
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _isDirty = true;
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _isDirty = true;
        }
    }
    
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _isDirty = true;
        }
    }
    
    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (_isDirty)
            {
                _cachedMatrix = Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) *
                       Matrix4x4.CreateTranslation(Position);
                _isDirty = false;
            }

            return _cachedMatrix;
        }
    }
}