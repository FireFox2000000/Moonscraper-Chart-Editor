using UnityEngine;
using System.Collections;

public class Clipboard {
    public ChartObject[] data;

    uint _areaChartPosMin, _areaChartPosMax;
    float xPosition;
    float collisionAreaXSize;

    public uint areaChartPosMin { get { return _areaChartPosMin; } }
    public uint areaChartPosMax { get { return _areaChartPosMax; } }

    public uint collisionAreaToDataDistance
    {
        get
        {
            if (data.Length > 0)
                return (data[0].position - _areaChartPosMin);
            else
                return 0;
        }
    }

    public Rect GetCollisionRect (uint chartPosInit, Song song)
    {
        Vector2 size;

        float minWorldPos = song.ChartPositionToWorldYPosition(chartPosInit);
        float maxWorldPos = song.ChartPositionToWorldYPosition(_areaChartPosMax - _areaChartPosMin + chartPosInit);

        size = new Vector2(collisionAreaXSize, maxWorldPos - minWorldPos);

        Vector2 position = new Vector2(xPosition, minWorldPos);

        return new Rect(position, size);
    }

    public Clipboard()
    {
        data = new ChartObject[0];

        xPosition = 0;
        collisionAreaXSize = 0;

        _areaChartPosMin = 0;
        _areaChartPosMax = 0;
    }

    public Clipboard(ChartObject[] data, SelectionArea area, Song song)
    {
        this.data = data;
        SetCollisionArea(area, song);
    }

    public void SetCollisionArea(SelectionArea area, Song song)
    {
        xPosition = area.xPos;
        collisionAreaXSize = area.width;

        _areaChartPosMin = area.tickMin;
        _areaChartPosMax = area.tickMax;

        //_areaChartPosMin = song.WorldYPositionToChartPosition(rect.position.y);
        //_areaChartPosMax = song.WorldYPositionToChartPosition(rect.position.y + rect.height);
    }

    public struct SelectionArea
    {
        public float xPos, width;
        public uint tickMin;
        public uint tickMax;

        public SelectionArea(Rect rect, uint tickMin, uint tickMax)
        {
            this.xPos = rect.x;
            this.width = rect.width;
            this.tickMin = tickMin;
            this.tickMax = tickMax;
        }

        public SelectionArea(Vector2 cornerA, Vector2 cornerB, uint tickMin, uint tickMax)
        {
            // Bottom left corner is position
            if (cornerA.x < cornerB.x)
                xPos = cornerA.x;
            else
                xPos = cornerB.x;

            Vector2 size = new Vector2(Mathf.Abs(cornerA.x - cornerB.x), Mathf.Abs(cornerA.y - cornerB.y));

            this.width = size.x;
            this.tickMin = tickMin;
            this.tickMax = tickMax;
        }

        public Rect GetRect(Song song)
        {
            Vector2 position;
            Vector2 size;

            float yMin = song.ChartPositionToWorldYPosition(tickMin);
            float yMax = song.ChartPositionToWorldYPosition(tickMax);
            size.x = width;
            size.y = yMax - yMin;

            position.x = xPos;
            position.y = yMin;

            return new Rect(position, size);
        }

        public static SelectionArea operator +(SelectionArea a, SelectionArea b)
        {
            SelectionArea area = a;

            if (area.tickMin > b.tickMin)
                area.tickMin = b.tickMin;

            if (area.tickMax < b.tickMax)
                area.tickMax = b.tickMax;

            // Extend the rect
            float xMin, xMax;

            if (area.xPos < b.xPos)
                xMin = area.xPos;
            else
                xMin = b.xPos;

            if (area.xPos + area.width > b.xPos + b.width)
                xMax = area.xPos + area.width;
            else
                xMax = b.xPos + b.width;

            area.xPos = xMin;
            area.width = xMax - xMin;

            return area;
        }
        /*
        public static SelectionArea operator -(SelectionArea a, SelectionArea b)
        {
            SelectionArea area = a;

            if (area.tickMin < b.tickMin)
                area.tickMin = b.tickMin;

            if (area.tickMax > b.tickMax)
                area.tickMax = b.tickMax;

            // Leave rect x size

            return area;
        }*/
    }
}
