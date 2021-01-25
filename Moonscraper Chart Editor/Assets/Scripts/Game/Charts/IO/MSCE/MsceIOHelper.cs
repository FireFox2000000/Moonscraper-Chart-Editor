using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperChartEditor.Song.IO
{
    // Stores space characters found in ChartEvent objects as Japanese full-width spaces. Need to convert this back when loading.
    public class MsceIOHelper
    {
        public const char WhitespaceChartEventReplacement = '\u3000';
        public const string FileExtention = ".msce";
    }
}
