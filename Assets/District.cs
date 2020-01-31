using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class District {
    List<Vector2> boundaryNodes;
    Vector2 centre;

    List<Road> boundaryEdges;

    List<Road> secondaryEdges;

    List<Building> buildings;

    float segmentSize = 10;
    int degree = 3;
    float snapSize = 5;
    float connectivity;
    float proportionalRandomness = 0.25f;
    int startingRoadAmount = 3;

    public District(List<Vector2> points, Vector2 centre) {
        this.boundaryNodes = points;
        this.centre = centre;
        UpdateBoundaryEdges();
        this.secondaryEdges = new List<Road>();
    }

    private void AddBoundaryNode(Road currentBoundaryEdge, Road newEdge) {
        for (int i = 0; i < boundaryNodes.Count; i++) {
            if (boundaryNodes[i] == currentBoundaryEdge.source) {
                boundaryNodes.Insert(i + 1, newEdge.source);
                UpdateBoundaryEdges();
                return;
            }
        }
    }

    private void UpdateBoundaryEdges() {
        boundaryEdges = new List<Road>();
        for (int i = 0; i < boundaryNodes.Count; i++) {
            boundaryEdges.Add(new Road(boundaryNodes[i], boundaryNodes[(i + 1) % boundaryNodes.Count]));
        }
    }

    public void CreateSecondaryRoads() {
        // Road longestEdge = GetLongestEdge();
        Road[] longestEdges = GetLongestEdges(startingRoadAmount);

        // foreach (Road edge in longestEdges) {
        //     Debug.Log(edge);
        // }

        // Road firstRoad = longestEdge.GetPerpendicularRoad(segmentSize + Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add midpoint/rotation deviation

        secondaryEdges = new List<Road>();
        Queue<Road> nodeQueue = new Queue<Road>();

        foreach (Road edge in longestEdges) {
            Road startingRoad = edge.GetPerpendicularRoad(segmentSize + Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add midpoint/rotation deviation
            AddBoundaryNode(edge, startingRoad);
            if (!CheckIntersects(startingRoad)) {
                secondaryEdges.Add(startingRoad);
                nodeQueue.Enqueue(startingRoad);
            }
        }

        // if (!CheckIntersects(firstRoad)) {
        //     secondaryEdges.Add(firstRoad);
        //     nodeQueue.Enqueue(firstRoad);
        // }
        int counter = 0;

        while (nodeQueue.Count != 0 && counter++ < 200) {
            Road currentRoad = nodeQueue.Dequeue();
            // Debug.Log("Popping and spreading edge: : " + currentRoad);
            for (int i = 1; i <= degree; i++) {
                float rotationModifier = i * (360 / (degree + 1)) + Random.Range(-(360 * proportionalRandomness) / (degree + 1), (360 * proportionalRandomness) / (degree + 1)); // TODO - Add rotation deviation.
                Road newEdge = currentRoad.GetSproutedRoad(rotationModifier, segmentSize + Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add segmetn size deviation.

                List<Vector2> inRangeEdges = GetSnapRangeVectors(newEdge, currentRoad);

                Vector2 outputDest = new Vector2(0, 0);
                float snapDist = float.MaxValue;
                foreach (Vector2 node in inRangeEdges) {
                    if (node != currentRoad.source && node != currentRoad.destination) {
                        float srcDist = newEdge.GetPointDistance(node);
                        if (srcDist < snapDist) {
                            snapDist = srcDist;
                            outputDest = node;
                        }
                    }
                }
                bool snapFlag = (snapDist <= snapSize);
                Road snappedRoad = snapFlag ? new Road(newEdge.source, outputDest) : newEdge;

                // if (snapDist <= snapSize) {
                //     Debug.Log("Snapping: From: " + newEdge + " To: " + new Road(newEdge.source, outputDest) + " size: " + snapDist + " count: " + counter);
                //     secondaryEdges.Add(new Road(newEdge.source, outputDest));
                // } else {
                //     secondaryEdges.Add(newEdge);
                //     nodeQueue.Enqueue(newEdge);
                // }

                if (!CheckIntersects(snappedRoad)) {
                    secondaryEdges.Add(snappedRoad);
                    if (!snapFlag) { nodeQueue.Enqueue(snappedRoad); }
                }

            }
        }
        // Debug.Log("\n\n");
        // Debug.Log("\n\n");
        // Debug.Log("Secondary Edges:");
        // foreach (Road edge in secondaryEdges) {
        //     Debug.Log(edge);
        // }
        // Debug.Log("\n\n");
    }

    private bool CheckIntersects(Road newRoad) {
        List<Road> snapRangeEdges = GetSnapRangeEdges(newRoad);
        foreach (Road edge in snapRangeEdges) {
            if (newRoad.LineIntersection(edge)) {
                return true;
            }
        }
        return false;
    }

    private List<Road> GetSnapRangeEdges(Road newRoad) {
        List<Road> output = new List<Road>();
        Road snapBox = newRoad.GetSnapBox(snapSize);
        foreach (Road edge in secondaryEdges) {
            if (newRoad.source != edge.destination && newRoad.destination != edge.source &&
                newRoad.destination != edge.destination && newRoad.source != edge.source &&
                !edge.Equals(newRoad) && snapBox.BoundingBoxOverlap(edge)) {
                output.Add(edge);
            }
        }
        foreach (Road edge in boundaryEdges) {
            if (newRoad.source != edge.destination && newRoad.destination != edge.source &&
                newRoad.destination != edge.destination && newRoad.source != edge.source &&
                !edge.Equals(newRoad) && snapBox.BoundingBoxOverlap(edge)) {
                output.Add(edge);
            }
        }
        return output;
    }

    private List<Vector2> GetSnapRangeVectors(Road newRoad, Road lastRoad) {
        HashSet<Vector2> output = new HashSet<Vector2>();
        HashSet<Vector2> forbiddenPoints = new HashSet<Vector2>();
        forbiddenPoints.Add(newRoad.source);
        forbiddenPoints.Add(newRoad.destination);
        forbiddenPoints.Add(lastRoad.source);
        forbiddenPoints.Add(lastRoad.destination);

        Road snapBox = newRoad.GetSnapBox(snapSize);
        foreach (Road edge in secondaryEdges) {
            if (!forbiddenPoints.Contains(edge.source) && !output.Contains(edge.source) && snapBox.BoundingBoxOverlap(edge.source)) {
                output.Add(edge.source);
            }
            if (!forbiddenPoints.Contains(edge.destination) && !output.Contains(edge.destination) && snapBox.BoundingBoxOverlap(edge.destination)) {
                output.Add(edge.destination);
            }
        }
        foreach (Road edge in boundaryEdges) {
            if (!forbiddenPoints.Contains(edge.source) && !output.Contains(edge.source) && snapBox.BoundingBoxOverlap(edge.source)) {
                output.Add(edge.source);
            }
            if (!forbiddenPoints.Contains(edge.destination) && !output.Contains(edge.destination) && snapBox.BoundingBoxOverlap(edge.destination)) {
                output.Add(edge.destination);
            }
        }
        return output.ToList();
    }

    private Road GetLongestEdge() {
        Road output = boundaryEdges[0];
        foreach (Road edge in boundaryEdges) {
            if (edge.GetLength() > output.GetLength()) {
                output = edge;
            }
        }
        return output;
    }

    private Road[] GetLongestEdges(int num) {
        Road[] output = new Road[num];
        int topOfarray = 0;
        output[topOfarray] = new Road();
        foreach (Road edge in boundaryEdges) {
            double edgeLength = edge.GetLength();
            if (edgeLength > output[topOfarray].GetLength()) {
                int iterator = topOfarray++;
                topOfarray = Mathf.Min(topOfarray, num - 1);
                while (iterator >= 0 && edgeLength > output[iterator].GetLength()) {
                    if (iterator + 1 < num) {
                        output[iterator + 1] = output[iterator];
                    }
                    output[iterator] = edge;
                    iterator--;
                }
            }
        }
        return output;
    }

    public void DrawGizmos() {
        DrawCentre();
        DrawPrimaryRoads();
        DrawSecondaryRoads();
        DrawBuildings();
    }

    private void DrawCentre() {
        Gizmos.color = Color.red;
        if (centre != null) {
            Gizmos.DrawSphere(centre, 1.0f);
        }
    }

    private void DrawPrimaryRoads() {
        Gizmos.color = Color.green;
        if (boundaryEdges != null) {
            foreach (Road edge in boundaryEdges) {
                edge.DrawGizmos();
            }
        }

        if (boundaryNodes != null) {
            foreach (Vector2 node in boundaryNodes) {
                Gizmos.DrawSphere(node, 1.0f);
            }
        }

    }

    private void DrawSecondaryRoads() {
        Gizmos.color = Color.cyan;
        if (secondaryEdges != null) {
            foreach (Road edge in secondaryEdges) {
                edge.DrawGizmos();
            }
        }
    }

    private void DrawBuildings() {
        if (buildings != null) {
            foreach (Building building in buildings) {
                building.DrawGizmos();
            }
        }
    }

    private List<Road> GetAllEdges() {
        List<Road> list = new List<Road>();
        list.AddRange(boundaryEdges);
        list.AddRange(secondaryEdges);
        return list;
    }

    public CustomMesh CreateMesh() {
        CustomMesh roads = CreateRoadMesh();
        CustomMesh buildings = CreateBuildingMesh();
        roads.ConcatMesh(buildings);
        return roads;
    }

    public CustomMesh CreateRoadMesh() {
        Road[] allEdges = GetAllEdges().ToArray();
        Vector3[] verts = new Vector3[allEdges.Length * 4];
        int[] tris = new int[2 * allEdges.Length * 3];
        for (int i = 0; i < allEdges.Length; i++) {
            Vector3[] meshCoords = allEdges[i].GetMeshCoords(1);
            System.Array.Copy(meshCoords, 0, verts, i * 4, 4);
            int[] currentTris = Road.GetTris(i * 4);
            System.Array.Copy(currentTris, 0, tris, i * 6, 6);
        }
        CustomMesh mesh = new CustomMesh(verts, tris);
        return mesh;
    }

    public CustomMesh CreateBuildingMesh() {
        CustomMesh outputMesh = new CustomMesh(new Vector3[0], new int[0]);
        foreach (Building building in buildings) {
            outputMesh.ConcatMesh(building.CreateMesh());
        }
        return outputMesh;
    }

    public Dictionary<Vector2, List<Road>> GetGraph() {
        Dictionary<Vector2, List<Road>> graph = new Dictionary<Vector2, List<Road>>();
        foreach (Road edge in boundaryEdges.Concat(secondaryEdges)) {
            List<Road> edgeList;
            if (graph.TryGetValue(edge.source, out edgeList)) {
                edgeList.Add(edge);
            } else {
                edgeList = new List<Road>();
                edgeList.Add(edge);
                graph.Add(edge.source, edgeList);
            }

            if (graph.TryGetValue(edge.destination, out edgeList)) {
                edgeList.Add(edge.GetInverse());
            } else {
                edgeList = new List<Road>();
                edgeList.Add(edge.GetInverse());
                graph.Add(edge.destination, edgeList);
            }
        }
        return graph;
    }

    public List<Building> CreateBuildings() {
        List<Building> output = new List<Building>();
        Dictionary<Vector2, List<Road>> graph = GetGraph();
        HashSet<Road> exploredEdges = new HashSet<Road>();

        Vector2 startNode = GetLeftMostNode(graph);

        LogOutsideEdges(graph, exploredEdges, startNode);

        int counter = exploredEdges.Count;

        Road startEdge = GetStartEdge(graph, startNode);

        Queue<Road> nextEdges = new Queue<Road>();
        nextEdges.Enqueue(startEdge);

        while (nextEdges.Count > 0) {
            Road currentEdge = nextEdges.Dequeue();
            if (exploredEdges.Contains(currentEdge)) { continue; }

            // Debug.Log("Opening new circuit " + currentEdge + " set size: " + exploredEdges.Count);

            List<Road> currentBuildingList = new List<Road>();
            Vector2 buildingStartNode = currentEdge.source;

            do {
                exploredEdges.Add(currentEdge);
                counter++;
                currentBuildingList.Add(currentEdge);
                List<Road> sproutingEdges;
                graph.TryGetValue(currentEdge.destination, out sproutingEdges);
                foreach (Road edge in sproutingEdges) {
                    if (!exploredEdges.Contains(edge)) {
                        nextEdges.Enqueue(edge);
                    }
                }
                currentEdge = currentEdge.GetMostClockWise(sproutingEdges);
            } while (currentEdge.source != buildingStartNode);

            output.Add(new Building(currentBuildingList));
            // Debug.Log("Building Edge Num: " + currentBuildingList.Count + "Queue Size " + nextEdges.Count);

        }
        int totalEdgeNumber = boundaryEdges.Count + secondaryEdges.Count;
        Debug.Log("Number of edges resolved: " + counter + " Total Number Of Edges: " + 2 * totalEdgeNumber);

        buildings = output;
        return output;
    }

    public Road GetStartEdge(Dictionary<Vector2, List<Road>> graph, Vector2 startNode) {
        List<Road> sproutingEdges;
        graph.TryGetValue(startNode, out sproutingEdges);
        return new Road(new Vector2(startNode.x - 1, startNode.y), startNode).GetLeastClockWise(sproutingEdges);
    }

    public void LogOutsideEdges(Dictionary<Vector2, List<Road>> graph, HashSet<Road> exploredEdges, Vector2 startNode) {
        List<Road> sproutingEdges;
        graph.TryGetValue(startNode, out sproutingEdges);
        Road currentEdge = new Road(new Vector2(startNode.x - 1, startNode.y), startNode).GetMostClockWise(sproutingEdges);
        exploredEdges.Add(currentEdge);

        graph.TryGetValue(currentEdge.destination, out sproutingEdges);

        while (currentEdge.destination != startNode) {
            currentEdge = currentEdge.GetMostClockWise(sproutingEdges);

            exploredEdges.Add(currentEdge);

            graph.TryGetValue(currentEdge.destination, out sproutingEdges);
        }
    }

    public Vector2 GetLeftMostNode(Dictionary<Vector2, List<Road>> graph) {
        Vector2 leftMost = boundaryEdges[0].source;
        foreach (Vector2 node in graph.Keys) {
            if (node.x < leftMost.x) {
                leftMost = node;
            }
        }
        return leftMost;
    }
}