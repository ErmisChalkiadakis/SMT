using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] private bool invertRotation = false;
    [SerializeField] [Range(1f, 25f)] private float rotateSpeed = 5f;
    [SerializeField] [Range(50f, 200f)] private float radius = 100f;
    [SerializeField] [Range(32, 720)] private int numberOfVertices = 180;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameObject meshObject;
    [SerializeField] private Material material;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Vector3> cachedVertices;
    private int[] cachedIndices;
    private float previousClipIntensity;

    private Vector3 cachedLocalScale;

    void Start()
    {
        cachedLocalScale = meshObject.transform.localScale;

        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();

        Mesh mesh = GenerateCircleMesh();

        meshFilter.mesh = mesh;
        meshRenderer.material = material;
    }
    
    void Update()
    {
        float rotationOffset = invertRotation ? rotateSpeed : -rotateSpeed;
        meshObject.transform.Rotate(new Vector3(0f, 0f, transform.rotation.z + rotationOffset));
        
        UpdateCircleScale();
    }

    private void UpdateCircleScale()
    {
        float currentClipIntensity = Math.Abs(audioMixer.GetCurrentClipIntensity()) * 500f;
        meshObject.transform.localScale = cachedLocalScale + currentClipIntensity * Vector3.one;

        previousClipIntensity = currentClipIntensity;
    }

    private void UpdateTriangles()
    {
        List<Vector3> newVertices = cachedVertices;
        Vector3 direction = new Vector3();
        float clipIntensity = Math.Abs(audioMixer.GetCurrentClipIntensity()) * 1000f;
        for (int i = 1; i < cachedVertices.Count; i = i + 3)
        {
            direction = (cachedVertices[i] - meshObject.transform.localPosition).normalized; //Debug.Log($"Direction {direction} = vertex {i} {cachedVertices[i].x}, {cachedVertices[i].y}, {cachedVertices[i].z} - meshObject Position {meshObject.transform.position}");
            newVertices[i] = cachedVertices[i] + (direction * clipIntensity);
        }
        meshFilter.mesh.SetVertices(newVertices);
        meshFilter.mesh.SetIndices(cachedIndices, MeshTopology.Triangles, 0);
        meshFilter.mesh.RecalculateBounds();
    }

    private Mesh GenerateCircleMesh()
    {
        int CircleSegmentCount = numberOfVertices * 2;
        int CircleVertexCount = CircleSegmentCount + 2;
        int CircleIndexCount = CircleSegmentCount * 3;

        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        var segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        var angle = 0f;
        vertices.Add(Vector3.zero);
        for (int i = 1; i < CircleVertexCount; ++i)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            angle -= segmentWidth;
            if (i > 1)
            {
                var j = (i - 2) * 3;
                indices[j + 0] = 0;
                indices[j + 1] = i - 1;
                indices[j + 2] = i;
            }
        }
        circle.SetVertices(vertices);
        circle.SetIndices(indices, MeshTopology.Triangles, 0);
        circle.RecalculateBounds();
        cachedVertices = vertices;
        cachedIndices = indices;
        return circle;
    }
}
