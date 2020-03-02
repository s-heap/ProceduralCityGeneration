using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh {
    public Vector3[] verts;
    public int[] tris;

    Color[] colors;

    public CustomMesh(Vector3[] verts, int[] tris, Color[] colors = null) {
        this.verts = verts;
        this.tris = tris;
        this.colors = colors;
    }

    public void ConcatMesh(CustomMesh input) {
        Vector3[] newVerts = new Vector3[verts.Length + input.verts.Length];
        System.Array.Copy(verts, 0, newVerts, 0, verts.Length);

        System.Array.Copy(input.verts, 0, newVerts, verts.Length, input.verts.Length);
        int[] newTris = new int[tris.Length + input.tris.Length];
        System.Array.Copy(tris, 0, newTris, 0, tris.Length);
        for (int i = 0; i < input.tris.Length; i++) {
            newTris[i + tris.Length] = input.tris[i] + verts.Length;
        }

        verts = newVerts;
        tris = newTris;
    }

    public Mesh GetMesh(CityConstants constants = null) {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.triangles = tris;
        if (constants != null) {
            Color[] outputColors = new Color[verts.Length];
            for (int i = 0; i < verts.Length; i++) {
                Vector2 currentVert = new Vector2(verts[i].x, verts[i].z);
                float popNoiseValue = constants.GetPopNoiseValue(currentVert);
                float busNoiseValue = constants.GetBusNoiseValue(currentVert);

                float noiseValue = (popNoiseValue + busNoiseValue + constants.GetNormalisedDistanceToCentre(currentVert)) / 3.0f;
                outputColors[i] = new Color(Mathf.Pow(noiseValue, 3) * 2, 0.1f, 0.1f);
            }
            mesh.colors = outputColors;
        }
        return mesh;
    }
}