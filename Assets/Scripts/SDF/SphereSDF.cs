using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public static class SphereSDF
{
    public static JobHandle Schedule(in SDFParams sdfParams, in float radius, ref TestChunk chunk, JobHandle inputDeps = default)
    {
        return new Noise.SphereSDFJob
        {
            sdfParams   = sdfParams,
            points      = chunk.gridData,
            radius      = radius,
        }
        .Schedule(chunk.gridData.Length, 32, inputDeps);
    }
}