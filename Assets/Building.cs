using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Building {
    private CityConstants constants;

    List<Road> landBorders;
    List<Road> inwardJuttingEdges;
    List<Road> foundationEdges;
    List<Vector2> foundationVertices;

    float roadWidth;

    float buildingHeight = 10f;

    public Building(List<Road> landBorders, CityConstants constants, float roadWidth) {
        this.roadWidth = roadWidth;
        this.constants = constants;
        this.landBorders = landBorders;

        Road referenceRoad = landBorders.First();
        if (referenceRoad != null) {
            buildingHeight += GetPerlinValue(referenceRoad.source);
        }

        GenerateFoundations();
    }

    public float GetPerlinValue(Vector2 input) {
        Vector2 populationCoord = input / 75 + constants.GetPopShift();
        Vector2 businessCoord = input / 75 + constants.GetBusShift();

        float populationNoiseValue = Mathf.PerlinNoise(populationCoord.x, populationCoord.y);
        float businessNoiseValue = Mathf.PerlinNoise(businessCoord.x, businessCoord.y);

        float noiseValue = (populationNoiseValue + businessNoiseValue + constants.GetNormalisedDistanceToCentre(input) + constants.GetNormalisedDistanceToCentre(input)) / 4.0f;

        if (noiseValue == 0) {
            Debug.Log("PERLIN NOISE 0 GENERATED");
            return 0;
        }

        int range = 100;
        int power = 4;

        return range * Mathf.Pow(noiseValue, power);
    }

    public bool GenerateFoundations() {
        List<Road> inwardJuttingEdges = new List<Road>();
        for (int i = 0; i < landBorders.Count; i++) {
            Road juttingEdge = landBorders[i].GetBisector(landBorders[(i + 1) % landBorders.Count], 1.5f * roadWidth);
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

    public CustomSingleMesh CreateMesh(bool flattenBuildings = false) {
        CustomSingleMesh outputMesh = new CustomSingleMesh();
        if (foundationEdges != null) {
            if (!flattenBuildings) {
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
                    CustomSingleMesh wall = new CustomSingleMesh(verts, tris);
                    outputMesh.ConcatMesh(wall);
                }
            }
            outputMesh.ConcatMesh(CreateRoof(flattenBuildings));
        }
        return outputMesh;
    }

    public CustomSingleMesh CreateRoof(bool flattenBuildings = false) {
        float displayHeight = flattenBuildings ? 0 : buildingHeight;
        Vector2[] verts2D = foundationVertices.ToArray();
        Triangulator tr = new Triangulator(verts2D);
        int[] indices = tr.Triangulate();
        Vector3[] vertices = new Vector3[verts2D.Length];
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = new Vector3(verts2D[i].x, displayHeight, verts2D[i].y);
        }
        CustomSingleMesh mesh = new CustomSingleMesh(vertices, indices);
        return mesh;
    }

    public float SignedPolygonArea() {

        // Get the areas.
        float area = 0;
        foreach (Road edge in foundationEdges) {
            area += (edge.destination.x + edge.source.x) *
                (edge.destination.y - edge.source.y) / 2;
        }

        // Return the result.
        return area;
    }
}