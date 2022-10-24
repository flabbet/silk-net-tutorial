using Silk.NET.OpenGL;

namespace SilkNet;

public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
{
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _glContext;

    public unsafe BufferObject(GL glContext, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _glContext = glContext;
        _bufferType = bufferType;
        _handle = _glContext.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            _glContext.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, GLEnum.StaticDraw);
        }
    }

    public void Bind()
    {
        _glContext.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _glContext.DeleteBuffer(_handle);
    }
}