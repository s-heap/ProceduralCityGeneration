using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CityGenerator : MonoBehaviour {
	[SerializeField][Range(10, 10000)] private int m_pointCount = 50;
	[SerializeField] private float m_mapWidth = 1000;
	[SerializeField] private float m_mapHeight = 1000;
	[SerializeField] private int m_numToView = 1;

	City currrentCity;

	bool flattenBuildings = true;
	bool showStressMap = false;

	List<GameObject> markers;

	string filepath = "Resources/savedCity.CityObj";

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

		// Road test1 = new Road(new Vector2(0, 0), new Vector2(0, 20));
		// Road test2 = new Road(new Vector2(0, 0), new Vector2(0, 20));

		// Debug.Log(test1 == test2);
		// Debug.Log(test1.Equals(test2));

		// test2.stress += 10;

		// Debug.Log(test1 == test2);
		// Debug.Log(test1.Equals(test2));

		// Road test3 = new Road(new Vector2(20, 20), new Vector2(20, 0));
		// Road test4 = new Road(new Vector2(20, 0), new Vector2(0, 0));
		// List<Road> input = new List<Road>();
		// input.Add(test1);
		// input.Add(test2);
		// input.Add(test3);
		// input.Add(test4);
		// Building currentBuilding = new Building(input, new CityConstants(100, 1000, 1000, new Vector2(0, 0), new Vector2(0, 0)));

		// var cheese = input.GetRange(0, 2);
		// Debug.Log(cheese.Count);
		// Road

		markers = new List<GameObject>();
		StartCoroutine(CreateNewCity());

	}

	void Update() {
		if (Input.GetMouseButtonDown(2)) {
			StartCoroutine(CreateNewCity());
		}

		if (Input.GetMouseButtonDown(1)) {
			flattenBuildings = !flattenBuildings;
			RenderMesh();
		}

		if (Input.GetKey("=") || Input.GetAxis("Mouse ScrollWheel") > 0f) {
			IncreaseDistricts();
		}
		if (Input.GetKey("-") || Input.GetAxis("Mouse ScrollWheel") < 0f) {
			DecreaseDistricts();
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			CreateMarkers();
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			DestroyMarkers();
		}

		if (Input.GetKeyDown(KeyCode.K)) {
			Debug.Log("Saving Current City");
			Serializer.Save<City>(filepath, currrentCity);
			Debug.Log("City Saved!");
		}

		if (Input.GetKeyDown(KeyCode.L)) {
			Debug.Log("Loading City");
			currrentCity = Serializer.Load<City>(filepath);
			Debug.Log("City Successfully Loaded!");
			currrentCity.resetLightingAndCamera();
			m_numToView = Mathf.Min(m_numToView, currrentCity.GetNumDistricts());
			DestroyMarkers();
			CreateMarkers();
			RenderMesh();
		}

		if (Input.GetKeyDown(KeyCode.M)) {
			showStressMap = !showStressMap;
			RenderMesh();
		}
	}

	private void DestroyMarkers() {
		foreach (GameObject marker in markers) {
			Destroy(marker.gameObject);
		}
		markers = new List<GameObject>();
	}

	private void CreateMarkers() {
		if (markers.Count == 0) {
			foreach (District district in currrentCity.districts) {
				CreateMarker(district.centre);
			}
		}
	}

	IEnumerator CreateNewCity() {
		DestroyMarkers();

		currrentCity = new City(m_pointCount, m_mapWidth, m_mapHeight, (int) (4329 * m_mapWidth / 1000 * m_mapHeight / 1000));
		m_numToView = 1;
		RenderMesh();
		while (m_numToView < currrentCity.GetNumDistricts()) {
			yield return new WaitForSeconds(0.01f);
			IncreaseDistricts();
		}
		foreach (District district in currrentCity.districts) {
			CreateMarker(district.centre);
		}
	}

	void IncreaseDistricts() {
		m_numToView = Mathf.Min(m_numToView + 1, currrentCity.GetNumDistricts());
		RenderMesh();
	}

	void DecreaseDistricts() {
		m_numToView = Mathf.Max(m_numToView - 1, 1);
		RenderMesh();
	}

	void RenderMesh() {
		if (showStressMap) {
			GetComponent<Renderer>().materials = Enumerable.Repeat(new Material(Shader.Find("Custom/VertexColoredDiffuse")), 1).ToArray();
		} else {
			GetComponent<Renderer>().materials = Enumerable.Repeat(new Material(Shader.Find("Custom/VertexColoredDiffuse")), 2 * Mathf.Max(m_numToView, 1)).ToArray();
		}
		GetComponent<MeshFilter>().mesh = currrentCity.GetCityMesh(m_numToView, flattenBuildings, showStressMap);
	}

	void CreateMarker(Vector2 location) {
		GameObject marker = new GameObject("Marker");
		markers.Add(marker);
		marker.AddComponent<MeshFilter>();
		marker.AddComponent<MeshRenderer>();

		var mesh = new Mesh();
		marker.GetComponent<MeshFilter>().mesh = mesh;
		Vector3 p0 = new Vector3(-0.5f, 0, -Mathf.Sqrt(0.75f) / 3);
		Vector3 p1 = new Vector3(+0.5f, 0, -Mathf.Sqrt(0.75f) / 3);
		Vector3 p2 = new Vector3(0, 0, Mathf.Sqrt(0.75f) * 2 / 3);
		Vector3 p3 = new Vector3(0, Mathf.Sqrt(0.75f), 0);

		mesh.vertices = new Vector3[] { p0, p1, p2, p3 };
		mesh.triangles = new int[] {
			0,
			1,
			2,
			0,
			2,
			3,
			2,
			1,
			3,
			0,
			3,
			1
		};

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();

		float scaleFactor = currrentCity.constants.GetMaxDimension() / 20;

		Vector3 newLocation = new Vector3(location.x, scaleFactor, location.y);
		marker.transform.position = newLocation;

		marker.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

		marker.GetComponent<Renderer>().material.color = Color.red;
		marker.GetComponent<Renderer>().material.color = Color.red;
		marker.transform.Rotate(180, Random.Range(0, 360), 0, Space.Self);
	}

}