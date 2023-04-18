using System.Text.RegularExpressions;

public class Song
{
  public static int[] STANDARD_TUNING = new int[] { 64, 59, 55, 50, 45, 40 };

  public List<Measure> measures { get; }

  public Song() {
    this.measures = new List<Measure>();
  }
  public Song(string tab, int[] tuning)
  {
    List<List<string>> blocks = getCleanedTabBlocks(tab);

    this.measures = new List<Measure>();

    foreach (List<string> block in blocks)
    {
      // a block can have multiple measures
      this.measures.AddRange(parseBlockIntoMeasures(block, tuning));
    }
  }

  public Song(TabForm tabForm) : this(tabForm.tab, tabForm.tuning) { }


  // starts and ends with |
  // includes - | / \ ( ) _ and alphanumeric
  // | is the measure end character
  // - is a space/rest character
  // /\s are for slides
  // () are for grouping
  // _= are for sustaining a note
  // 0-9 are the fret numbers
  // hp are for hammer-on and pickup
  // br are for bend and release
  // o is for repeat bar ASCII art
  // ~ is for vibrato
  private static Regex blockLineRegex = new Regex(@"\|[\-|\/\\\(\)_=~0-9hpbrso]*\|", RegexOptions.IgnoreCase);
  private static Regex noteRegex = new Regex(@"\d+");

  /*
    returns a list of normalised blocks eg

    |-------5-7-----7-|-8-----8-2-----2-|-0---------0-----|-----------------|
    |-----5-----5-----|---5-------3-----|---1---1-----1---|-0-1-1-----------|
    |---5---------5---|-----5-------2---|-----2---------2-|-0-2-2---2-------|
    |-7-------6-------|-5-------4-------|-3---------------|-----------------|
    |-----------------|-----------------|-----------------|-2-0-0---0--/8-7-|
    |-----------------|-----------------|-----------------|-----------------|

    each block is represented as a list of lines
  */
  private static List<List<string>> getCleanedTabBlocks(string tabText)
  {
    // find consecutive lines
    string[] lines = tabText.Split('\n');

    List<List<string>> blocks = new List<List<string>>();
    List<string> currentBlockLines = new List<string>();

    for (int i = 0; i < lines.Length; i++)
    {
      bool thisLineMatches = blockLineRegex.IsMatch(lines[i]);
      if (thisLineMatches)
      {
        currentBlockLines.Add(lines[i]);
      }

      if ((!thisLineMatches || i == lines.Length - 1) && currentBlockLines.Count > 0)
      {
        // handle end of block
        blocks.Add(currentBlockLines);
        currentBlockLines = new List<string>();
      }
    }

    return blocks;
  }

  private static List<Measure> parseBlockIntoMeasures(List<string> block, int[] tuning)
  {
    List<Measure> measures = new List<Measure>();

    // parse blocks one measure at a time
    int measureStart = block[0].IndexOf('|') + 2; // TODO assumes one beginning spacer
    int measureEnd = block[0].IndexOf('|', measureStart);
    while (true)
    {
      Measure measure = new Measure();
      int measureLength = measureEnd - measureStart;

      // read notes from each line of the block
      for (int lineIndex = 0; lineIndex < block.Count; lineIndex++)
      {
        string blockLine = block[lineIndex];
        // trim to the current measure
        string measureLine = blockLine.Substring(measureStart, measureLength);
        MatchCollection noteMatches = noteRegex.Matches(measureLine);

        foreach (Match noteMatch in noteMatches)
        {
          Note note = new Note();
          note.pitch = tuning[lineIndex] + Int32.Parse(noteMatch.Value);
          note.measureStart = ((float)noteMatch.Index) / (float)measureLength; // TODO simple rhythm guessing
          measure.notes.Add(note);
        }
      }

      // sort measure by note order
      measure.notes.Sort((Note a, Note b) =>
      {
        return a.measureStart.CompareTo(b.measureStart);
      });

      measures.Add(measure);

      measureStart = measureEnd + 2;
      if (measureStart >= block[0].Length) break;
      measureEnd = block[0].IndexOf('|', measureStart);
      if (measureEnd == -1) break;
    }

    return measures;
  }

  public override bool Equals(object obj) {
    if (obj.GetType() != typeof(Song)) return false;

    return this.measures.SequenceEqual(((Song)obj).measures);
  }
}