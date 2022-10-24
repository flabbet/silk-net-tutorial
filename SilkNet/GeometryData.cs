namespace SilkNet;

public static class GeometryData
{
    //Vertex data, uploaded to the VBO.
    public static readonly float[] Vertices =
    {
        //X    Y      Z     R  G  B  A
        0.5f,  0.5f, 0.0f, 1, 0, 0, 1,
        0.5f, -0.5f, 0.0f, 0, 0, 0, 1,
        -0.5f, -0.5f, 0.0f, 0, 0, 1, 1,
        -0.5f,  0.5f, 0.5f, 0, 0, 0, 1
    };

    public static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
}