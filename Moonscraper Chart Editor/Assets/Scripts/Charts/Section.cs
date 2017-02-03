using UnityEngine;
using System.Collections;

public class Section : Event
{
    private readonly ID _classID = ID.Section;

    public override int classID { get { return (int)_classID; } }

    SectionController _controller = null;

    new public SectionController controller
    {
        get { return _controller; }
        set { _controller = value; base.controller = value; }
    }

    public Section(Song song, string _title, uint _position) : base(_title, _position) { }

    public Section(Section section) : base(section.title, section.position) { }

    public override string GetSaveString()
    {
        return Globals.TABSPACE + position + " = E \"section " + title + "\"" + Globals.LINE_ENDING;
    }

    new public static bool regexMatch(string line)
    {
        return new System.Text.RegularExpressions.Regex(@"\d+ = E " + @"""section [^""\\]*(?:\\.[^""\\]*)*""").IsMatch(line);
    }

    public override SongObject Clone()
    {
        return new Section(this);
    }
}
