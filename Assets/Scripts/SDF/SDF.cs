using Unity.Jobs;
using Unity.Mathematics;

public static class SDF
{
    public static JobHandle SphereSDF(in SDFParams sdfParams, in float radius, ref TestChunk chunk, JobHandle inputDeps = default)
    {
        return new Noise.SphereSDFJob
        {
            sdfParams   = sdfParams,
            points      = chunk.gridData,
            radius      = radius,
        }
        .Schedule(chunk.gridData.Length, 32, inputDeps);
    }

    public static JobHandle BoxSDF(in SDFParams sdfParams, in float3 size, ref TestChunk chunk, JobHandle inputDeps = default)
    {
        return new Noise.BoxSDFJob
        {
            sdfParams   = sdfParams,
            points      = chunk.gridData,
            size        = size,
        }
        .Schedule(chunk.gridData.Length, 32, inputDeps);
    }
}