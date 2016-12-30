using UnityEngine;
using System.Collections;

public class Clipboard {
    public ChartObject[] data;

    uint _areaChartPosMin, _areaChartPosMax;
    float xPosition;
    float collisionAreaXSize;

    public uint areaChartPosMin { get { return _areaChartPosMin; } }
    public uint areaChartPosMax { get { return _areaChartPosMax; } }

    public Rect GetCollisionRect (Song song)
    {
        Vector2 size;

        float minWorldPos = song.ChartPositionToWorldYPosition(_areaChartPosMin);
        float maxWorldPos = song.ChartPositionToWorldYPosition(_areaChartPosMax);

        size = new Vector2(collisionAreaXSize, maxWorldPos - minWorldPos);

        Vector2 min = new Vector2(xPosition, minWorldPos);

        return new Rect(min, size);
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
