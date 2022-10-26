namespace SilkNet;

public static class GeometryData
{
    // OpenGL has image origin in the bottom-left corner.
    public static readonly float[] Vertices =
    {
        //X    Y      Z     U   V
        1f,  1f, 0.0f, 1f, 0f,
        1f, -1f, 0.0f, 1f, 1f,
        -1f, -1f, 0.0f, 0f, 1f,
        -1f,  1f, 0f, 0f, 0f
    };

    public static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
}