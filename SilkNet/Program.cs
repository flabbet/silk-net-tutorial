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
    
    private static BufferObject<float> _vbo;
    private static BufferObject<uint> _ebo;
    private static VertexArrayObject<float, uint> _vao;
    private static Shader _shaderProgram;
    private static Texture _texture; 

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

        _ebo = new BufferObject<uint>(_gl, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        _shaderProgram = new Shader(_gl, vertexShader, fragmentShader);
        _texture = new Texture(_gl, "texture.png");
    }
    
    private static void OnUpdate(double value)
    {
        
    }
    
    private static unsafe void OnRender(double value)
    {
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        
        _vao.Bind();
        _shaderProgram.Use();

        _texture.Bind();
        _shaderProgram.SetUniform("uTexture0", 0);

        _gl.DrawElements(PrimitiveType.Triangles, (uint)GeometryData.Indices.Length, DrawElementsType.UnsignedInt, null);
    }
    
    private static void OnClose()
    {
        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
        _shaderProgram.Dispose();
        _texture.Dispose();
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