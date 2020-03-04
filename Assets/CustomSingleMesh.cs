using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

[System.Serializable]

public class CustomSingleMesh {
    public Vector3[] verts;
    public int[] tris;

    public Color[] colors;

    public CustomSingleMesh(Vector3[] verts, int[] tris, Color[] colors = null) {

        this.verts = verts;
        this.tris = tris;
        this.colors = colors;
        if (this.colors == null || colors.Length != verts.Length) {
            this.colors = Enumerable.Repeat(new Color(0, 0, 0), verts.Length).ToArray();
        }
    }

    public CustomSingleMesh() {
        this.verts = new Vector3[0];
        this.tris = new int[0];
        this.colors = new Color[0];
    }

    public void ConcatMesh(CustomSingleMesh input) {
        Vector3[] newVerts = new Vector3[verts.Length + input.verts.Length];
        System.Array.Copy(verts, 0, newVerts, 0, verts.Length);
        System.Array.Copy(input.verts, 0, newVerts, verts.Length, input.verts.Length);

        Color[] newColors = new Color[colors.Length + input.colors.Length];
        System.Array.Copy(colors, 0, newColors, 0, colors.Length);
        System.Array.Copy(input.colors, 0, newColors, colors.Length, input.colors.Length);

        int[] newTris = new int[tris.Length + input.tris.Length];
        System.Array.Copy(tris, 0, newTris, 0, tris.Length);
        for (int i = 0; i < input.tris.Length; i++) {
            newTris[i + tris.Length] = input.tris[i] + verts.Length;
        }

        verts = newVerts;
        tris = newTris;
        colors = newColors;

        if (verts.Length > colors.Length) {
            Debug.Log("SingleMeshConcat Diff Detected: " + verts.Length + " " + colors.Length);
        }
    }

    public Mesh GetMesh() {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        return mesh;
    }
}