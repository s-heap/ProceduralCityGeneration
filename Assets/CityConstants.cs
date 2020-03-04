using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class CityConstants {
    private int m_pointCount;
    public float m_mapWidth;
    public float m_mapHeight;

    public float roadWidth = 5.5f;

    private Vector2 populationPerlinShift;
    private Vector2 businessPerlinShift;

    public CityConstants(int m_pointCount, float m_mapWidth, float m_mapHeight,
        Vector2 populationPerlinShift, Vector2 businessPerlinShift) {
        this.m_pointCount = m_pointCount;
        this.m_mapWidth = m_mapWidth;
        this.m_mapHeight = m_mapHeight;
        this.populationPerlinShift = populationPerlinShift;
        this.businessPerlinShift = businessPerlinShift;
    }

    public int GetPointCount() { return m_pointCount; }
    public float GetMapWidth() { return m_mapWidth; }
    public float GetMapHeight() { return m_mapHeight; }
    public Vector2 GetPopShift() { return populationPerlinShift; }
    public Vector2 GetBusShift() { return businessPerlinShift; }

    public Vector2 GetCentre() {
        return new Vector2(m_mapWidth / 2, m_mapHeight / 2);
    }

    public float GetMaxDistance() {
        return Vector2.Distance(GetCentre(), new Vector2(m_mapWidth, m_mapHeight));
    }

    public float GetDistanceToCentre(Vector2 input) {
        return Vector2.Distance(input, GetCentre());
    }

    public float GetNormalisedDistanceToCentre(Vector2 input) {
        return 1 - GetDistanceToCentre(input) / GetMaxDistance();
    }

    public float GetPopNoiseValue(Vector2 input) {
        input += populationPerlinShift;
        return Mathf.PerlinNoise(input.x, input.y);
    }
    public float GetBusNoiseValue(Vector2 input) {
        input += businessPerlinShift;
        return Mathf.PerlinNoise(input.x, input.y);
    }

    public float GetMaxDimension() {
        return Mathf.Max(m_mapWidth, m_mapHeight);
    }
}