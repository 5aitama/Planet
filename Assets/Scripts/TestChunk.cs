using Saitama.Mathematics;
using Saitama.ProceduralMesh;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter))]
public class TestChunk : MonoBehaviour
{

    public ComputeShader compute;
    public int3 chunkSize;

    public float threshold      = 0.5f;
    public float noiseFrequency = 0.01f;
    public float noiseAmplitude = 1f;

    private MeshCollider    meshCollider;
    private MeshFilter      meshFilter;
    private Mesh            mesh;

    // Used to store the noise data
    private ComputeBuffer gridBuffer;
    // Used to store triangle vertices
    private ComputeBuffer trisBuffer;
    // Used to store the amount of triangle in trisBuffer
    private ComputeBuffer countBuffer;

    // Used to store the value of countBuffer
    private int[] countBufferArray = new int[] { 0 };
    // Used to temporary store data from trisBuffer
    private Triangle[] trisBufferTempArray;

    // Used to store grid data
    public NativeArray<float4> gridData;

    public int PointAmount => chunkSize.Amount();
    public int BlockAmount => (chunkSize - 1).Amount();

    public const int MAX_VERTEX_PER_BLOCK   = 12;
    public const int MAX_TRIANGLE_PER_BLOCK = 5;

    private void Awake()
    {
        meshCollider    = GetComponent<MeshCollider>();
        meshFilter      = GetComponent<MeshFilter>();

        mesh = new Mesh();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void Start()
    {
        InitBuffers();
        InitializeGridData().Complete();
        BuildMesh();
    }

    public void UpdateMeshCollider()
    {
        var id = mesh.GetInstanceID();

        new PhysicsBakeMesh.PhysicsBakeMeshJob
        { 
            meshID = id,
        }
        .Schedule()
        .Complete();
        
        meshCollider.sharedMesh = mesh;
    }

    public void InitBuffers()
    {
        gridBuffer  = new ComputeBuffer(PointAmount, sizeof(float) * 4, ComputeBufferType.Default);
        trisBuffer  = new ComputeBuffer(BlockAmount * MAX_TRIANGLE_PER_BLOCK, sizeof(float) * 9, ComputeBufferType.Append);
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        trisBufferTempArray = new Triangle[BlockAmount * MAX_TRIANGLE_PER_BLOCK];
    }

    public void DisposeBuffers()
    {
        gridBuffer.Dispose();
        trisBuffer.Dispose();
        countBuffer.Dispose();
    }

    public JobHandle InitializeGridData(JobHandle inputDeps = default)
    {

        if (!gridData.IsCreated)
            gridData = new NativeArray<float4>(PointAmount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        var jobHandle = new Noise.NoiseJob
        {
            chunkSize   = chunkSize,
            frequency   = noiseFrequency,
            amplitude   = noiseAmplitude,
            points      = gridData,
        }
        .Schedule(PointAmount, 32, inputDeps);

        // Do some other jobs here...

        return jobHandle;
    }

    public void BuildMesh()
    {
        var k = compute.FindKernel("Marching");

        gridBuffer.SetData(gridData, 0, 0, gridData.Length);
        trisBuffer.SetCounterValue(0);

        compute.SetInts("chunkSize", chunkSize.x, chunkSize.y, chunkSize.z);
        compute.SetFloats("threshold", threshold);
        compute.SetBuffer(k, "points", gridBuffer);
        compute.SetBuffer(k, "triangles", trisBuffer);
        
        compute.GetKernelThreadGroupSizes(k, out uint kx, out uint ky, out uint kz);

        var thgx = (int)math.ceil(chunkSize.x / (float)kx);
        var thgy = (int)math.ceil(chunkSize.y / (float)ky);
        var thgz = (int)math.ceil(chunkSize.z / (float)kz);

        compute.Dispatch(k, thgx, thgy, thgz);

        ComputeBuffer.CopyCount(trisBuffer, countBuffer, 0);
        countBuffer.GetData(countBufferArray, 0, 0, 1);

        var tCount = countBufferArray[0];

        if (tCount == 0)
            return;

        trisBuffer.GetData(trisBufferTempArray, 0, 0, tCount);

        var vertices = new NativeArray<Vertex>(tCount * 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var triangles = new NativeArray<Saitama.ProceduralMesh.Triangle>(tCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var trisBufferNativeArray = new NativeArray<Triangle>(trisBufferTempArray, Allocator.TempJob);

        new RecalculateIndexJob
        {
            trisBuffer  = trisBufferNativeArray,
            vertices    = vertices,
            triangles   = triangles,
        }
        .Schedule(tCount, 64)
        .Complete();
        
        mesh.Update(triangles, vertices);

        triangles.Dispose();
        vertices.Dispose();

        mesh.RecalculateNormals();

        UpdateMeshCollider();
    }

    public void Clean()
    {
        mesh.Clear();
    }

    private void OnDestroy()
    {
        DisposeBuffers();
        // Don't forget to dispose native array!!
        gridData.Dispose();
    }
}