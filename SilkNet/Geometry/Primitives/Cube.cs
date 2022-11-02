using System.Numerics;
using Silk.NET.OpenGL;
using SilkNet.Rendering;
using Shader = SilkNet.Rendering.Shader;

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
    
    public Cube(Material material) : base(new GeometryData(Vertices, Indices), material)
    {
        _ebo = new BufferObject<uint>(Application.GlContext, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(Application.GlContext, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(Application.GlContext, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
        _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
        _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
    }

    public override void Draw() 
    {
        _vao.Bind();
        Material.Use();
        Application.GlContext.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    public override void Dispose()
    {
        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
    }
}