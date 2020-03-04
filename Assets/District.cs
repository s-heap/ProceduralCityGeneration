using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]

public class District : IComparable<District> {
    List<Vector2> boundaryNodes;
    public Vector2 centre;

    List<Road> boundaryEdges;

    List<Road> secondaryEdges;

    List<Building> buildings;

    private CityConstants constants;

    float segmentSize = 30;
    int degree = 3;
    float snapSize = 15;
    float connectivity;
    float proportionalRandomness = 0.1f;
    int startingRoadAmount = 3;

    public float population;

    public District(List<Vector2> points, Vector2 centre, CityConstants constants) {
        this.constants = constants;

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

        // Road firstRoad = longestEdge.GetPerpendicularRoad(segmentSize + UnityEngine.Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add midpoint/rotation deviation

        secondaryEdges = new List<Road>();
        Queue<Road> nodeQueue = new Queue<Road>();

        foreach (Road edge in longestEdges) {
            Road startingRoad = edge.GetPerpendicularRoad(segmentSize + UnityEngine.Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add midpoint/rotation deviation
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
        // int counter = 0;

        int count = 0;

        while (nodeQueue.Count != 0) {
            if (count++ > 2000) {
                Debug.Log("INFINITE DISTRICT GENERATION: SECONDARY ROAD GEN");
                return;
            }
            Road currentRoad = nodeQueue.Dequeue();
            // Debug.Log("Popping and spreading edge: : " + currentRoad);
            for (int i = 1; i <= degree; i++) {
                float rotationModifier = i * (360 / (degree + 1)) + UnityEngine.Random.Range(-(360 * proportionalRandomness) / (degree + 1), (360 * proportionalRandomness) / (degree + 1)); // TODO - Add rotation deviation.
                Road newEdge = currentRoad.GetSproutedRoad(rotationModifier, segmentSize + UnityEngine.Random.Range(-proportionalRandomness * segmentSize, proportionalRandomness * segmentSize)); // TODO - Add segmetn size deviation.

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
                int count = 0;
                while (iterator >= 0 && edgeLength > output[iterator].GetLength()) {
                    if (count++ > 2000) {
                        Debug.Log("INFINITE LOOP DISTRICT GENERATION: FINDING LONGEST EDGES");
                        return null;
                    }
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

    // private List<Road> GetAllEdges() {
    //     // List<Road> list = new List<Road>();
    //     // list.AddRange(boundaryEdges);
    //     // list.AddRange(secondaryEdges);
    //     // return list;
    //     HashSet<Road> roadSet = GetGraph().storedEdges;
    //     foreach (Road edge in boundaryEdges) {
    //         roadSet.Remove(edge);
    //     }
    //     return roadSet.ToList();
    // }

    // public CustomMesh CreateMesh(bool flattenBuildings = false) {
    //     CustomMesh roads = CreateRoadMesh();
    //     CustomMesh buildings = CreateBuildingMesh(flattenBuildings);
    //     roads.ConcatMesh(buildings);
    //     return roads;
    // }

    public CustomSingleMesh CreateRoadMesh() {
        return GetGraph().CreateRoadMesh(constants, boundaryEdges);
    }

    public CustomSingleMesh CreateBuildingMesh(bool flattenBuildings = false) {
        CustomSingleMesh outputMesh = new CustomSingleMesh();
        if (buildings != null) {
            foreach (Building building in buildings) {
                outputMesh.ConcatMesh(building.CreateMesh(flattenBuildings));
            }
        }
        return outputMesh;
    }

    public RoadGraph GetGraph() {
        RoadGraph graph = new RoadGraph();
        graph.AddEdgeList(boundaryEdges);
        graph.AddEdgeList(secondaryEdges);
        return graph;
    }

    public List<Building> CreateBuildings() {
        List<Building> output = new List<Building>();
        RoadGraph graph = GetGraph();
        HashSet<Road> exploredEdges = new HashSet<Road>();
        HashSet<Road> addedToQueue = new HashSet<Road>();

        // ClearConsole();
        // Debug.Log("Creating Buildings");

        // Debug.Log("Boundary Edges:");
        // foreach (Road edge in boundaryEdges) {
        //     Debug.Log(edge);

        // }

        // Debug.Log("Secondary Edges:");
        // foreach (Road edge in secondaryEdges) {
        //     Debug.Log(edge);

        // }

        // Debug.Log("\n\n\n");
        // Debug.Log("\n\n\n");
        // Debug.Log("\n\n\n");
        // Debug.Log("\n\n\n");

        Vector2 startNode = graph.GetLeftMostNode();

        // Debug.Log("Start Node: " + startNode);

        LogOutsideEdges(graph, exploredEdges, startNode);

        // Debug.Log("Num outside Edges Logged: " + exploredEdges.Count);

        int counter = exploredEdges.Count;

        Road startEdge = GetStartEdge(graph, startNode);

        // Debug.Log("Start Edge: " + startEdge);

        Queue<Road> nextEdges = new Queue<Road>();
        nextEdges.Enqueue(startEdge);

        int outerCounter = 0;

        while (nextEdges.Count > 0) {

            if (outerCounter++ > 100000) {
                Debug.Log("INFINITE(100000 iterations) LOOP MINIMAL CYCLE: OUTER FAILURE");
                buildings = output;
                return null;
            }

            Road currentEdge = nextEdges.Dequeue();

            if (exploredEdges.Contains(currentEdge)) { continue; }

            // Debug.Log("Resolving Queue Entry: " + currentEdge);

            List<Road> currentBuildingList = new List<Road>();
            Vector2 buildingStartNode = currentEdge.source;

            int innerCounter = 0;

            do {
                // Debug.Log("    Resolving Current Edge: " + currentEdge);
                exploredEdges.Add(currentEdge);
                counter++;
                currentBuildingList.Add(currentEdge);
                List<Road> sproutingEdges = graph.GetRoadList(currentEdge.destination);
                foreach (Road edge in sproutingEdges) {
                    if (!exploredEdges.Contains(edge)) {
                        if (!addedToQueue.Contains(edge)) {
                            nextEdges.Enqueue(edge);
                            addedToQueue.Add(edge);
                            // Debug.Log("        Adding To Queue: " + edge + " Angle: " + currentEdge.GetAngleAntiClockWise(edge));
                        }

                    }
                }
                if (innerCounter++ > 2000) {
                    Debug.Log("INFINITE LOOP MINIMAL CYCLE: INNER FAILURE");
                    buildings = output;
                    return null;
                }
                currentEdge = currentEdge.GetMostClockWise(sproutingEdges);

            }
            while (currentEdge.source != buildingStartNode);

            // Debug.Log("    Start Returned To, Logging Building With : " + currentBuildingList.Count + " Edges");
            output.Add(new Building(currentBuildingList, constants, constants.roadWidth));
            // Debug.Log("Building Edge Num: " + currentBuildingList.Count + "Queue Size " + nextEdges.Count);

        }
        int totalEdgeNumber = boundaryEdges.Count + secondaryEdges.Count;
        // Debug.Log("Number of edges resolved: " + counter + " Total Number Of Edges: " + 2 * totalEdgeNumber);

        buildings = output;
        return output;
    }

    public Road GetStartEdge(RoadGraph graph, Vector2 startNode) {
        List<Road> sproutingEdges = graph.GetRoadList(startNode);

        return new Road(new Vector2(startNode.x - 1, startNode.y), startNode).GetLeastClockWise(sproutingEdges);
    }

    public void LogOutsideEdges(RoadGraph graph, HashSet<Road> exploredEdges, Vector2 startNode) {
        List<Road> sproutingEdges = graph.GetRoadList(startNode);
        Road currentEdge = new Road(new Vector2(startNode.x - 1, startNode.y), startNode).GetMostClockWise(sproutingEdges);
        exploredEdges.Add(currentEdge);

        sproutingEdges = graph.GetRoadList(currentEdge.destination);

        int count = 0;
        while (currentEdge.destination != startNode) {
            if (count++ > 2000) {
                Debug.Log("INFINITE LOOP DISTRICT GENERATION: LOGGING OUTSIDE EDGES IN MINIMAL CYCLE FINDER");
                return;
            }

            currentEdge = currentEdge.GetMostClockWise(sproutingEdges);

            exploredEdges.Add(currentEdge);

            sproutingEdges = graph.GetRoadList(currentEdge.destination);
        }
    }

    public int CompareTo(District other) {
        float thisLength = constants.GetDistanceToCentre(centre);
        float otherLength = constants.GetDistanceToCentre(other.centre);
        return thisLength.CompareTo(otherLength);
    }

    public float GetPopValue() {
        return constants.GetPopNoiseValue(centre);
    }

    public float GetBusValue() {
        return constants.GetBusNoiseValue(centre);
    }

}