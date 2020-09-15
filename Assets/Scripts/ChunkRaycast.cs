using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ChunkRaycast : MonoBehaviour
{
    public float maxDistance = 10f;
    public LayerMask chunkLayerMask;

    public float power = 1f;
    public float radius = 2f;

    public float3 boxSize = 2f;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, maxDistance, chunkLayerMask.value))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.white);

            if (!hit.collider.TryGetComponent(out TestChunk chunk))
                return;

            if(Input.GetMouseButton(0))
            {
                var sdfParams = new SDFParams(hit.point, chunk.chunkSize, power, Time.deltaTime);
                SDF.SphereSDF(sdfParams, radius, ref chunk).Complete();
                chunk.BuildMesh();
            }
            else if (Input.GetMouseButton(2))
            {
                var sdfParams = new SDFParams(hit.point, chunk.chunkSize, -power, Time.deltaTime);
                SDF.SphereSDF(sdfParams, radius, ref chunk).Complete();
                chunk.BuildMesh();
            }
        }
    }
}
