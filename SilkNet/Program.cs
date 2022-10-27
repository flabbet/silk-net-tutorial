using System.Numerics;
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
    private static Gif _dogGif;

    private static Vector3 _cameraPosition = new Vector3(0, 1, 3f);
    private static Vector3 _cameraFront = new Vector3(0, 0, -1);
    private static Vector3 _cameraTarget = Vector3.Zero;
    private static Vector3 _cameraDirection = Vector3.Normalize(_cameraPosition - _cameraTarget);
    private static Vector3 _cameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, _cameraDirection));
    private static Vector3 _cameraUp = Vector3.Cross(_cameraDirection, _cameraRight);

    private static float _cameraYaw = -90f;
    private static float _cameraPitch = 0f;
    private static float _cameraZoom = 45f;

    private static Vector2 _lastMousePosition;
    private static IKeyboard _primaryKeyboard;
    
    private const int GifSpeed = 1;
    private static float _normalizedTime;
    private static int _currentFrame;
    
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
        IInputContext input = _window.CreateInput();
        RegisterKeyboards(input);
        RegisterMouse(input);

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
        _texture = new Texture(_gl, "Images/texture.png");
        _dogGif = new Gif(_gl, "Images/dancing-dog.gif");
    }
    
    private static void OnUpdate(double deltaTime)
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        if (_primaryKeyboard.IsKeyPressed(Key.W))
        {
            _cameraPosition += moveSpeed * _cameraFront;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.S))
        {
            _cameraPosition -= moveSpeed * _cameraFront;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.A))
        {
            _cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
        }
        if (_primaryKeyboard.IsKeyPressed(Key.D))
        {
            _cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
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
        _vao.Bind();
        
        _shaderProgram.Use();
        _dogGif.GetFrame(_currentFrame).Bind();
        
        var difference = (float)(_window.Time * 100);
        float differenceRadians = MathHelper.DegreesToRadians(difference);
        var model = Matrix4x4.CreateRotationY(differenceRadians) * Matrix4x4.CreateRotationX(differenceRadians);
        var view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_cameraZoom), _window.Size.X / _window.Size.Y, 0.1f, 100f);
        
        _shaderProgram.SetUniform("uModel", model);
        _shaderProgram.SetUniform("uView", view);
        _shaderProgram.SetUniform("uProjection", projection);
        
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
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
        _cameraZoom = Math.Clamp(_cameraZoom - wheel.Y, 1f, 45f);
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
            float xOffset = (pos.X - _lastMousePosition.X) * lookSensitivity;
            float yOffset = (pos.Y - _lastMousePosition.Y) * lookSensitivity;
            _lastMousePosition = pos;
            
            _cameraYaw += xOffset;
            _cameraPitch -= yOffset;

            _cameraPitch = Math.Clamp(_cameraPitch, -89f, 89f);

            _cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(_cameraYaw) *
                                           MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch)));
            _cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(_cameraPitch));
            _cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(_cameraYaw)) *
                                 MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
            _cameraFront = Vector3.Normalize(_cameraDirection);
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