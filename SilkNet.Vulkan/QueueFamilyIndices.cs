namespace SilkNet.Vulkan;

struct QueueFamilyIndices
{
    public uint? GraphicsFamily { get; set; }
    public bool IsComplete()
    {
        return GraphicsFamily.HasValue;
    }
}