using SilkNet.Rendering;

namespace SilkNet.Geometry;

public abstract class GeometryObject : IDisposable
{
    public Transform Transform { get; set; } = new Transform();
    public Material Material { get; set; }
    public GeometryData GeometryData { get; }
    
    public abstract void Draw();
    
    public GeometryObject(GeometryData geometryData, Material material)
    {
        GeometryData = geometryData;
        Material = material;
    }

    public abstract void Dispose();
}