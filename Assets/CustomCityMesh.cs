using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

[System.Serializable]
public class CustomCityMesh {
    public Vector3[] verts;
    Color[] colors;

    List<int[]> roadTris;
    List<int[]> buildingTris;

    CustomSingleMesh stressNetwork;

    public CustomCityMesh() {
        verts = new Vector3[0];
        colors = new Color[0];
        roadTris = new List<int[]>();
        buildingTris = new List<int[]>();
    }

    public void AddRoadMesh(CustomSingleMesh input) {
        Vector3[] newVerts = new Vector3[verts.Length + input.verts.Length];
        System.Array.Copy(verts, 0, newVerts, 0, verts.Length);
        System.Array.Copy(input.verts, 0, newVerts, verts.Length, input.verts.Length);

        Color[] newColors = new Color[colors.Length + input.colors.Length];
        System.Array.Copy(colors, 0, newColors, 0, colors.Length);
        System.Array.Copy(input.colors, 0, newColors, colors.Length, input.colors.Length);

        int[] newTris = input.tris;
        for (int i = 0; i < newTris.Length; i++) {
            newTris[i] += verts.Length;
        }
        roadTris.Add(newTris);

        verts = newVerts;
        colors = newColors;
    }

    public void AddBuildingMesh(CustomSingleMesh input) {
        Vector3[] newVerts = new Vector3[verts.Length + input.verts.Length];
        System.Array.Copy(verts, 0, newVerts, 0, verts.Length);
        System.Array.Copy(input.verts, 0, newVerts, verts.Length, input.verts.Length);

        Color[] newColors = new Color[colors.Length + input.colors.Length];
        System.Array.Copy(colors, 0, newColors, 0, colors.Length);
        System.Array.Copy(input.colors, 0, newColors, colors.Length, input.colors.Length);

        int[] newTris = input.tris;
        for (int i = 0; i < newTris.Length; i++) {
            newTris[i] += verts.Length;
        }
        buildingTris.Add(newTris);

        verts = newVerts;
        colors = newColors;
    }

    public void AddStressNetwork(CustomSingleMesh input) {
        stressNetwork = input;
    }

    public Mesh GetMesh(int numDistricts = 1, bool showStressMap = false) {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        if (showStressMap && stressNetwork != null) {
            // mesh.colors = colors;
            // int subMeshNum = Mathf.Min(numDistricts, buildingTris.Count);

            // mesh.subMeshCount = subMeshNum;
            // for (int i = 0; i < subMeshNum; i++) {
            //     mesh.SetTriangles(roadTris[i], i);
            // }
            mesh = stressNetwork.GetMesh();
        } else {
            int subMeshNum = Mathf.Min(numDistricts, buildingTris.Count) * 2;

            mesh.subMeshCount = subMeshNum;
            for (int i = 0; i < numDistricts; i++) {
                mesh.SetTriangles(roadTris[i], i);
                mesh.SetTriangles(buildingTris[i], subMeshNum / 2 + i);
            }
        }

        mesh.RecalculateNormals();
        return mesh;
    }
}