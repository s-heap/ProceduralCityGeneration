using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Road : IEquatable<Road> {
    public Vector2 source;
    public Vector2 destination;

    public Road(Vector2 source, Vector2 destination) {
        this.source = source;
        this.destination = destination;
    }

    public Road() {
        this.source = new Vector2(0, 0);
        this.destination = new Vector2(0, 0);
    }

    public void DrawGizmos() {
        Gizmos.DrawLine((Vector3) source, (Vector3) destination);
    }

    public double GetLength() {
        return Vector2.Distance(source, destination);
    }

    public Road GetPerpendicularRoad(float length) {
        return new Road(GetMidPoint(), GetMidPoint() + (GetPerpendicular().normalized * length));
    }

    public Vector2 GetMidPoint() {
        return (source + destination) / 2;
    }

    public Vector2 GetPerpendicular() {
        return Vector2.Perpendicular(destination - source);
    }

    public Road GetSnapBox(float snapSize) {
        Road boundingBox = GetBoundingBox();
        boundingBox.source = boundingBox.source - new Vector2(snapSize, snapSize);
        boundingBox.destination = boundingBox.destination + new Vector2(snapSize, snapSize);
        return boundingBox;

        // Vector2 diff = destination - source;
        // Vector2 outputSrc = source;
        // Vector2 outputDest = destination;

        // outputSrc.x += (diff.x < 0) ? snapSize : -snapSize;
        // outputDest.x += (diff.x < 0) ? -snapSize : snapSize;

        // outputSrc.y += (diff.y < 0) ? snapSize : -snapSize;
        // outputDest.y += (diff.y < 0) ? -snapSize : snapSize;

        // return new Road(outputSrc, outputDest);
    }

    public bool BoundingBoxOverlap(Road input) {
        Road bounds = GetBoundingBox();
        Road inputBounds = input.GetBoundingBox();

        if (bounds.source.x > inputBounds.destination.x || bounds.destination.x < inputBounds.source.x) {
            return false;
        }

        if (bounds.source.y > inputBounds.destination.y || bounds.destination.y < inputBounds.source.y) {
            return false;
        }

        return true;
    }

    public bool BoundingBoxOverlap(Vector2 input) {
        Road bounds = GetBoundingBox();

        if (bounds.source.x > input.x || bounds.destination.x < input.x) {
            return false;
        }

        if (bounds.source.y > input.y || bounds.destination.y < input.y) {
            return false;
        }

        return true;
    }

    public Road GetBoundingBox() {
        return new Road(Vector2.Min(source, destination), Vector2.Max(source, destination));
    }

    public float GetPointDistance(Vector2 input) {
        Vector2 ab = destination - source;
        Vector2 ap = input - source;
        float r = Vector2.Dot(ab, ap) / ab.sqrMagnitude;
        float s = Vector2.Dot(Vector2.Perpendicular(ap), ab) / ab.sqrMagnitude;
        // Debug.Log(r);
        // Debug.Log(s);
        // return Math.Abs(s) * ab.magnitude;
        float u = ((input.x - source.x) * (destination.x - source.x) + (input.y - source.y) * (destination.y - source.y)) / ab.sqrMagnitude;
        Vector2 intersection = source + (u * ab);
        return Vector2.Distance(intersection, input);
    }

    public bool LineIntersection(Road inputRoad) {
        Vector2 a = destination - source;
        Vector2 b = inputRoad.source - inputRoad.destination;
        Vector2 c = source - inputRoad.source;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float denominator = a.y * b.x - a.x * b.y;

        if (denominator == 0) {
            return false;
        } else if (denominator > 0) {
            if (alphaNumerator < 0 || alphaNumerator > denominator || betaNumerator < 0 || betaNumerator > denominator) {
                return false;
            }
        } else if (alphaNumerator > 0 || alphaNumerator < denominator || betaNumerator > 0 || betaNumerator < denominator) {
            return false;
        }
        return true;

        // Vector2 ab = destination - source;
        // Vector2 cd = inputRoad.destination - inputRoad.source;
        // float denom = (ab.x * cd.y) - (ab.y * cd.x);
        // if (denom == 0) return false; // lines are parallel
        // Vector2 ca = source - inputRoad.source;
        // float r = ((ca.y * cd.x) - (ca.x * cd.y)) / denom;
        // float s = ((ca.y * ab.x) - (ca.x * ab.y)) / denom;
        // Debug.Log(r);
        // Debug.Log(s);
        // if (r == 0 && s == 0) return false; // lines are coincident
        // return true;
    }

    public float GetReverseRoadBearing() {
        return (360 - Vector2.SignedAngle(new Vector2(0, 1), (source - destination))) % 360;
    }

    public override string ToString() {
        return source + " -> " + destination;
    }

    public Road GetSproutedRoad(float rot, float segmentSize) {
        float newRoadBearingRad = ((GetReverseRoadBearing() + rot) % 360) * Mathf.Deg2Rad;
        Vector2 newRoadVector = new Vector2(Mathf.Sin(newRoadBearingRad), Mathf.Cos(newRoadBearingRad));
        return new Road(destination, destination + ((newRoadVector.normalized) * segmentSize));
    }

    public Vector3[] GetMeshCoords(float roadWidth) {
        Vector2 perp = GetPerpendicular().normalized * (roadWidth / 2);
        Vector3[] output = new Vector3[4];
        output[0] = MakeVec3(source + perp);
        output[1] = MakeVec3(source - perp);
        output[2] = MakeVec3(destination + perp);
        output[3] = MakeVec3(destination - perp);
        return output;
    }

    public Mesh CreateMesh() {
        Vector3[] meshCoords = GetMeshCoords(1);
        int[] tris = new int[] { 0, 2, 1, 1, 2, 3 };

        Mesh mesh = new Mesh();
        mesh.vertices = meshCoords;
        mesh.triangles = tris;
        return mesh;
    }

    private Vector3 MakeVec3(Vector2 input) {
        return new Vector3(input.x, 0, input.y);
    }

    public static int[] GetTris(int startPos) {
        return new int[] {
            startPos + 0, startPos + 2, startPos + 1,
                startPos + 1, startPos + 2, startPos + 3,
        };
    }

    public Road GetInverse() {
        return new Road(destination, source);
    }

    public Road GetMostClockWise(List<Road> edges) {
        return edges.OrderBy(x => GetAngleAntiClockWise(x)).First();
    }

    public Road GetLeastClockWise(List<Road> edges) {
        return edges.OrderBy(x => GetAngleAntiClockWise(x)).Last();
    }

    // For clockwise turns, the less, the better. Full backtracking edge will return 360
    public float GetAngleAntiClockWise(Road input) {
        if (source == input.destination && destination == input.source) {
            return 360;
        }
        return (360 + Vector2.SignedAngle(source - destination, input.destination - input.source)) % 360;
    }

    public override bool Equals(object obj) {
        return Equals(obj as Road);
    }

    public bool Equals(Road obj) {
        return obj.source == source && obj.destination == destination;
    }

    public override int GetHashCode() {
        String code = source.ToString() + " " + destination.ToString();
        int output;
        Int32.TryParse(code, out output);
        return output;
    }

    // presumes that source of this road is at road intersection
    public Road GetBisector(Road secondRoad, float length) {
        float direction = ((GetReverseRoadBearing() - GetAngleAntiClockWise(secondRoad) / 2) % 360) * Mathf.Deg2Rad;
        Vector2 vec = length * new Vector2(Mathf.Sin(direction), Mathf.Cos(direction)).normalized;
        return new Road(destination, destination + vec);
    }

    public Vector2 GetGradient() {
        return (destination - source).normalized;
    }

    public bool AreParallel(Road input) {
        return GetGradient() == input.GetGradient();
    }
}