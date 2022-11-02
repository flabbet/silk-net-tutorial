using System.Numerics;

namespace SilkNet.Rendering;

public class ShaderProperty
{
    public string UniformName { get; set; }
    public object Value { get; set; }
    
    public ShaderProperty(string name)
    {
        UniformName = name;
    }
}

public class ShaderProperty<T> : ShaderProperty
{
    public new T Value { get; set; }
    
    public ShaderProperty(string uniformName, T value) : base(uniformName)
    {
        UniformName = uniformName;
        Value = value;
    }
}