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

	City currrentCity;

	bool flattenBuildings = false;
	int numToView = 1;

	void Awake() {

		// RoadGraph graph = new RoadGraph();

		// Road test1 = new Road(new Vector2(0, 0), new Vector2(0, 10));
		// Road test2 = new Road(new Vector2(0, 0), new Vector2(0, 5));

		// graph.SmartAddEdge(test2);
		// graph.Print();

		// graph.SmartAddEdge(test1);
		// graph.Print();

		// Road test3 = new Road(new Vector2(-5, 5), new Vector2(0, 5));
		// Debug.Log(test2.LineIntersection(test1));
		// Debug.Log(test1.GetAngleAntiClockWise(test2));
		// Debug.Log(test1.LineIntersection(test3));
		// Debug.Log(test1.GetGradient());
		// Debug.Log(test2.GetGradient());
		// Debug.Log(test2.AreParallel(test1));
		// Debug.Log(test1.AreParallel(test2));
		// Debug.Log(test1.AreParallel(test2.GetInverse()));

		Road test1 = new Road(new Vector2(0, 0), new Vector2(0, 20));
		Road test2 = new Road(new Vector2(0, 20), new Vector2(20, 20));
		Road test3 = new Road(new Vector2(20, 20), new Vector2(20, 0));
		Road test4 = new Road(new Vector2(20, 0), new Vector2(0, 0));
		List<Road> input = new List<Road>();
		input.Add(test1);
		input.Add(test2);
		input.Add(test3);
		input.Add(test4);
		Building currentBuilding = new Building(input, new CityConstants(100, 1000, 1000, new Vector2(0, 0), new Vector2(0, 0)));

		var cheese = input.GetRange(0, 2);
		Debug.Log(cheese.Count);

		currrentCity = new City(m_pointCount, m_mapWidth, m_mapHeight);
		GetComponent<MeshFilter>().mesh = currrentCity.CreateMesh(numToView, flattenBuildings);
	}

	void Update() {
		if (Input.GetMouseButtonDown(2)) {
			currrentCity = new City(m_pointCount, m_mapWidth, m_mapHeight);
			GetComponent<MeshFilter>().mesh = currrentCity.CreateMesh(numToView, flattenBuildings);
		}

		if (Input.GetMouseButtonDown(1)) {
			flattenBuildings = !flattenBuildings;
			GetComponent<MeshFilter>().mesh = currrentCity.CreateMesh(numToView, flattenBuildings);
		}

		if (Input.GetKey("=")) {
			numToView += 1;
			Debug.Log(numToView);
			GetComponent<MeshFilter>().mesh = currrentCity.CreateMesh(numToView, flattenBuildings);

		}
		if (Input.GetKey("-")) {
			numToView -= 1;
			Debug.Log(numToView);
			GetComponent<MeshFilter>().mesh = currrentCity.CreateMesh(numToView, flattenBuildings);
		}
	}

}