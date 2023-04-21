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

  private int[] tuning;

  private int numFrets;

  // 12th fret should be 13 inches
  // 1.75 in between top/bottom string = .35in between each string
  private double[] fretDistances;
  private double stringDistance;

  public Playing(Song song, int[] tuning, int numFrets)
  {
    this.song = song;
    this.tuning = tuning;
    this.numFrets = numFrets;

    this.fretDistances = new double[numFrets + 1];
    for (int fret = 0; fret <= numFrets; fret++) {
      this.fretDistances[fret] = 26.0 / Math.Pow(2.0, (double)fret / 12.0); // TODO different guitar lengths
    }
    this.stringDistance = .35; // TODO custom

    this.playingMeasures = song.measures.Select(measure =>
    {
      PlayingMeasure pm = new PlayingMeasure();

      pm.notes = measure.notes.Select(note =>
      {
        PlayingNote pn = new PlayingNote();
        // find the any string that has the right note
        // find last index will pick a lower string and therefore higher fret to start with
        // this speeds up optimizing for distance
        // this causes an edge case if a chord ends up with multiple notes on the same string
        // this would be impossible to play so optimizing will fix it
        // but it would break ToString if not fixed
        pn.stringIndex = Array.FindLastIndex(tuning, openNote => openNote <= note.pitch && (openNote + numFrets) >= note.pitch);
        pn.fret = note.pitch - tuning[pn.stringIndex];

        return pn;
      }).ToList();

      return pm;
    }).ToList();
  }

  private bool stringHasNote(int pitch, int openNote) {
    return openNote <= pitch && (openNote + this.numFrets) >= pitch;
  }

  private int getPitch(PlayingNote pn) {
    return pn.fret + this.tuning[pn.stringIndex];
  }

  public void optimizeDistance(int iterations)
  {
    this.playingMeasures = SimulatedAnnealing.solve<List<PlayingMeasure>>(
      this.playingMeasures,
      iterations,
      genNeighbor,
      scoreDistance
    );
  }

  private static Random rand = new Random();

  private List<PlayingMeasure> genNeighbor(List<PlayingMeasure> current) {
    // TODO more efficient if mutation is ok so we want to make that possible somehow (check salesman.js from other project)
    // TODO add totalNotes property to Playing (so we can pick a random note with equal chance for each)
    // TODO change Playing to PlayingTab
    // TODO fix chord on same string issue
    int selectedMeasureIndex, selectedNoteIndex;
    PlayingNote newNote;
    while (true) {
      // randomly do stuff until something is done (TODO improve this comment)
      // TODO var
      selectedMeasureIndex = (int)rand.NextInt64(current.Count);
      var selectedMeasure = current[selectedMeasureIndex];
      selectedNoteIndex = (int)rand.NextInt64(selectedMeasure.notes.Count);
      var selectedNote = selectedMeasure.notes[selectedNoteIndex];

      // TODO only move up or down one string, you want the nearest neighbor possible
      // https://en.wikipedia.org/wiki/Simulated_annealing#Sufficiently_near_neighbour
      var otherStringIndices = (new [] {0, 1, 2, 3, 4, 5})
        .Where(strIndex => {
          if (strIndex == selectedNote.stringIndex) return false;
          return this.stringHasNote(this.getPitch(selectedNote), this.tuning[strIndex]);
        })
        .ToArray();

      if (otherStringIndices.Length == 0) continue;
      var newStrIndex = otherStringIndices[(int)rand.NextInt64(otherStringIndices.Length)];
      var newFret = this.getPitch(selectedNote) - this.tuning[newStrIndex];
      newNote = new PlayingNote();
      newNote.fret = newFret;
      newNote.stringIndex = newStrIndex;
      break;
    }

    // assemble new measure list
    var newList = current.ToList();
    var newNoteList = newList[selectedMeasureIndex].notes.ToList();
    newNoteList[selectedNoteIndex] = newNote;
    var newMeasure = new PlayingMeasure();
    newMeasure.notes = newNoteList;
    newList[selectedMeasureIndex] = newMeasure;

    return newList;
  }

  private double scoreDistance(List<PlayingMeasure> measures) {
    double distance = 0;

    for (int i = 0; i < measures.Count; i++) {
      var measure = measures[i];
      for (int j = i == 0 ? 1 : 0; j < measure.notes.Count; j++) {
        var firstNoteMeasure = measures[j == 0 ? i - 1 : i];
        var note1 = firstNoteMeasure.notes[j == 0 ? firstNoteMeasure.notes.Count - 1 : j - 1];
        var note2 = measure.notes[j];
        double xDist = this.fretDistances[note2.fret] - this.fretDistances[note1.fret];
        double yDist = (note2.stringIndex - note1.stringIndex) * this.stringDistance;
        distance += Math.Sqrt(xDist * xDist + yDist * yDist);
      }
    }

    return distance;
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