using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkNet.Geometry;
using SilkNet.Geometry.Primitives;
using SilkNet.Rendering;
using Shader = SilkNet.Rendering.Shader;
using Texture = SilkNet.Rendering.Texture;

namespace SilkNet;

public class Application
{
    public static GL GlContext { get; private set; }
    private static IWindow _window;

    private static Shader _lampShader;
    private static Shader _lightingShader;
    private static Texture _diffuseMap;
    private static Texture _specularMap;
    private static Gif _dogGif;
    private static Vector3 LampPosition = new Vector3(1.2f, 1.0f, 2.0f);

    private static Cube _cube;
    private static Material _cubeMaterial;

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

        ShaderLoader.VertexShader = ShaderLoader.LoadRaw("VertexShader");
        ShaderLoader.UnlitShader = ShaderLoader.LoadRaw("UnlitShader");
        ShaderLoader.LitShader = ShaderLoader.LoadRaw("LitShader");
        
        //Getting the opengl api for drawing to the screen.
        GlContext = GL.GetApi(_window);

        //The lighting shader will give our main cube its colour multiplied by the lights intensity
        _lightingShader = new Shader(GlContext, ShaderLoader.VertexShader, ShaderLoader.LitShader);
        _lampShader = new Shader(GlContext, ShaderLoader.VertexShader, ShaderLoader.UnlitShader);

        _diffuseMap = new Texture(GlContext, "Images/silkBoxed.png");
        _specularMap = new Texture(GlContext, "Images/silkSpecular.png");
        _dogGif = new Gif(GlContext, "Images/dancing-dog.gif");
        
        _camera = new Camera(Vector3.UnitZ * 6, Vector3.UnitZ * -1, Vector3.UnitY, (float)_window.Size.X / _window.Size.Y);

        _cubeMaterial = new Material("BasicMat", _lightingShader);
        _cubeMaterial.AddTexture(_diffuseMap);
        _cubeMaterial.AddTexture(_specularMap);
        _cubeMaterial.AddProperty<Matrix4x4>("uModel");
        _cubeMaterial.AddProperty<Matrix4x4>("uView");
        _cubeMaterial.AddProperty<Matrix4x4>("uProjection");
        _cubeMaterial.AddProperty<Vector3>("viewPos");
        _cubeMaterial.AddProperty<float>("material.diffuse");
        _cubeMaterial.AddProperty<float>("material.specular");
        _cubeMaterial.AddProperty<float>("material.shininess");
        
        _cubeMaterial.AddProperty<Vector3>("light.specular");
        _cubeMaterial.AddProperty<Vector3>("light.ambient");
        _cubeMaterial.AddProperty<Vector3>("light.diffuse");
        _cubeMaterial.AddProperty<Vector3>("light.position");

        _cube = new Cube(_cubeMaterial);
        _cube.PreRender += CubeOnPreRender;
    }

    private static void CubeOnPreRender()
    {
        _cubeMaterial.SetProperty("uModel", Matrix4x4.CreateRotationY(25f));
        _cubeMaterial.SetProperty("uView", _camera.ViewMatrix);
        _cubeMaterial.SetProperty("uProjection", _camera.ProjectionMatrix);
        _cubeMaterial.SetProperty("viewPos", _camera.Position);
        _cubeMaterial.SetProperty("material.diffuse", 0f);
        _cubeMaterial.SetProperty("material.specular", 1f);
        _cubeMaterial.SetProperty("material.shininess", 32f);
        
        var difference = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        _lightColor = Vector3.Zero;
        _lightColor.X = MathF.Sin(difference * 2f);
        _lightColor.Y = MathF.Sin(difference * 0.7f);
        _lightColor.Z = MathF.Sin(difference * 1.3f);
        
        var diffuseColor = _lightColor * new Vector3(0.5f);
        var ambientColor = diffuseColor * new Vector3(0.3f);
        
        _cubeMaterial.SetProperty("light.specular", new Vector3(1f, 1f, 1f));
        _cubeMaterial.SetProperty("light.ambient", ambientColor);
        _cubeMaterial.SetProperty("light.diffuse", diffuseColor);
        _cubeMaterial.SetProperty("light.position", LampPosition);
    }

    private static void OnUpdate(double deltaTime)
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        HandleMovement(moveSpeed);

        _normalizedTime = Math.Clamp(_normalizedTime + (float)deltaTime, 0, 1);
        if(_normalizedTime >= 1)
        {
            _normalizedTime = 0;
        }
        
        _currentFrame = (int)(_normalizedTime * _dogGif.FrameCount);
    }

    private static void HandleMovement(float moveSpeed)
    {
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
    }

    private static void OnRender(double deltaTime)
    {
        GlContext.Enable(EnableCap.DepthTest);
        GlContext.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _cube.Draw();
        
        RenderLampCube();
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

        GlContext.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    private static void OnClose()
    {
        _lampShader.Dispose();
        _lightingShader.Dispose();
        _diffuseMap.Dispose();
        _specularMap.Dispose();
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