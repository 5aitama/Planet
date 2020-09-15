using Unity.Mathematics;

public struct Planet
{
    public float3 Center    { get; private set; }
    public float Radius     { get; private set; }

    public Planet(float3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}