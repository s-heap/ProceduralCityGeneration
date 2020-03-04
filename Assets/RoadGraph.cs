using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadGraph {

    public Dictionary<Vector2, List<Road>> graph;
    public HashSet<Road> storedEdges;

    public RoadGraph() {
        this.graph = new Dictionary<Vector2, List<Road>>();
        this.storedEdges = new HashSet<Road>();
    }

    public List<Road> GetRoadList(Vector2 input) {
        List<Road> edgeList;
        if (graph.TryGetValue(input, out edgeList)) {
            return edgeList;
        } else {
            List<Road> newRoadList = new List<Road>();
            graph.Add(input, newRoadList);
            return newRoadList;
        }
    }

    public void AddEdge(Road input) {
        if (!storedEdges.Contains(input)) {
            storedEdges.Add(input);
            GetRoadList(input.source).Add(input);
        }
    }

    public void SmartAddEdge(Road input) {
        if (!storedEdges.Contains(input)) {
            List<Road> currentRoadList = GetRoadList(input.source);
            foreach (Road edge in currentRoadList) {
                if (input.AreParallel(edge)) {
                    if (input.GetLength() > edge.GetLength()) {
                        Road newEdge = new Road(edge.destination, input.destination);
                        SmartAddEdge(newEdge);
                    } else {
                        Road newEdge = new Road(input.destination, edge.destination);

                        currentRoadList.Remove(edge);
                        storedEdges.Remove(edge);

                        currentRoadList.Add(input);
                        SmartAddEdge(newEdge);

                    }
                    return;
                }
            }
            storedEdges.Add(input);
            currentRoadList.Add(input);
        }
    }

    public void AddEdgeList(List<Road> inputList) {
        foreach (Road edge in inputList) {
            AddEdge(edge);
            AddEdge(edge.GetInverse());
        }
    }

    public void SmartAddEdgeList(List<Road> inputList) {
        foreach (Road edge in inputList) {
            SmartAddEdge(edge);
            SmartAddEdge(edge.GetInverse());
        }
    }

    public Vector2 GetLeftMostNode() {
        Vector2 leftMost = graph.Keys.First();
        foreach (Vector2 node in graph.Keys) {
            if (node.x < leftMost.x) {
                leftMost = node;
            }
        }
        return leftMost;
    }

    public void AddGraph(RoadGraph input) {
        foreach (Vector2 node in input.graph.Keys) {
            SmartAddEdgeList(input.GetRoadList(node));
        }
    }

    public void Print() {
        Debug.Log("Printing Graph:");
        foreach (Vector2 node in graph.Keys) {
            List<Road> roadList = GetRoadList(node);
            String printable = "  " + node + "\n";
            foreach (Road edge in roadList) {
                printable += "    " + edge + "\n";
            }
            Debug.Log(printable);
        }
    }

    public CustomSingleMesh CreateRoadMesh(CityConstants constants, List<Road> roadsToIgnore = null) {
        if (roadsToIgnore != null) {
            foreach (Road edge in roadsToIgnore) {
                storedEdges.Remove(edge);
            }
        }
        Road[] allEdges = storedEdges.ToArray();
        foreach (Road edge in allEdges) {
            edge.stress = (constants.GetNormalisedDistanceToCentre(edge.GetMidPoint()));
            // edge.stress = -0.00001f;
        }

        Vector3[] verts = new Vector3[allEdges.Length * 4];
        int[] tris = new int[2 * allEdges.Length * 3];
        Color[] colors = new Color[allEdges.Length * 4];

        for (int i = 0; i < allEdges.Length; i++) {
            Vector3[] meshCoords = allEdges[i].GetMeshCoords(constants.roadWidth);
            System.Array.Copy(meshCoords, 0, verts, i * 4, 4);

            int[] currentTris = Road.GetTris(i * 4);
            System.Array.Copy(currentTris, 0, tris, i * 6, 6);

            Color[] currentColors = allEdges[i].GetColorCoords();
            System.Array.Copy(currentColors, 0, colors, i * 4, 4);

        }

        CustomSingleMesh mesh = new CustomSingleMesh(verts, tris, colors);
        return mesh;
    }
}