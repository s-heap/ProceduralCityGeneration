using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh {
    public Vector3[] verts;
    public int[] tris;

    public CustomMesh(Vector3[] verts, int[] tris) {
        this.verts = verts;
        this.tris = tris;
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

    public Mesh GetMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        return mesh;
    }
}