using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;

public class VoronoiDemo : MonoBehaviour {
	[SerializeField] private int m_pointCount = 50;
	[SerializeField] private float m_mapWidth = 100;
	[SerializeField] private float m_mapHeight = 100;

	private List<Vector2> m_points;

	private Delaunay.Voronoi v;

	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private List<List<Vector2>> m_regions;

	private int currentRegion = 0;
	private int currentRegionPiece = 0;

	void Awake () {
		Demo ();
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			currentRegion++;
		}
		if (Input.GetMouseButtonDown (1)) {
			currentRegionPiece++;
		}
		if (Input.GetMouseButtonDown (02)) {
			Demo ();
		}
	}

	private void Demo () {

		List<uint> colors = new List<uint> ();
		m_points = new List<Vector2> ();

		for (int i = 0; i < m_pointCount; i++) {
			colors.Add (0);
			m_points.Add (new Vector2 (
				UnityEngine.Random.Range (0, m_mapWidth),
				UnityEngine.Random.Range (0, m_mapHeight)));
		}
		v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();
		m_regions = v.Regions ();
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.red;
		if (m_points != null) {
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points[i], 1.0f);
			}
		}

		if (m_edges != null) {
			Gizmos.color = Color.white;
			for (int i = 0; i < m_edges.Count; i++) {
				Vector2 left = (Vector2) m_edges[i].p0;
				Vector2 right = (Vector2) m_edges[i].p1;
				Gizmos.DrawLine ((Vector3) left, (Vector3) right);
			}
		}

		// Gizmos.color = Color.magenta;
		// if (m_delaunayTriangulation != null) {
		// 	for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
		// 		Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
		// 		Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
		// 		Gizmos.DrawLine ((Vector3)left, (Vector3)right);
		// 	}
		// }

		// 	Gizmos.color = Color.green;
		// if (m_spanningTree != null) {
		// 	for (int i = 0; i< m_spanningTree.Count; i++) {
		// 		LineSegment seg = m_spanningTree [i];				
		// 		Vector2 left = (Vector2)seg.p0;
		// 		Vector2 right = (Vector2)seg.p1;
		// 		Gizmos.DrawLine ((Vector3)left, (Vector3)right);
		// 	}
		// }

		// if (m_regions != null) {
		// 	Gizmos.color = Color.cyan;
		// 	List<Vector2> currentRegion = m_regions[currentRegion % (m_regions.Count - 1)];
		// 	Gizmos.DrawSphere (m_points[currentRegion % (m_regions.Count - 1)], 1.0f);
		// 	for (int i = 0; i < currentRegion.Count; i++) {
		// 		if (i != currentRegionPiece % currentRegion.Count) {
		// 			Gizmos.DrawSphere (currentRegion[i], 2.0f);
		// 		} else {
		// 			Gizmos.color = Color.yellow;
		// 			Gizmos.DrawSphere (currentRegion[i], 2.0f);
		// 			Gizmos.color = Color.cyan;
		// 		}
		// 	}
		// }

		if (m_points != null) {
			Gizmos.color = Color.cyan;
			Vector2 currentPoint = m_points[currentRegion % m_points.Count];
			Gizmos.DrawSphere (currentPoint, 1.0f);

			List<LineSegment> selectedRegionEdges = v.VoronoiBoundaryForSite (currentPoint);
			for (int i = 0; i < selectedRegionEdges.Count; i++) {
				LineSegment seg = selectedRegionEdges[i];
				Vector2 left = (Vector2) seg.p0;
				Vector2 right = (Vector2) seg.p1;
				Gizmos.DrawLine ((Vector3) left, (Vector3) right);
				Gizmos.color = Color.cyan;
			}

			List<Vector2> selectedRegionPoints = v.Region (currentPoint);
			for (int i = 0; i < selectedRegionPoints.Count; i++) {
				if (i != currentRegionPiece % selectedRegionPoints.Count) {
					Gizmos.DrawSphere (selectedRegionPoints[i], 2.0f);
				} else {
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere (selectedRegionPoints[i], 2.0f);
					Gizmos.color = Color.cyan;
				}
			}
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
}