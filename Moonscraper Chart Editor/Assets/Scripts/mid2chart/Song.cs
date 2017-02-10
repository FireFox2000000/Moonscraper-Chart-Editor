using System;
using System.Collections.Generic;
using System.Linq;

namespace mid2chart {
    internal class Song {
        public string songname = "", artist = "", year = "", charter = "";
        public long offset = 0;
        public List<Sync> sync = new List<Sync>();
        public List<Section> sections = new List<Section>();
        public List<Event> eGuitar = new List<Event>();
        public List<Event> mGuitar = new List<Event>();
        public List<Event> hGuitar = new List<Event>();
        public List<Event> xGuitar = new List<Event>();
        public List<Event> eBass = new List<Event>();
        public List<Event> mBass = new List<Event>();
        public List<Event> hBass = new List<Event>();
        public List<Event> xBass = new List<Event>();
        public List<Event> eKeys = new List<Event>();
        public List<Event> mKeys = new List<Event>();
        public List<Event> hKeys = new List<Event>();
        public List<Event> xKeys = new List<Event>();
        public List<NoteSection> tapGuitar = new List<NoteSection>();
        public List<NoteSection> tapBass = new List<NoteSection>();
        public List<NoteSection> eGuitarForceStrum = new List<NoteSection>();
        public List<NoteSection> mGuitarForceStrum = new List<NoteSection>();
        public List<NoteSection> hGuitarForceStrum = new List<NoteSection>();
        public List<NoteSection> xGuitarForceStrum = new List<NoteSection>();
        public List<NoteSection> eGuitarForceHOPO = new List<NoteSection>();
        public List<NoteSection> mGuitarForceHOPO = new List<NoteSection>();
        public List<NoteSection> hGuitarForceHOPO = new List<NoteSection>();
        public List<NoteSection> xGuitarForceHOPO = new List<NoteSection>();
        public List<NoteSection> eBassForceStrum = new List<NoteSection>();
        public List<NoteSection> mBassForceStrum = new List<NoteSection>();
        public List<NoteSection> hBassForceStrum = new List<NoteSection>();
        public List<NoteSection> xBassForceStrum = new List<NoteSection>();
        public List<NoteSection> eBassForceHOPO = new List<NoteSection>();
        public List<NoteSection> mBassForceHOPO = new List<NoteSection>();
        public List<NoteSection> hBassForceHOPO = new List<NoteSection>();
        public List<NoteSection> xBassForceHOPO = new List<NoteSection>();
        public List<NoteSection> eKeysForceHOPO = new List<NoteSection>();
        public List<NoteSection> mKeysForceHOPO = new List<NoteSection>();
        public List<NoteSection> hKeysForceHOPO = new List<NoteSection>();
        public List<NoteSection> xKeysForceHOPO = new List<NoteSection>();
        public List<NoteSection> eGuitarOpenNotes = new List<NoteSection>();
        public List<NoteSection> mGuitarOpenNotes = new List<NoteSection>();
        public List<NoteSection> hGuitarOpenNotes = new List<NoteSection>();
        public List<NoteSection> xGuitarOpenNotes = new List<NoteSection>();
        public List<NoteSection> eBassOpenNotes = new List<NoteSection>();
        public List<NoteSection> mBassOpenNotes = new List<NoteSection>();
        public List<NoteSection> hBassOpenNotes = new List<NoteSection>();
        public List<NoteSection> xBassOpenNotes = new List<NoteSection>();

        internal void TapToHopo() {
            if(tapGuitar.Count > 0) {
                eGuitarForceHOPO = TapToHopo(eGuitarForceHOPO,tapGuitar);
                mGuitarForceHOPO = TapToHopo(mGuitarForceHOPO,tapGuitar);
                hGuitarForceHOPO = TapToHopo(hGuitarForceHOPO,tapGuitar);
                xGuitarForceHOPO = TapToHopo(xGuitarForceHOPO,tapGuitar);
                tapGuitar.Clear();
            }
            if(tapBass.Count > 0) {
                eBassForceHOPO = TapToHopo(eBassForceHOPO,tapBass);
                mBassForceHOPO = TapToHopo(mBassForceHOPO,tapBass);
                hBassForceHOPO = TapToHopo(hBassForceHOPO,tapBass);
                xBassForceHOPO = TapToHopo(xBassForceHOPO,tapBass);
                tapBass.Clear();
            }
        }

