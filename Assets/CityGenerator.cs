using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CityGenerator : MonoBehaviour {
	[SerializeField][Range(10, 500)] private int m_pointCount = 50;
	[SerializeField] private float m_mapWidth = 1000;
	[SerializeField] private float m_mapHeight = 1000;

	private List<District> districts;

	void Awake() {
		// Debug.Log((360 - Vector2.SignedAngle(new Vector2(0, 1), new Vector2(1, 1))) % 360);
		// Debug.Log((360 - Vector2.SignedAngle(new Vector2(0, 1), new Vector2(-1, 1))) % 360);
		// Road one = new Road(new Vector2(35, 50), new Vector2(30, 50));
		// Debug.Log(one.GetPointDistance(new Vector2(40f, 50f)));

		// Road testing = new Road(new Vector2(37.8f, 51.8f), new Vector2(27.9f, 52.8f));

		// Debug.Log(testing.GetPointDistance(new Vector2(57.4f, 49.4f)));

		// HashSet<Vector2> hashSet = new HashSet<Vector2>();
		// hashSet.Add(new Vector2(0, 0));
		// hashSet.Add(new Vector2(1, 0));
		// hashSet.Add(new Vector2(0, 0));
		// hashSet.Add(new Vector2(1, 1));
		// hashSet.Add(new Vector2(1, 0));
		// foreach (Vector2 vector in hashSet.ToList()) {
		// 	// Debug.Log(vector);
		// }

		// Road one = new Road(new Vector2(0, 0), new Vector2(10, 10));
		// Road two = new Road(new Vector2(0, 10), new Vector2(0, 0));
		// Road three = new Road(new Vector2(0, 10), new Vector2(10, 10));
		// Debug.Log(one.LineIntersection(two));
		// Debug.Log(one.LineIntersection(three));

		// Vector3[] a = new Vector3[2];
		// a[0] = new Vector3(0, 0, 0);
		// a[1] = new Vector3(1, 1, 1);
		// int[] b = new int[2];
		// b[0] = 0;
		// b[1] = 1;
		// CustomMesh finalMesh = new CustomMesh(a, b);
		// finalMesh.ConcatMesh(new CustomMesh(a, b));
		// finalMesh.ConcatMesh(new CustomMesh(a, finalMesh.tris));
		// foreach (Vector3 i in finalMesh.verts) {
		// 	Debug.Log(i);
		// }
		// foreach (int i in finalMesh.tris) {
		// 	Debug.Log(i);
		// }

		// List<Vector2> points = new List<Vector2>();
		// points.Add(new Vector2(-1, -1));
		// points.Add(new Vector2(-1, 1));
		// points.Add(new Vector2(1, 1));
		// points.Add(new Vector2(1, -1));
		// District test = new District(points, new Vector2(0, 0));

		// List<List<Road>> buildings = test.GetBuildingLocations();
		// Debug.Log(buildings.Count);

		// foreach (List<Road> building in buildings) {
		// 	foreach (Road road in building) {
		// 		Debug.Log(road);
		// 	}
		// }

		// Dictionary<Vector2, List<Road>> graph = test.GetGraph();

		// foreach (Vector2 point in graph.Keys) {
		// 	Debug.Log(point);
		// }
		// List<Road> temp;
		// if (graph.TryGetValue(graph.Keys.ElementAt(0), out temp)) {
		// 	foreach (Road edge in temp) {
		// 		Debug.Log(edge);
		// 	}
		// }

		// Road temp = new Road(new Vector2(0, 0), new Vector2(1, 1));
		// Road temp2 = new Road(new Vector2(0, 0), new Vector2(1, 1));
		// Road tempInverse = new Road(new Vector2(1, 1), new Vector2(0, 0));

		// HashSet<Road> exploredEdges = new HashSet<Road>();
		// exploredEdges.Add(temp);

		// if (exploredEdges.Contains(temp2)) {
		// 	Debug.Log("Search successfull");
		// } else {
		// 	Debug.Log("Search unsuccessfull");
		// }

		// if (temp == temp2) {
		// 	Debug.Log("Search successfull");
		// } else {
		// 	Debug.Log("Search unsuccessfull");
		// }

		// if (temp.Equals(temp2)) {
		// 	Debug.Log("Search successfull");
		// } else {
		// 	Debug.Log("Search unsuccessfull");
		// }

		Road temp = new Road(new Vector2(0, 0), new Vector2(0, 1));
		Road temp2 = new Road(new Vector2(0, 1), new Vector2(1, 1));
		Debug.Log(temp.GetReverseRoadBearing());
		Debug.Log(temp2.GetReverseRoadBearing());
		Debug.Log(temp.GetAngleAntiClockWise(temp2));
		Debug.Log(temp.GetBisector(temp2, 1));

		// Road test = new Road(new Vector2(0, 0), new Vector2(0, 1));
		// Road test2 = new Road(new Vector2(0, 1), new Vector2(0, 0));
		// Debug.Log(test.GetAngleAntiClockWise(test2));

		Build();
	}

	void Update() {
		if (Input.GetMouseButtonDown(2)) {
			Build();
		}
		// if (Input.GetMouseButtonDown(0)) {
		// 	currentBuilding = (currentBuilding + 1) % buildings.Count;
		// 	Debug.Log(currentBuilding + " out of " + buildings.Count);
		// }
	}

	private void Build() {
		do { districts = CreateDistricts(); } while (districts.Count < 1);

		CustomMesh finalMesh = new CustomMesh(new Vector3[0], new int[0]);

		foreach (District district in districts) {
			district.CreateSecondaryRoads();
			district.CreateBuildings();
			finalMesh.ConcatMesh(district.CreateMesh());
		}

		Mesh outputMesh = finalMesh.GetMesh();
		outputMesh.RecalculateNormals();

		GetComponent<MeshFilter>().mesh = outputMesh;
	}

	private List<District> CreateDistricts() {
		Delaunay.Voronoi v = GetVoronoi();

		List<District> newDistricts = new List<District>();

		foreach (Vector2 districtHubPoint in v.SiteCoords()) {
			List<Vector2> districtBoundaryPoints = v.Region(districtHubPoint);
			if (!IsOnBorder(districtBoundaryPoints)) {
				newDistricts.Add(new District(districtBoundaryPoints, districtHubPoint));
			}
		}
		return newDistricts;
	}

	private bool IsOnBorder(List<Vector2> points) {
		foreach (Vector2 point in points) {
			if (point.x == 0 || point.x == m_mapWidth || point.y == 0 || point.y == m_mapHeight) {
				return true;
			}
		}
		return false;
	}

	private Delaunay.Voronoi GetVoronoi() {
		List<uint> colors = new List<uint>();
		List<Vector2> m_points = new List<Vector2>();

		for (int i = 0; i < m_pointCount; i++) {
			colors.Add(0);
			m_points.Add(new Vector2(
				UnityEngine.Random.Range(0, m_mapWidth),
				UnityEngine.Random.Range(0, m_mapHeight)));
		}
		return new Delaunay.Voronoi(m_points, colors, new Rect(0, 0, m_mapWidth, m_mapHeight));
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
		Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, m_mapHeight));
		Gizmos.DrawLine(new Vector2(0, 0), new Vector2(m_mapWidth, 0));
		Gizmos.DrawLine(new Vector2(m_mapWidth, 0), new Vector2(m_mapWidth, m_mapHeight));
		Gizmos.DrawLine(new Vector2(0, m_mapHeight), new Vector2(m_mapWidth, m_mapHeight));
	}
}