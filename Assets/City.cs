using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class City {
    private CityConstants constants;

    private List<District> districts;

    public Mesh cityMesh;
    public Mesh[] cityMeshs;
    public Mesh flatCityMesh;
    public Mesh[] flatCityMeshs;

    public City(int m_pointCount, float m_mapWidth, float m_mapHeight) {
        Vector2 populationPerlinShift = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));
        Vector2 businessPerlinShift = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));

        constants = new CityConstants(m_pointCount, m_mapWidth, m_mapHeight, populationPerlinShift, businessPerlinShift);

        Vector2 centreOfMap = constants.GetCentre();
        GameObject.Find("Main Camera").transform.position = new Vector3(centreOfMap.x, 100, centreOfMap.y);

        Build();
        // PreCalculateMeshes();
    }

    private void Build() {
        int count = 0;
        do {
            districts = CreateDistricts();
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
        cityMeshs = new Mesh[districts.Count];
        flatCityMeshs = new Mesh[districts.Count];
    }

    // public void PreCalculateMeshes() {
    //     cityMesh = CreateMesh();
    //     flatCityMesh = CreateMesh(true);
    // }

    public Mesh CreateMesh(int inputDistrictNum, bool flattenBuildings = false) {
        int numDistricts = Mathf.Max(1, Mathf.Min(inputDistrictNum, districts.Count - 1));
        if (flattenBuildings) {
            if (flatCityMeshs[numDistricts] == null) {
                CustomMesh newMesh = new CustomMesh(new Vector3[0], new int[0]);
                foreach (District district in districts.GetRange(0, numDistricts)) {
                    newMesh.ConcatMesh(district.CreateMesh(flattenBuildings));
                }
                Mesh meshToSave = newMesh.GetMesh(constants);
                meshToSave.RecalculateNormals();
                flatCityMeshs[numDistricts] = meshToSave;
            }
            return flatCityMeshs[numDistricts];
        } else {
            if (cityMeshs[numDistricts] == null) {
                CustomMesh newMesh = new CustomMesh(new Vector3[0], new int[0]);
                foreach (District district in districts.GetRange(0, numDistricts)) {
                    newMesh.ConcatMesh(district.CreateMesh(flattenBuildings));
                }
                Mesh meshToSave = newMesh.GetMesh(constants);
                meshToSave.RecalculateNormals();
                cityMeshs[numDistricts] = meshToSave;
            }
            return cityMeshs[numDistricts];
        }

        // // CustomMesh finalMesh = new CustomMesh(new Vector3[0], new int[0]);

        // foreach (District district in districts) {
        //     finalMesh.ConcatMesh(district.CreateMesh(flattenBuildings));
        // }
        // Mesh outputMesh = finalMesh.GetMesh(constants);
        // outputMesh.RecalculateNormals();
        // return outputMesh;
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

    RoadGraph GetGraph() {
        RoadGraph output = new RoadGraph();
        foreach (District district in districts) {
            output.AddGraph(district.GetGraph());
        }
        return output;
    }

}