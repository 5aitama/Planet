using Saitama.Mathematics;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Noise
{
    [BurstCompile]
    public struct NoiseJob : IJobParallelFor
    {
        [ReadOnly]
        public int3 chunkSize;

        [ReadOnly]
        public float frequency;

        [ReadOnly]
        public float amplitude;

        [WriteOnly]
        public NativeArray<float4> points;

        public void Execute(int index)
        {
            var pos = index.To3D(chunkSize);

            var sphere = math.length(index.To3D(32) - new float3(16)) - 16f;
            sphere += noise.snoise((float3)pos * frequency);
            sphere = math.min(1f, math.max(-1f, sphere)) * -1;

            points[index] = new float4(pos, sphere);
        }
    }

    [BurstCompile]
    public struct SphereSDFJob : IJobParallelFor
    {
        [ReadOnly]
        public SDFParams sdfParams;

        [ReadOnly]
        public float radius;

        public NativeArray<float4> points;

        public void Execute(int index)
        {

            float val = math.length(index.To3D(sdfParams.chunkSize) - sdfParams.position) - radius;
            val = math.min(0f, math.max(-1f, val));
            var w = math.min(1f, math.max(-1f, points[index].w - val * sdfParams.power * sdfParams.deltaTime));
            var p = points[index];
            p.w = w;
            points[index] = p;
            
        }
    }

    [BurstCompile]
    public struct BoxSDFJob : IJobParallelFor
    {
        [ReadOnly]
        public SDFParams sdfParams;

        /// <summary>
        /// The size of the box.
        /// </summary>
        [ReadOnly]
        public float3 size;

        /// <summary>
        /// The chunk's points.
        /// </summary>
        public NativeArray<float4> points;

        public void Execute(int index)
        {
            var q = math.abs(index.To3D(sdfParams.chunkSize) - sdfParams.position) - size;
            var val = math.length(math.max(q, 0f)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0f);

            val = math.min(0f, math.max(-1f, val));
            var w = math.min(1f, math.max(-1f, points[index].w - val * sdfParams.power * sdfParams.deltaTime));
            var p = points[index];
            p.w = w;
            
            points[index] = p;

        }
    }
}
