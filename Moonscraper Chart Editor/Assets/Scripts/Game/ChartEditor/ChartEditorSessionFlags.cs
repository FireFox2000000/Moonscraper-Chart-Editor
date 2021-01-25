using System;

[Flags]
public enum ChartEditorSessionFlags
{
    None = 0,
    CurrentChartSavedInProprietyFormat = 1 << 0,
}
