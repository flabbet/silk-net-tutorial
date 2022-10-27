using System.Numerics;
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
    private static string unlitShader;
    private static string litShader;
    
    private static BufferObject<float> _vbo;
    private static BufferObject<uint> _ebo;
    private static VertexArrayObject<float, uint> _cubeVao;
    private static Shader _lightingShader;
    private static Shader _lampShader;
    private static Texture _texture;
    private static Gif _dogGif;
    private static Vector3 LampPosition = new Vector3(1.2f, 1.0f, 2.0f);

    private static Camera _camera; 

    private static Vector2 _lastMousePosition;
    private static IKeyboard _primaryKeyboard;

    private static Vector3 _lightColor;
    private const int GifSpeed = 1;
    private static float _normalizedTime;
    private static int _currentFrame;
    
    //Track when the window started so we can use the time elapsed to rotate the cube
    private static DateTime _startTime;
    
    private static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Party Time";
        _window = Window.Create(options);
        
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        _window.WindowBorder = WindowBorder.Fixed;

        _window.Run();
    }

    private static void OnLoad()
    {
        _startTime = DateTime.UtcNow;
        IInputContext input = _window.CreateInput();
        RegisterKeyboards(input);
        RegisterMouse(input);

        vertexShader = ShaderLoader.LoadRaw("VertexShader");
        unlitShader = ShaderLoader.LoadRaw("UnlitShader");
        litShader = ShaderLoader.LoadRaw("LitShader");
        
        //Getting the opengl api for drawing to the screen.
        _gl = GL.GetApi(_window);

        _ebo = new BufferObject<uint>(_gl, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _cubeVao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        _cubeVao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        _cubeVao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);

        //The lighting shader will give our main cube its colour multiplied by the lights intensity
        _lightingShader = new Shader(_gl, vertexShader, litShader);
        _lampShader = new Shader(_gl, vertexShader, unlitShader);

        _texture = new Texture(_gl, "Images/texture.png");
        _dogGif = new Gif(_gl, "Images/dancing-dog.gif");
        
        _camera = new Camera(Vector3.UnitZ * 6, Vector3.UnitZ * -1, Vector3.UnitY, (float)_window.Size.X / _window.Size.Y);
    }
    
    private static void OnUpdate(double deltaTime)
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        if (_primaryKeyboard.IsKeyPressed(Key.W))
        {
            _camera.Position += moveSpeed * _camera.Front;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.S))
        {
            _camera.Position -= moveSpeed * _camera.Front;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * moveSpeed;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * moveSpeed;
        }

        _normalizedTime = Math.Clamp(_normalizedTime + (float)deltaTime, 0, 1);
        if(_normalizedTime >= 1)
        {
            _normalizedTime = 0;
        }
        
        _currentFrame = (int)(_normalizedTime * _dogGif.FrameCount);
    }
    
    private static void OnRender(double deltaTime)
    {
        _gl.Enable(EnableCap.DepthTest);
        _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _cubeVao.Bind();
        
        RenderLitCube();
        RenderLampCube();
    }

    private static void RenderLitCube()
    {
        _lightingShader.Use();
        //_dogGif.GetFrame(_currentFrame).Bind();

        _lightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(25f));
        _lightingShader.SetUniform("uView", _camera.ViewMatrix);
        _lightingShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _lightingShader.SetUniform("viewPos", _camera.Position);
        _lightingShader.SetUniform("material.ambient", new Vector3(1f, 0.5f, 0.31f));
        _lightingShader.SetUniform("material.diffuse", new Vector3(1f, 0.5f, 0.31f));
        _lightingShader.SetUniform("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
        _lightingShader.SetUniform("material.shininess", 32f);

        var difference = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        _lightColor = Vector3.Zero;
        _lightColor.X = MathF.Sin(difference * 2f);
        _lightColor.Y = MathF.Sin(difference * 0.7f);
        _lightColor.Z = MathF.Sin(difference * 1.3f);
        
        var diffuseColor = _lightColor * new Vector3(0.5f);
        var ambientColor = diffuseColor * new Vector3(0.2f);
        
        _lightingShader.SetUniform("light.ambient", ambientColor);
        _lightingShader.SetUniform("light.diffuse", diffuseColor);
        _lightingShader.SetUniform("light.specular", new Vector3(1f, 1f, 1f));
        _lightingShader.SetUniform("light.position", LampPosition);
        
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }
    
    private static void RenderLampCube()
    {
        _lampShader.Use();

        var lampMatrix = Matrix4x4.Identity;
        lampMatrix *= Matrix4x4.CreateScale(0.2f);
        lampMatrix *= Matrix4x4.CreateTranslation(new Vector3(1.2f, 1f, 2f));

        _lampShader.SetUniform("uModel", lampMatrix);
        _lampShader.SetUniform("uView", _camera.ViewMatrix);
        _lampShader.SetUniform("uProjection", _camera.ProjectionMatrix);
        _lampShader.SetUniform("uColor", _lightColor);

        _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    private static void OnClose()
    {
        _vbo.Dispose();
        _ebo.Dispose();
        _cubeVao.Dispose();
        _lampShader.Dispose();
        _lightingShader.Dispose();
        _texture.Dispose();
        _dogGif.Dispose();
    }

    private static void RegisterKeyboards(IInputContext input)
    {
        _primaryKeyboard = input.Keyboards.FirstOrDefault();
        
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += OnKeyDown;
        }
    }

    private static void RegisterMouse(IInputContext input)
    {
        for (int i = 0; i < input.Mice.Count; i++)
        {
            var mouse = input.Mice[i];
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnScroll;
            mouse.Click += OnMouseClick;
        }
    }

    private static void OnMouseClick(IMouse mouse, MouseButton clickBtn, Vector2 pos)
    {
        if (clickBtn == MouseButton.Left)
        {
            _lastMousePosition = pos;
        }
    }

    private static void OnScroll(IMouse mouse, ScrollWheel wheel)
    {
        _camera.Zoom = wheel.Y;
    }

    private static void OnMouseMove(IMouse mouse, Vector2 pos)
    {
        if(!mouse.IsButtonPressed(MouseButton.Left)) return;
        
        float lookSensitivity = 0.1f;
        if (_lastMousePosition == default)
        {
            _lastMousePosition = pos;
        }
        else
        {
            float offsetX = (pos.X - _lastMousePosition.X) * lookSensitivity;
            float offsetY = (pos.Y - _lastMousePosition.Y) * lookSensitivity;
            _lastMousePosition = pos;
            
            _camera.SetDirection(offsetX, offsetY);
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