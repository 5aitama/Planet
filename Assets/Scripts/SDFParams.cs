using Unity.Mathematics;

public struct SDFParams
{
    public float3 position;
    public int3 chunkSize;
    public float power;
    public float deltaTime;

    public SDFParams(in float3 position, in int3 chunkSize, in float power, in float deltaTime)
    {
        this.position   = position;
        this.chunkSize  = chunkSize;
        this.power      = power;
        this.deltaTime  = deltaTime;
    }
}