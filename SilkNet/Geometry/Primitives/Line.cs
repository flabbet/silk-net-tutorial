﻿using System.Numerics;
using Silk.NET.OpenGL;
using SilkNet.Rendering;

namespace SilkNet.Geometry.Primitives;

public class Line : GeometryObject
{
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;

    private static readonly uint[] Indices =
    {
        0, 1
    };
    
    public Vector3 Start { get; set; }
    public Vector3 End { get; set; }
    
    public Line(Vector3 start, Vector3 end, int materialIndex) : base(new GeometryData(new[] { start.X, start.Y, start.Z, end.X, end.Y, end.Z }, Indices), materialIndex)
    {
        Start = start;
        End = end;

        _vbo = new BufferObject<float>(Application.GlContext, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(Application.GlContext, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(Application.GlContext, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0);
    }

    public override void OpenDrawingContext()
    {
        _vao.Bind();
    }

    public override void Draw()
    {
        Application.GlContext.DrawArrays(PrimitiveType.Lines, 0, 2);
    }

    public override bool IsInFrustum(Frustum frustum, Transform transform)
    {
        return true;
    }

    public override void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}