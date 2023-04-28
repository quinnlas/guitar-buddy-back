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

  private int numStrings;

  // 12th fret should be 13 inches
  // 1.75 in between top/bottom string = .35in between each string
  private double[] fretDistances;
  private double stringDistance;

  public Playing(OptimizeForm form)
  {
    this.song = form.song;
    this.tuning = form.tuning;
    this.numFrets = form.numFrets;
    this.numStrings = form.numStrings;

    this.fretDistances = new double[numFrets + 1];
    for (int fret = 0; fret <= numFrets; fret++) {
      // 12 notes in an octave
      // ratio between adjacent notes is equal
      // octave ratio is double (double the frequency/half the length)
      this.fretDistances[fret] = form.neckInches / Math.Pow(2.0, (double)fret / 12.0);
    }
    this.stringDistance = form.inchesBetweenStrings;

    this.playingMeasures = song.measures.Select(measure =>
    {
      var pm = new PlayingMeasure();

      pm.notes = measure.notes.Select(note =>
      {
        var pn = new PlayingNote();
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
    // TODO fix chord on same string issue
    int selectedMeasureIndex, selectedNoteIndex;
    PlayingNote newNote;
    while (true) {
      // pick a random note that can be adjusted and randomly adjust it
      selectedMeasureIndex = (int)rand.NextInt64(current.Count);
      var selectedMeasure = current[selectedMeasureIndex];

      if (selectedMeasure.notes.Count == 0) continue;

      selectedNoteIndex = (int)rand.NextInt64(selectedMeasure.notes.Count);
      var selectedNote = selectedMeasure.notes[selectedNoteIndex];

      var otherStringIndices = (new [] { selectedNote.stringIndex - 1, selectedNote.stringIndex + 1 })
        .Where(strIndex => {
          if (strIndex < 0 || strIndex >= this.numStrings) return false;
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
    double distance = 0.0;

    PlayingNote lastNote = null;
    for (int i = 0; i < measures.Count; i++) {
      var measure = measures[i];
      for (int j = 0 ; j < measure.notes.Count; j++) {
        var curNote = measure.notes[j];
        // don't count open string notes since you don't have to move your hand for them
        if (curNote.fret == 0) continue;
        if (lastNote != null) {
          double xDist = this.fretDistances[lastNote.fret] - this.fretDistances[curNote.fret];
          double yDist = (lastNote.stringIndex - curNote.stringIndex) * this.stringDistance;
          distance += Math.Sqrt(xDist * xDist + yDist * yDist);
        }
        lastNote = curNote;
      }
    }

    return distance;
  }

  // outputs the playing arrangement as a tab
  public override string ToString()
  {
    var result = "";

    var blockLines = new string[this.numStrings];

    for (int mIndex = 0; mIndex < this.playingMeasures.Count; mIndex++)
    {
      var measureLines = this.measureIndexToLines(mIndex);
      int measureLength = measureLines[0].Length;

      if (blockLines[0] != null && (blockLines[0].Length + measureLength > 119)) {
        // end this block
        result += String.Join('\n', blockLines.Select(l => l + "|")) + "\n\n";
        blockLines = new string[this.numStrings];
      }

      blockLines = blockLines.Select((line, i) => line + measureLines[i]).ToArray();
    }
    result += String.Join('\n', blockLines.Select(l => l + "|")) + "\n\n";
    blockLines = new string[this.numStrings];

    return result;
  }

  private string[] measureIndexToLines(int measureIndex) {
    var pm = this.playingMeasures[measureIndex];
    var measureLines = new string[this.numStrings];
    Array.Fill(measureLines, "|-"); // should return the array >:(

    double lastMeasureStart = 0;
    var addedToLine = new bool[this.numStrings];

    // add all the notes
    for (int nIndex = 0; nIndex < pm.notes.Count; nIndex++)
    {
      var pn = pm.notes[nIndex];

      double currentMeasureStart = this.song.measures[measureIndex].notes[nIndex].measureStart;
      if (currentMeasureStart > lastMeasureStart)
      {
        lastMeasureStart = currentMeasureStart;
        for (int lIndex = 0; lIndex < measureLines.Length; lIndex++)
        {
          // add spacers to lines that didn't have notes
          if (!addedToLine[lIndex]) measureLines[lIndex] += "--";
          // add spacers to all lines (TODO change spacing based on rhythm)
          measureLines[lIndex] += "-";
        }
        addedToLine = new[] { false, false, false, false, false, false };
      }

      measureLines[pn.stringIndex] += pn.fret.ToString().PadRight(2, '-');
      addedToLine[pn.stringIndex] = true;
    }

    // handle the right side spacing
    int maxLineLength = measureLines.MaxBy(l => l.Length).Length;
    return measureLines.Select(l => l.PadRight(maxLineLength + 1, '-')).ToArray();
  }
}