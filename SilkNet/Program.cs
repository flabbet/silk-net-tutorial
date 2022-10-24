using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace SilkNet;

public class Program
{
    private static IWindow _window;
    private static GL _gl;
    
    private static string vertexShader;
    private static string fragmentShader;
    
    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;
    private static uint _shaderProgram;

    private static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "A window";
        
        _window = Window.Create(options);
        
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        
        _window.Run();
    }

    private static void OnLoad()
    {
        IInputContext input = _window.CreateInput();
        RegisterKeyboards(input);

        vertexShader = ShaderLoader.LoadRaw("VertexShader");
        fragmentShader = ShaderLoader.LoadRaw("FragmentShader");
        
        //Getting the opengl api for drawing to the screen.
        _gl = GL.GetApi(_window);

        //Creating a vertex array.
        _vao = GenerateVertexArray();

        //Initializing a vertex buffer that holds the vertex data.
        _vbo = GenerateBuffer(GLEnum.ArrayBuffer);
        BindVertexGeometryData();

        _ebo = GenerateBuffer(GLEnum.ElementArrayBuffer);
        BindIndicesGeometryData();

        uint vertShader = CreateShader(ShaderType.VertexShader, vertexShader, out _);
        uint fragShader = CreateShader(ShaderType.FragmentShader, fragmentShader, out _);

        _shaderProgram = CreateShaderProgram(vertShader, fragShader);
        
        _gl.GetProgram(_shaderProgram, GLEnum.LinkStatus, out var linkingStatus);
        if (linkingStatus == 0)
        {
            Console.WriteLine($"Error linking shaders {_gl.GetProgramInfoLog(_shaderProgram)}");
        }

        DisposeShaders(_shaderProgram, fragShader, vertShader);
        
        //Tell opengl how to give the data to the shaders.
        SetupVertexInputPass();
    }
    
    private static void OnUpdate(double value)
    {
        
    }
    
    private static unsafe void OnRender(double value)
    {
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_shaderProgram);
        
        _gl.DrawElements(PrimitiveType.Triangles, (uint)GeometryData.Indices.Length, DrawElementsType.UnsignedInt, null);
    }
    
    private static void OnClose()
    {
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteProgram(_shaderProgram);
    }

    private static unsafe void SetupVertexInputPass()
    {
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        _gl.EnableVertexAttribArray(0);
    }

    private static void DisposeShaders(uint shaderProgram, params uint[] shaders)
    {
        for (int i = 0; i < shaders.Length; i++)
        {
            var shader = shaders[i];
            _gl.DetachShader(shaderProgram, shader);
            _gl.DeleteShader(shader);
        }
    }

    private static uint CreateShaderProgram(params uint[] shaders)
    {
        uint program = _gl.CreateProgram();

        for (int i = 0; i < shaders.Length; i++)
        {
            _gl.AttachShader(program, shaders[i]);
        }
        
        _gl.LinkProgram(program);
        return program;
    }
    
    private static uint CreateShader(ShaderType type, string rawShader, out string errors)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, rawShader);
        _gl.CompileShader(shader);
        errors = _gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(errors))
        {
            Console.WriteLine($"Error compiling shader: {errors}");
        }
        
        return shader;
    }

    private static unsafe void BindIndicesGeometryData()
    {
        fixed (void* i = &GeometryData.Indices[0])
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(GeometryData.Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
        }
    }

    private static unsafe void BindVertexGeometryData()
    {
        fixed (void* v = &GeometryData.Vertices[0])
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (GeometryData.Vertices.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw); //Setting buffer data.
        }
    }

    private static uint GenerateVertexArray()
    {
        uint vbo = _gl.GenVertexArray();
        _gl.BindVertexArray(vbo);
        return vbo;
    }
    
    private static uint GenerateBuffer(GLEnum bufferType)
    {
        uint buffer = _gl.GenBuffer();
        _gl.BindBuffer(bufferType, buffer);
        return buffer;
    }

    private static void RegisterKeyboards(IInputContext input)
    {
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += OnKeyDown;
        }
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int someIntValue)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}