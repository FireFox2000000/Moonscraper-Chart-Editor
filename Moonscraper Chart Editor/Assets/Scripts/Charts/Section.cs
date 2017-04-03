public class Section : Event
{
    private readonly ID _classID = ID.Section;

    public override int classID { get { return (int)_classID; } }

    public Section(string _title, uint _position) : base(_title, _position) { }

    public Section(Section section) : base(section.title, section.position) { }

    internal override string GetSaveString()
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
