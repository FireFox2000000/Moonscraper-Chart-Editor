// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;

namespace MoonscraperChartEditor.Song
{
    // Marker for roll lanes in drums and strum lanes for guitar
    [Serializable]
    public class LaneModifier : ChartObject
    {
        private readonly ID _classID = ID.LaneModifier;
        public override int classID { get { return (int)_classID; } }
        public uint length;

        public LaneModifier(LaneModifier that) : base(that.tick)
        {
            length = that.length;
        }

        public override SongObject Clone()
        {
            return new LaneModifier(this);
        }

        public override bool AllValuesCompare<T>(T songObject)
        {
            if (this == songObject && (songObject as LaneModifier).length == length)
                return true;
            else
                return false;
        }
    }
}
