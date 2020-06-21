﻿// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

[System.Serializable]
public class Section : Event
{
    private readonly ID _classID = ID.Section;

    public override int classID { get { return (int)_classID; } }

    public Section(string _title, uint _position) : base(_title, _position) { }

    public Section(Section section) : base(section.title, section.tick) { }

    internal override string GetSaveString()
    {
        return Globals.TABSPACE + tick + " = E \"section " + title + "\"" + Globals.LINE_ENDING;
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
