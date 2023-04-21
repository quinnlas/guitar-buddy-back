// describes a way of playing a song (what frets/strings to play each note on)
public class Playing
{
  class PlayingMeasure
  {
    public List<PlayingNote> notes { get; set; }

    public PlayingMeasure()
    {
      this.notes = new List<PlayingNote>();
    }
  }

  class PlayingNote
  {
    // guitar strings are usually described as 1st, 2nd etc
    // but this is 0-indexed
    public int stringIndex { get; set; }
    public int fret { get; set; }

    public PlayingNote() { }
  }

  private Song song;
  private List<PlayingMeasure> playingMeasures;

  public Playing(Song song, int[] tuning, int numFrets)
  {
    this.song = song;
    this.playingMeasures = song.measures.Select(measure =>
    {
      PlayingMeasure pm = new PlayingMeasure();

      pm.notes = measure.notes.Select(note =>
      {
        PlayingNote pn = new PlayingNote();
        // find the first string the has the right note
        // this causes an edge case if a chord ends up with multiple notes on the same string
        // this would be impossible to play so optimizing will fix it
        // but it would break ToString if not fixed
        pn.stringIndex = Array.FindIndex(tuning, openNote => openNote <= note.pitch && (openNote + numFrets) >= note.pitch);
        pn.fret = note.pitch - tuning[pn.stringIndex];

        return pn;
      }).ToList();

      return pm;
    }).ToList();
  }

  public void optimizeDistance()
  {

  }

  // outputs the playing arrangement as a tab
  public override string ToString()
  {
    string result = "";

    for (int mIndex = 0; mIndex < this.playingMeasures.Count; mIndex++)
    {
      PlayingMeasure pm = this.playingMeasures[mIndex];
      string[] measureLines = new[] { "|-", "|-", "|-", "|-", "|-", "|-" }; // TODO support diff number of strings

      double lastMeasureStart = 0;
      bool[] addedToLine = new[] { false, false, false, false, false, false }; // TODO

      for (int nIndex = 0; nIndex < pm.notes.Count; nIndex++)
      {
        PlayingNote pn = pm.notes[nIndex];

        double currentMeasureStart = this.song.measures[mIndex].notes[nIndex].measureStart;
        if (currentMeasureStart > lastMeasureStart)
        {
          lastMeasureStart = currentMeasureStart;
          for (int lIndex = 0; lIndex < measureLines.Length; lIndex++)
          {
            // add spacers to lines that didn't have notes
            if (!addedToLine[lIndex]) measureLines[lIndex] += "--";
            // add spacers to all lines (TODO something something rhythm)
            measureLines[lIndex] += "-";
          }
          addedToLine = new[] { false, false, false, false, false, false };
        }

        measureLines[pn.stringIndex] += pn.fret.ToString().PadRight(2, '-'); // TODO more elegant handling of 2 digit frets
        addedToLine[pn.stringIndex] = true;
      }

      result += String.Join('\n', measureLines) + "\n\n";
    }

    return result;
  }
}