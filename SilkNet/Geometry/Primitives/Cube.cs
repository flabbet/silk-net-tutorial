using System.Numerics;
using Silk.NET.OpenGL;
using SilkNet.Rendering;

namespace SilkNet.Geometry.Primitives;

public class Cube : GeometryObject
{
    private static readonly float[] Vertices = new[]
    {
        //X    Y      Z       Normals             U     V
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,

        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,
        1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f,
        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,

        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        -1f,  1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,

        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f
    };

    private static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
                
    private BufferObject<float> _vbo;
    
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;

    public Cube(int materialIndex) : base(new GeometryData(Vertices, Indices), materialIndex)
    {
        _ebo = new BufferObject<uint>(Application.GlContext, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(Application.GlContext, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(Application.GlContext, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
        _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
        _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
    }

    public override void OpenDrawingContext()
    {
        _vao.Bind();
    }

    public override void Draw() 
    {
        Application.GlContext.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    public override bool IsInFrustum(Frustum frustum, Transform transform)
    {
        Vector3 position = transform.Position;

        Vector3 min = position - Vector3.One;
        Vector3 max = position + Vector3.One;
        
        AABB aabb = new AABB(min, max); 
        return aabb.IsOnOrForwardPlane(frustum.Left) && aabb.IsOnOrForwardPlane(frustum.Right) &&
               aabb.IsOnOrForwardPlane(frustum.Top) && aabb.IsOnOrForwardPlane(frustum.Bottom) &&
               aabb.IsOnOrForwardPlane(frustum.Near) && aabb.IsOnOrForwardPlane(frustum.Far);
    }

    public override void Dispose()
    {
        _vbo.Dispose();
        _ebo.Dispose(); 
        _vao.Dispose();
    }
}