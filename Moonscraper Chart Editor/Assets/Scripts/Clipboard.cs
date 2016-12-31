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

    public Clipboard(ChartObject[] data, Rect rect, Song song)
    {
        this.data = data;
        SetCollisionArea(rect, song);
    }

    public void SetCollisionArea(Rect rect, Song song)
    {
        xPosition = rect.x;
        collisionAreaXSize = rect.width;

        _areaChartPosMin = song.WorldYPositionToChartPosition(rect.position.y);
        _areaChartPosMax = song.WorldYPositionToChartPosition(rect.position.y + rect.height);
    }
}
