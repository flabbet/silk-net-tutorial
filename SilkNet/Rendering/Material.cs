using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkNet.Rendering;

public class Material : IDisposable
{
    public string Name { get; set; }
    public Shader Shader { get; set; }
    public List<ShaderProperty> Properties { get; set; } = new List<ShaderProperty>();
    public Texture[] Textures { get; set; } = new Texture[32];
    
    private int _textureCount;
    
    public Material(string name, Shader shader)
    {
        Name = name;
        Shader = shader;
    }

    public void Use()
    {
        BindTextures();
        Shader.Use();
    }

    private void BindTextures()
    {
        for (var i = 0; i < _textureCount; i++)
        {
            TextureUnit unit = (TextureUnit)(0x84C0 + i);
            Textures[i].Bind(unit);
        }
    }

    public void AddTexture(Texture texture)
    {
        Textures[_textureCount] = texture;
        _textureCount++;
    }

    public void AddProperty<T>(string name, T defaultValue = default) where T: struct
    {
        ShaderProperty<T> property = new ShaderProperty<T>(name, defaultValue);
        Properties.Add(property);
        ApplyToShader(property);
    }
    
    public void SetProperty<T>(string name, T value) where T : struct
    {
        foreach (var property in Properties)
        {
            if (property.UniformName == name)
            {
                if (property is ShaderProperty<T> prop)
                {
                    prop.Value = value;
                    ApplyToShader(prop);
                    return;
                }
                
                throw new Exception($"Property {name} is not of type {typeof(T)}");
            }
        }
        
        throw new Exception($"Property {name} does not exist");
    }

    private void ApplyToShader<T>(ShaderProperty<T> prop) where T : struct
    {
        if (prop.Value is float floatValue)
        {
            Shader.SetUniform(prop.UniformName, floatValue);
        }
        else if (prop.Value is int intValue)
        {
            Shader.SetUniform(prop.UniformName, intValue);
        }
        else if (prop.Value is Vector3 vec3)
        {
            Shader.SetUniform(prop.UniformName, vec3);
        }
        else if(prop.Value is Matrix4x4 mat4)
        {
            Shader.SetUniform(prop.UniformName, mat4);
        }
        else
        {
            throw new Exception($"Property {prop.UniformName} is not a supported type");
        }
    }
    
    public void Dispose()
    {
        Shader.Dispose();
        foreach (var texture in Textures)
        {
            texture?.Dispose();
        }
    }
}