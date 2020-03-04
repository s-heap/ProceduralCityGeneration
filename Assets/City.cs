using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class City {
    public CityConstants constants;

    public List<District> districts;

    public CustomCityMesh cityMesh;
    public CustomCityMesh flatCityMesh;

    private int totalPopulation;

    private List<float> popValues;
    private float popValueTotal;
    private List<float> busValues;
    private float busValueTotal;

    public City(int m_pointCount, float m_mapWidth, float m_mapHeight, int population) {
        // Debug.Log("Total input population found to be " + population);

        Vector2 populationPerlinShift = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));
        Vector2 businessPerlinShift = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));
        totalPopulation = population;
        constants = new CityConstants(m_pointCount, m_mapWidth, m_mapHeight, populationPerlinShift, businessPerlinShift);

        Build();
        PreCalculateMeshes();

        resetLightingAndCamera();

        popValues = new List<float>();
        busValues = new List<float>();
        foreach (District district in districts) {
            popValues.Add(district.GetPopValue());
            busValues.Add(district.GetBusValue());
        }
        popValueTotal = popValues.Sum();
        busValueTotal = busValues.Sum();

        for (int i = 0; i < districts.Count; i++) {
            districts[i].population = popValues[i] / popValueTotal * totalPopulation;
        }

        CalculateStressNetwork();
    }

    public void CalculateStressNetwork() {
        RoadGraph graph = GetGraph();
        CustomSingleMesh mesh = graph.CreateRoadMesh(constants);
        cityMesh.AddStressNetwork(mesh);
        CustomSingleMesh flatMesh = graph.CreateRoadMesh(constants);
        flatCityMesh.AddStressNetwork(flatMesh);
    }

    public void resetLightingAndCamera() {
        Vector2 centreOfMap = constants.GetCentre();
        GameObject.Find("Main Camera").transform.position = new Vector3(centreOfMap.x + constants.GetMaxDimension() / 2, constants.GetMaxDimension() / 2, centreOfMap.y + constants.GetMaxDimension() / 2);
        GameObject.Find("Main Camera").transform.LookAt(new Vector3(centreOfMap.x, 0, centreOfMap.y));

        GameObject lightGameObject = GameObject.Find("Point Light");
        Light lightComp = lightGameObject.GetComponent<Light>();
        lightComp.range = constants.GetMaxDimension();
        lightGameObject.transform.position = new Vector3(centreOfMap.x, constants.GetMaxDimension() / 2, centreOfMap.y);

    }

    private void Build() {
        int count = 0;
        do {
            districts = CreateConnectedDistricts();
            if (count++ > 2000) {
                Debug.Log("INFINITE LOOP CITY GENERATION: NON EMPTY DISTRICT LIST");
                return;
            }
        } while (districts.Count < 1);

        foreach (District district in districts) {
            district.CreateSecondaryRoads();
            district.CreateBuildings();
        }
        districts.Sort();
    }

    public void PreCalculateMeshes() {
        cityMesh = new CustomCityMesh();
        flatCityMesh = new CustomCityMesh();
        foreach (District district in districts) {
            cityMesh.AddRoadMesh(district.CreateRoadMesh());
            cityMesh.AddBuildingMesh(district.CreateBuildingMesh(false));

            flatCityMesh.AddRoadMesh(district.CreateRoadMesh());
            flatCityMesh.AddBuildingMesh(district.CreateBuildingMesh(true));
        }
    }

    public Mesh GetCityMesh(int inputDistrictNum, bool flattenBuildings = false, bool showStressMap = false) {
        int numDistricts = Mathf.Max(1, Mathf.Min(inputDistrictNum, districts.Count));

        return flattenBuildings ? flatCityMesh.GetMesh(numDistricts, showStressMap) : cityMesh.GetMesh(numDistricts, showStressMap);
    }

    private List<District> CreateConnectedDistricts() {
        List<District> outputDistricts = CreateDistricts();
        while (!DistrictsConnected(outputDistricts)) {
            outputDistricts = CreateDistricts();
        }
        return outputDistricts;
    }

    private bool DistrictsConnected(List<District> inputDistricts) {
        RoadGraph graph = GetGraph(inputDistricts);
        HashSet<Vector2> visitedNodes = new HashSet<Vector2>();
        Vector2 startNode = graph.GetLeftMostNode();
        visitedNodes.Add(startNode);

        Queue<Vector2> nodeQueue = new Queue<Vector2>();
        nodeQueue.Enqueue(startNode);

        while (nodeQueue.Count > 0) {
            Vector2 currentNode = nodeQueue.Dequeue();
            visitedNodes.Add(currentNode);
            List<Road> attachedRoads = graph.GetRoadList(currentNode);
            foreach (Road edge in attachedRoads) {
                if (!visitedNodes.Contains(edge.destination)) {
                    nodeQueue.Enqueue(edge.destination);
                }
            }
        }

        HashSet<Vector2> totalNodes = new HashSet<Vector2>(graph.graph.Keys);
        return totalNodes.SetEquals(visitedNodes);
    }

    private List<District> CreateDistricts() {
        List<Vector2> m_points = new List<Vector2>();
        List<uint> colors = new List<uint>();

        for (int i = 0; i < constants.GetPointCount(); i++) {
            colors.Add(0);
            m_points.Add(new Vector2(
                Random.Range(0, constants.GetMapWidth()),
                Random.Range(0, constants.GetMapHeight())));
        }

        Delaunay.Voronoi v = new Delaunay.Voronoi(m_points, colors, new Rect(0, 0, constants.GetMapWidth(), constants.GetMapHeight()));
        List<District> newDistricts = GetValidDistricts(v);

        while (newDistricts.Count < constants.GetPointCount()) {
            colors.Add(0);
            m_points.Add(new Vector2(
                Random.Range(0, constants.GetMapWidth()),
                Random.Range(0, constants.GetMapHeight())));
            v = new Delaunay.Voronoi(m_points, colors, new Rect(0, 0, constants.GetMapWidth(), constants.GetMapHeight()));
            newDistricts = GetValidDistricts(v);
        }
        return newDistricts;
    }

    private List<District> GetValidDistricts(Delaunay.Voronoi v) {
        List<District> newDistricts = new List<District>();

        foreach (Vector2 districtHubPoint in v.SiteCoords()) {
            List<Vector2> districtBoundaryPoints = v.Region(districtHubPoint);
            if (!IsOnBorder(districtBoundaryPoints)) {
                newDistricts.Add(new District(districtBoundaryPoints, districtHubPoint, constants));
            }
        }
        return newDistricts;
    }

    private bool IsOnBorder(List<Vector2> points) {
        foreach (Vector2 point in points) {
            if (point.x == 0 || point.x == constants.GetMapWidth() || point.y == 0 || point.y == constants.GetMapHeight()) {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos() {
        // if (districts != null) {
        // 	foreach (District district in districts) {
        // 		district.DrawGizmos();
        // 	}
        // }

        // Gizmos.color = Color.red;
        // if (buildings != null) {
        // 	Building temp = new Building(buildings[currentBuilding]);
        // 	temp.DrawGizmos();
        // }

        // DrawBox();
    }

    void DrawBox() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, constants.GetMapHeight()));
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(constants.GetMapWidth(), 0));
        Gizmos.DrawLine(new Vector2(constants.GetMapWidth(), 0), new Vector2(constants.GetMapWidth(), constants.GetMapHeight()));
        Gizmos.DrawLine(new Vector2(0, constants.GetMapHeight()), new Vector2(constants.GetMapWidth(), constants.GetMapHeight()));
    }

    RoadGraph GetGraph(List<District> inputDistricts) {
        RoadGraph output = new RoadGraph();
        foreach (District district in inputDistricts) {
            output.AddGraph(district.GetGraph());
        }
        return output;
    }

    RoadGraph GetGraph() {
        RoadGraph output = new RoadGraph();
        foreach (District district in districts) {
            output.AddGraph(district.GetGraph());
        }
        return output;
    }

    public int GetNumDistricts() {
        return districts.Count;
    }

}