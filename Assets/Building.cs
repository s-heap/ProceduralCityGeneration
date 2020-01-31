using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building {

    List<Road> landBorders;
    List<Road> inwardJuttingEdges;
    List<Road> foundationEdges;
    List<Vector2> foundationVertices;

    float roadSpacing = 1.5f;

    float buildingHeight = 2.5f;

    public Building(List<Road> landBorders) {
        this.landBorders = landBorders;
        GenerateFoundations();
    }

    public bool GenerateFoundations() {
        List<Road> inwardJuttingEdges = new List<Road>();
        for (int i = 0; i < landBorders.Count; i++) {
            Road juttingEdge = landBorders[i].GetBisector(landBorders[(i + 1) % landBorders.Count], roadSpacing);
            if (CheckIntersectWithLandBorders(juttingEdge)) {
                return false;
            }
            inwardJuttingEdges.Add(juttingEdge);
        }
        List<Road> floorplan = GetFloorPlanFromJuttingEdges(inwardJuttingEdges);
        if (CheckIntersectWithSelf(floorplan)) {
            return false;
        }
        this.inwardJuttingEdges = inwardJuttingEdges;
        foundationEdges = floorplan;
        return true;
    }

    public Boolean CheckIntersectWithLandBorders(Road input) {
        foreach (Road edge in landBorders) {
            if (input.LineIntersection(edge) && input.source != edge.destination && input.source != edge.source) {
                return true;
            }
        }
        return false;
    }

    public Boolean CheckIntersectWithSelf(List<Road> input) {
        foreach (Road currentEdge in input) {
            foreach (Road otherEdge in input) {
                if (currentEdge.source != otherEdge.source && currentEdge.source != otherEdge.destination) {
                    if (currentEdge.destination != otherEdge.source && currentEdge.destination != otherEdge.destination) {
                        if (currentEdge.LineIntersection(otherEdge)) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public List<Road> GetFloorPlanFromJuttingEdges(List<Road> input) {
        List<Road> floorPlan = new List<Road>();
        foundationVertices = new List<Vector2>();
        for (int i = 0; i < landBorders.Count; i++) {
            floorPlan.Add(new Road(input[i].destination, input[(i + 1) % landBorders.Count].destination));
            foundationVertices.Add(input[i].destination);
        }
        return floorPlan;
    }

    public void DrawGizmos() {
        if (inwardJuttingEdges != null) {
            Gizmos.color = Color.yellow;
            foreach (Road edge in inwardJuttingEdges) {
                edge.DrawGizmos();
            }
        }
        if (foundationEdges != null) {
            Gizmos.color = Color.red;
            foreach (Road edge in foundationEdges) {
                edge.DrawGizmos();
            }
        }
    }

    public CustomMesh CreateMesh() {
        CustomMesh outputMesh = new CustomMesh(new Vector3[0], new int[0]);
        if (foundationEdges != null) {
            foreach (Road edge in foundationEdges) {
                Vector2 source = edge.source;
                Vector2 destination = edge.destination;
                Vector3[] verts = new Vector3[] {
                    new Vector3(source.x, 0, source.y),
                    new Vector3(destination.x, 0, destination.y),
                    new Vector3(destination.x, buildingHeight, destination.y),
                    new Vector3(source.x, buildingHeight, source.y),
                };
                int[] tris = new int[] { 0, 1, 2, 0, 2, 3 };
                CustomMesh wall = new CustomMesh(verts, tris);
                outputMesh.ConcatMesh(wall);
            }
            outputMesh.ConcatMesh(CreateRoof());
        }
        return outputMesh;
    }

    public CustomMesh CreateRoof() {
        Vector2[] verts2D = foundationVertices.ToArray();
        Triangulator tr = new Triangulator(verts2D);
        int[] indices = tr.Triangulate();
        Vector3[] vertices = new Vector3[verts2D.Length];
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = new Vector3(verts2D[i].x, buildingHeight, verts2D[i].y);
        }
        CustomMesh mesh = new CustomMesh(vertices, indices);
        return mesh;
    }
}