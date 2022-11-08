using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkNet.Geometry;
using SilkNet.Geometry.Primitives;
using SilkNet.Optimization;
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

    private static List<GeometryObject> _objects = new List<GeometryObject>();
    private static List<GeometryObject> _gizmos = new List<GeometryObject>();
    private static List<Material> _materials = new List<Material>();

    private static Camera _camera; 

    private static Vector2 _lastMousePosition;
    private static IKeyboard _primaryKeyboard;

    private static Vector3 _lightColor;
    private const int GifSpeed = 1;
    private static float _normalizedTime;
    private static int _currentFrame;
    private static double _lastTime;
    
    //Track when the window started so we can use the time elapsed to rotate the cube
    private static DateTime _startTime;
    
    private static MaterialBatcher _materialBatcher;

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
        _window.VSync = false;
        _window.WindowBorder = WindowBorder.Fixed;

        _window.Run();
    }

    private static void OnLoad()
    {
        _startTime = DateTime.UtcNow;
        IInputContext input = _window.CreateInput();
        RegisterKeyboards(input);
        RegisterMouse(input);

        ShaderLoader.BasicVertexShader = ShaderLoader.LoadRaw("BasicVertexShader");
        ShaderLoader.VertexShader = ShaderLoader.LoadRaw("VertexShader");
        ShaderLoader.UnlitShader = ShaderLoader.LoadRaw("UnlitShader");
        ShaderLoader.LitShader = ShaderLoader.LoadRaw("LitShader");
        
        //Getting the opengl api for drawing to the screen.
        GlContext = GL.GetApi(_window);

        //The lighting shader will give our main cube its colour multiplied by the lights intensity
        _lightingShader = new Shader(GlContext, ShaderLoader.VertexShader, ShaderLoader.LitShader);
        _lampShader = new Shader(GlContext, ShaderLoader.BasicVertexShader, ShaderLoader.UnlitShader);

        _diffuseMap = new Texture(GlContext, "Images/silkBoxed.png");
        _specularMap = new Texture(GlContext, "Images/silkSpecular.png");
        _dogGif = new Gif(GlContext, "Images/dancing-dog.gif");

        _camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, (float)_window.Size.X / _window.Size.Y);

        Material cubeMat = new Material("BasicMat", _lightingShader);
        cubeMat.AddProperty("material.diffuse", 1f);
        cubeMat.AddProperty("material.specular", 1f);
        cubeMat.AddProperty("material.shininess", 32f);
        
        cubeMat.AddProperty<Vector3>("light.specular", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.ambient", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.diffuse", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.position", LampPosition);
        
        Material unlitMat = new Material("UnlitMat", _lampShader);
        unlitMat.AddProperty("uColor", Vector3.One);

        _materials.Add(cubeMat);
        _materials.Add(unlitMat);

        SpawnCubes(10, 10, 10);
        
        _materialBatcher = new MaterialBatcher(_objects);
    }

    private static void SpawnCubes(int rows, int columns, int depth)
    {
        for (int y = 0; y < columns; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    _objects.Add(new Cube(0)
                    {
                        Transform = { Position = new Vector3(x * 2.5f, y * 2.5f, z * 2.5f) }
                    });
                }
            }
        }
    }

    private static void UpdateBasicMaterial()
    {
        Material cubeMat = _materials[0];

        var difference = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        _lightColor = Vector3.Zero;
        _lightColor.X = MathF.Sin(difference * 2f);
        _lightColor.Y = MathF.Sin(difference * 0.7f);
        _lightColor.Z = MathF.Sin(difference * 1.3f);

        var diffuseColor = _lightColor * new Vector3(0.5f);
        var ambientColor = diffuseColor * new Vector3(1f);
        
        cubeMat.SetProperty("light.specular", new Vector3(1f, 1f, 1f));
        cubeMat.SetProperty("light.ambient", ambientColor);
        cubeMat.SetProperty("light.diffuse", diffuseColor);
        cubeMat.SetProperty("light.position", LampPosition);
    }

    private static void OnUpdate(double deltaTime)
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        HandleMovement(moveSpeed);
        
        float t = (float) (DateTime.UtcNow - _startTime).TotalSeconds;

        _normalizedTime = Math.Clamp(_normalizedTime + (float)deltaTime, 0, 1);
        if(_normalizedTime >= 1)
        {
            _normalizedTime = 0;
        }

        double currentTime = _window.Time;
        _currentFrame++;

        if (currentTime - _lastTime >= 1)
        {
            _window.Title = $"Party Time - ms/frame: {(1000.0 / _currentFrame):F} (FPS: {_currentFrame})";
            _lastTime = currentTime;
            _currentFrame = 0;
        }

        _camera.RecalculateFrustum();
    }

    private static void HandleMovement(float moveSpeed)
    {
        if (_primaryKeyboard.IsKeyPressed(Key.W))
        {
            _camera.Position += moveSpeed * _camera.Forward;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.S))
        {
            _camera.Position -= moveSpeed * _camera.Forward;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * moveSpeed;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * moveSpeed;
        }
    }

    private static void OnRender(double deltaTime)
    {
        GlContext.Enable(EnableCap.DepthTest);
        GlContext.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        UpdateBasicMaterial();
        RenderObjects();
    }

    private static void RenderObjects()
    {
        foreach (var obj in _materialBatcher.Batches)
        {
            Batch batch = obj.Value;
            Material material = _materials[obj.Key];
            material.Use(_camera);
            
            for (int i = 0; i < batch.ObjectsCount; i++)
            {
                GeometryObject geometryObject = _objects[batch.StartIndex + i];
                if(!geometryObject.IsInFrustum(_camera.Frustum, geometryObject.Transform)) continue;
                
                geometryObject.OpenDrawingContext();
                _materials[geometryObject.MaterialIndex].PrepareForObject(geometryObject.Transform);
                geometryObject.Draw();
            }
           
        }
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