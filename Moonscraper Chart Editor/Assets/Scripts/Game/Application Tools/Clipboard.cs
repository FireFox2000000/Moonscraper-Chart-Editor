// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

[System.Serializable]
public class Clipboard {
    public SongObject[] data = new SongObject[0];
    public float resolution = SongConfig.STANDARD_BEAT_RESOLUTION;
    public Song.Instrument instrument = Song.Instrument.Guitar;

    uint _areaChartPosMin = 0, _areaChartPosMax = 0;
    float xPosition = 0;
    float collisionAreaXSize = 0;

    public uint areaChartPosMin { get { return _areaChartPosMin; } }
    public uint areaChartPosMax { get { return _areaChartPosMax; } }

    public uint collisionAreaToDataDistance
    {
        get
        {
            //System.Windows.Forms.Clipboard.SetDataObject(data);
            //data = System.Windows.Forms.Clipboard.GetDataObject().GetData(typeof(SongObject[])) as SongObject[];
            if (data.Length > 0)
                return (data[0].tick - _areaChartPosMin);
            else
                return 0;
        }
    }

    public Rect GetCollisionRect (uint chartPosInit, Song song)
    {
        Vector2 size;

        float minWorldPos = song.TickToWorldYPosition(chartPosInit);
        float maxWorldPos = song.TickToWorldYPosition(_areaChartPosMax - _areaChartPosMin + chartPosInit);

        size = new Vector2(collisionAreaXSize, maxWorldPos - minWorldPos);

        Vector2 position = new Vector2(xPosition, minWorldPos);

        return new Rect(position, size);
    }
    
    public Clipboard()
    {
        data = new SongObject[0];

        xPosition = 0;
        collisionAreaXSize = 0;

        _areaChartPosMin = 0;
        _areaChartPosMax = 0;
    }
    
    public void SetData(SongObject[] data, SelectionArea area, Song song)
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
    }

    [System.Serializable]
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

            float yMin = song.TickToWorldYPosition(tickMin);
            float yMax = song.TickToWorldYPosition(tickMax);
            size.x = width;
            size.y = yMax - yMin;

            position.x = xPos;
            position.y = yMin;

            return new Rect(position, size);
        }

        public static SelectionArea operator +(SelectionArea a, SelectionArea b)
        {
            SelectionArea area = a;

            if (area.tickMin >= area.tickMax)
                return b;

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
        
        public static SelectionArea operator -(SelectionArea a, SelectionArea b)
        {
            SelectionArea area = a;

            if (b.xPos < area.xPos && b.xPos + b.width > area.xPos + area.width)
            {
                // Eligable to reduce the tick size
                if (area.tickMin >= b.tickMin)
                    area.tickMin = b.tickMax;

                if (area.tickMax <= b.tickMax)
                    area.tickMax = b.tickMin;

                if (area.tickMin > area.tickMax)
                    area.tickMin = area.tickMax;
            }

            if (b.tickMax > area.tickMax && b.tickMin < area.tickMin)
            {
                // Eligable to reduce the width
                if (area.xPos >= b.xPos)
                {
                    float originalXPos = area.xPos;
                    area.xPos = b.xPos + b.width;
                    area.width -= area.xPos - originalXPos;
                }

                if (area.xPos + area.width <= b.xPos + b.width)
                    area.width = b.xPos - area.xPos;
            }

            return area;
        }
    }
}
