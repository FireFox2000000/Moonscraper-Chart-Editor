using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public abstract class ChartObject {
    public int position;

    public abstract string GetSaveString();
}

public class Event : ChartObject
{
    public string title;

    public Event(string _title, int _position)
    {
        title = _title;
        position = _position;
    }

    public override string GetSaveString()
    {
        const string TABSPACE = "  ";
        return TABSPACE + position + " = E \"" + title + "\"\n";
    }

    public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""[^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}

public class Section : Event
{
    public Section(string _title, int _position) : base(_title, _position) { }

    new public string GetSaveString()
    {
        const string TABSPACE = "  ";
        return TABSPACE + position + " = E \"section " + title + "\"\n";
    }

    new public static bool regexMatch(string line)
    {
        return new Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }
}
