using SilkNet.Rendering;

namespace SilkNet.Geometry;

public abstract class GeometryObject : IDisposable
{
    public Transform Transform { get; set; } = new Transform();
    public int MaterialIndex { get; set; } = -1;
    public GeometryData GeometryData { get; }

    public abstract void OpenDrawingContext();
    public abstract void Draw();
    
    public GeometryObject(GeometryData geometryData, int materialIndex)
    {
        GeometryData = geometryData;
        MaterialIndex = materialIndex;
    }

    public abstract void Dispose();
}