        private List<NoteSection> TapToHopo(List<NoteSection> forceHOPO, List<NoteSection> tap) {
            foreach(NoteSection t in tap) {
                forceHOPO.Add(t);
            }
            return forceHOPO.OrderBy(f => f.tick).ToList();
        }

        internal void FixOverlaps() {
            if (xGuitar.Count > 0) xGuitar = FixOverlaps(xGuitar);
            if (hGuitar.Count > 0) hGuitar = FixOverlaps(hGuitar);
            if (mGuitar.Count > 0) mGuitar = FixOverlaps(mGuitar);
            if (eGuitar.Count > 0) eGuitar = FixOverlaps(eGuitar);
            if (xBass.Count > 0) xBass = FixOverlaps(xBass);
            if (hBass.Count > 0) hBass = FixOverlaps(hBass);
            if (mBass.Count > 0) mBass = FixOverlaps(mBass);
            if (eBass.Count > 0) eBass = FixOverlaps(eBass);
            if (xKeys.Count > 0) xKeys = FixOverlaps(xKeys);
            if (hKeys.Count > 0) hKeys = FixOverlaps(hKeys);
            if (mKeys.Count > 0) mKeys = FixOverlaps(mKeys);
            if (eKeys.Count > 0) eKeys = FixOverlaps(eKeys);
        }

        internal List<Event> FixOverlaps(List<Event> notes) {
            foreach (var e in notes) {
                var currentNote = e as Note;
                if (currentNote != null) {
                    var nextNote = GetNextNote(currentNote, notes);
                    if (nextNote != null && currentNote.tick+currentNote.sus >= nextNote.tick) {
                        currentNote.sus = nextNote.tick-currentNote.tick-24;
                    }
                }
            }
            return notes;
        }

        internal void FixBrokenChords() {
            if (xGuitar.Count > 0) xGuitar = FixBrokenChords(xGuitar, 0);
            if (hGuitar.Count > 0) hGuitar = FixBrokenChords(hGuitar, 1);
            if (mGuitar.Count > 0) mGuitar = FixBrokenChords(mGuitar, 2);
            if (eGuitar.Count > 0) eGuitar = FixBrokenChords(eGuitar, 3);
            if (xBass.Count > 0) xBass = FixBrokenChords(xBass, 4);
            if (hBass.Count > 0) hBass = FixBrokenChords(hBass, 5);
            if (mBass.Count > 0) mBass = FixBrokenChords(mBass, 6);
            if (eBass.Count > 0) eBass = FixBrokenChords(eBass, 7);
            if (xKeys.Count > 0) xKeys = FixBrokenChords(xKeys, 8);
            if (hKeys.Count > 0) hKeys = FixBrokenChords(hKeys, 8);
            if (mKeys.Count > 0) mKeys = FixBrokenChords(mKeys, 8);
            if (eKeys.Count > 0) eKeys = FixBrokenChords(eKeys, 8);
        }

