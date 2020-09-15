using UnityEngine;
using System.Collections;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public static class PhysicsBakeMesh
{
    [BurstCompile]
    public struct PhysicsBakeMeshJob : IJob
    {
        [ReadOnly]
        public int meshID;

        public void Execute()
        {
            Physics.BakeMesh(meshID, false);
        }
    }
}
