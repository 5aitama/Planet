using Saitama.ProceduralMesh;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public struct RecalculateIndexJob : IJobParallelFor
{
    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Triangle> trisBuffer;

    [WriteOnly, NativeDisableParallelForRestriction]
    public NativeArray<Vertex> vertices;

    [WriteOnly]
    public NativeArray<Saitama.ProceduralMesh.Triangle> triangles;

    public void Execute(int i)
    {
        var vIndex = i * 3;

        vertices[vIndex] = new Vertex { pos = trisBuffer[i][0] };
        vertices[vIndex + 1] = new Vertex { pos = trisBuffer[i][1] };
        vertices[vIndex + 2] = new Vertex { pos = trisBuffer[i][2] };

        triangles[i] = new Saitama.ProceduralMesh.Triangle(0, 1, 2) + vIndex;
    }
}