        private List<Event> FixBrokenChords(List<Event> notes, int sec) {
            var fixedNotes = new List<Event>();
            foreach (var n in notes)
            {
                fixedNotes.Add(n);
                if (IsPartOfBrokenChord(n as Note, fixedNotes))
                {
                    var previousNote = GetPreviousNote(n as Note, fixedNotes);
                    var previousIndex = fixedNotes.IndexOf(previousNote);
                    var previousTick = previousNote.tick;
                    for (var j = previousIndex; j >= 0; j--, previousNote = fixedNotes[j] as Note)
                    {
                        //j >= 0 && previousNote.tick == previousTick; - old code incase something goes unexpectedly wrong
                        if (previousNote != null)
                        {
                            if (previousNote.tick != previousTick) break;
                            var tickDiff = n.tick - previousNote.tick; if (tickDiff < 96L) tickDiff = 0;
                            fixedNotes[j] = new Note(previousNote.note, previousNote.tick, tickDiff);
                            fixedNotes.Add(new Note(previousNote.note, n.tick, n.sus));
                            if (n.tick - previousTick <= 64)
                            {
                                switch (sec)
                                {
                                    case 0: xGuitarForceHOPO = AddForceHopoIfNecessary(n.tick, xGuitarForceHOPO, xGuitarForceStrum); break;
                                    case 1: hGuitarForceHOPO = AddForceHopoIfNecessary(n.tick, hGuitarForceHOPO, hGuitarForceStrum); break;
                                    case 2: mGuitarForceHOPO = AddForceHopoIfNecessary(n.tick, mGuitarForceHOPO, mGuitarForceStrum); break;
                                    case 3: eGuitarForceHOPO = AddForceHopoIfNecessary(n.tick, eGuitarForceHOPO, eGuitarForceStrum); break;
                                    case 4: xBassForceHOPO = AddForceHopoIfNecessary(n.tick, xBassForceHOPO, xBassForceStrum); break;
                                    case 5: hBassForceHOPO = AddForceHopoIfNecessary(n.tick, hBassForceHOPO, hBassForceStrum); break;
                                    case 6: mBassForceHOPO = AddForceHopoIfNecessary(n.tick, mBassForceHOPO, mBassForceStrum); break;
                                    case 7: eBassForceHOPO = AddForceHopoIfNecessary(n.tick, eBassForceHOPO, eBassForceStrum); break;
                                    case 8: break;
                                    default: throw new Exception("Invalid diff/instr. value(smaller than 0 or greater than 8)");
                                }
                            }
                            if (previousNote.tick != previousTick) break;
                        }
                        if (j == 0) break;
                    }
                }
            }
            return fixedNotes;
        }

        private List<NoteSection> AddForceHopoIfNecessary(long tick, List<NoteSection> forceHOPO, List<NoteSection> forceStrum) {
            List<NoteSection> added = forceHOPO;
            int i; bool check = true;
            foreach (var ns in forceStrum)
            {
                if (ns.tick > tick) break;
                if (ns.tick == tick) { check = false; break; }
            }
            if (check)
            {
                for (i = 0; i < added.Count; i++)
                {
                    NoteSection ns = added[i];
                    if (ns.tick > tick) break;
                    if (ns.tick + ns.sus > tick) { check = false; break; }
                }
                if (check) added.Insert(i, new NoteSection(tick, 24));
            }
            return added;
        }

        internal void FixForces() {
            if (xGuitarForceStrum.Count > 0) xGuitarForceStrum = FixForces(xGuitarForceStrum);
            if (hGuitarForceStrum.Count > 0) hGuitarForceStrum = FixForces(hGuitarForceStrum);
            if (mGuitarForceStrum.Count > 0) mGuitarForceStrum = FixForces(mGuitarForceStrum);
            if (eGuitarForceStrum.Count > 0) eGuitarForceStrum = FixForces(eGuitarForceStrum);
            if (xBassForceStrum.Count > 0) xBassForceStrum = FixForces(xBassForceStrum);
            if (hBassForceStrum.Count > 0) hBassForceStrum = FixForces(hBassForceStrum);
            if (mBassForceStrum.Count > 0) mBassForceStrum = FixForces(mBassForceStrum);
            if (eBassForceStrum.Count > 0) eBassForceStrum = FixForces(eBassForceStrum);
            if (xGuitarForceHOPO.Count > 0) xGuitarForceHOPO = FixForces(xGuitarForceHOPO);
            if (hGuitarForceHOPO.Count > 0) hGuitarForceHOPO = FixForces(hGuitarForceHOPO);
            if (mGuitarForceHOPO.Count > 0) mGuitarForceHOPO = FixForces(mGuitarForceHOPO);
            if (eGuitarForceHOPO.Count > 0) eGuitarForceHOPO = FixForces(eGuitarForceHOPO);
            if (xBassForceHOPO.Count > 0) xBassForceHOPO = FixForces(xBassForceHOPO);
            if (hBassForceHOPO.Count > 0) hBassForceHOPO = FixForces(hBassForceHOPO);
            if (mBassForceHOPO.Count > 0) mBassForceHOPO = FixForces(mBassForceHOPO);
            if (eBassForceHOPO.Count > 0) eBassForceHOPO = FixForces(eBassForceHOPO);
            if (tapGuitar.Count > 0) tapGuitar = FixForces(tapGuitar);
            if (tapBass.Count > 0) tapBass = FixForces(tapBass);
        }

        internal List<NoteSection> FixForces(List<NoteSection> forces) {
            var fixedForces = new List<NoteSection>();
            foreach (var ns in forces) {
                if (ns.sus == 1) fixedForces.Add(new NoteSection(ns.tick, ns.sus));
                else fixedForces.Add(new NoteSection(ns.tick, ns.sus+1));
            }
            return fixedForces;
        }

        internal void FixSp() {
            if (xGuitar.Count > 0) xGuitar = FixSp(xGuitar);
            if (hGuitar.Count > 0) hGuitar = FixSp(hGuitar);
            if (mGuitar.Count > 0) mGuitar = FixSp(mGuitar);
            if (eGuitar.Count > 0) eGuitar = FixSp(eGuitar);
            if (xBass.Count > 0) xBass = FixSp(xBass);
            if (hBass.Count > 0) hBass = FixSp(hBass);
            if (mBass.Count > 0) mBass = FixSp(mBass);
            if (eBass.Count > 0) eBass = FixSp(eBass);
        }

        internal List<Event> FixSp(List<Event> notes) {
            var fixedNotes = new List<Event>();
            foreach (var e in notes) {
                var n = e as Note;
                if (n != null) fixedNotes.Add(new Note(n.note, n.tick, n.sus));
                else {
                    var ns = e as NoteSection;
                    if (ns != null) fixedNotes.Add(new NoteSection(ns.tick, ns.sus+1));
                }
            }
            return fixedNotes;
        }

        internal void RemoveDuplicates() {
            if (xGuitar.Count > 0) xGuitar = RemoveDuplicates(xGuitar);
            if (hGuitar.Count > 0) hGuitar = RemoveDuplicates(hGuitar);
            if (mGuitar.Count > 0) mGuitar = RemoveDuplicates(mGuitar);
            if (eGuitar.Count > 0) eGuitar = RemoveDuplicates(eGuitar);
            if (xBass.Count > 0) xBass = RemoveDuplicates(xBass);
            if (hBass.Count > 0) hBass = RemoveDuplicates(hBass);
            if (mBass.Count > 0) mBass = RemoveDuplicates(mBass);
            if (eBass.Count > 0) eBass = RemoveDuplicates(eBass);
        }

        private List<Event> RemoveDuplicates(List<Event> notes) {
            var fixedNotes = notes;
            for (int i = fixedNotes.Count - 2; i  >=  0; i--) {
                if (IsDuplicate(notes[i], notes)) fixedNotes.RemoveAt(i);
            }
            return fixedNotes;
        }

        private bool IsDuplicate(Event e, List<Event> notes) {
            var n = e as Note;
            if (n != null) {
                for (var i = notes.IndexOf(n)+1; i < notes.Count; i++) {
                    var n2 = notes[i] as Note;
                    if (n2 != null) {
                        return n2.tick == n.tick && n2.note == n.note;
                    }
                }
            }
            else {
                var ns = e as NoteSection;
                if (ns != null) {
                    for (var i = notes.IndexOf(ns)+1; i < notes.Count; i++) {
                        var ns2 = notes[i] as NoteSection;
                        if (ns2 != null) {
                            return ns2.tick == ns.tick && ns2.sus == ns.sus;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsPartOfBrokenChord(Note n, List<Event> notes) {
            if (n == null) return false;
            if (notes[0] == n) return false;
            var previousNote = GetPreviousNote(n, notes);
            return previousNote != null && previousNote.tick != n.tick && previousNote.tick + previousNote.sus > n.tick;
        }

        public static Note GetPreviousNote(Note n, List<Event> notes) {
            if (notes[0] == n) return null;
            for (var i = notes.IndexOf(n)-1; i >= 0; i--) {
                var previousNote = notes[i] as Note;
                if (previousNote != null && n.tick-previousNote.tick != 0)
                    return notes[i] as Note;
            }
            return null;
        }

        public static Note GetNextNote(Note n, List<Event> notes) {
            if (notes[notes.Count-1] == n) return null;
            for (var i = notes.IndexOf(n)+1; i < notes.Count; i++) {
                var previousNote = notes[i] as Note;
                if (previousNote != null && n.tick-previousNote.tick != 0)
                    return notes[i] as Note;
            }
            return null;
        }
    }
}