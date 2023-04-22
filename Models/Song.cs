using System.Text.RegularExpressions;

public class Song
{
  public static int[] STANDARD_TUNING = new int[] { 64, 59, 55, 50, 45, 40 };

  public List<Measure> measures { get; }

  public Song()
  {
    this.measures = new List<Measure>();
  }
  public Song(string tab, int[] tuning)
  {
    // convert tab -> tab blocks -> tab measures -> measures
    List<List<string>> blocks = getCleanedTabBlocks(tab);
    List<List<string>> tabMeasures = blocks
      .Select(block => convertBlockToTabMeasures(block))
      .SelectMany(x => x) // since a block can have multiple measures, we need to flatten 1 level
      .ToList();
    this.measures = tabMeasures.Select(tm => parseTabMeasure(tm, tuning)).ToList();
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
  private static Regex notSpacerRegex = new Regex(@"[^-]");

  /*
    returns a list of normalised blocks eg

    e|-------5-7-----7-|-8-----8-2-----2-|-0---------0-----|-----------------|
    B|-----5-----5-----|---5-------3-----|---1---1-----1---|-0-1-1-----------|
    G|---5---------5---|-----5-------2---|-----2---------2-|-0-2-2---2-------|
    D|-7-------6-------|-5-------4-------|-3---------------|-----------------|
    A|-----------------|-----------------|-----------------|-2-0-0---0--/8-7-|
    E|-----------------|-----------------|-----------------|-----------------|

    removing lines from the tab text that are not actual tabulature
    each block is represented as a list of lines
    there may or may not be characters before the first measure starts
  */
  private static List<List<string>> getCleanedTabBlocks(string tabText)
  {
    // find consecutive lines
    var lines = tabText.Split('\n');

    List<List<string>> blocks = new List<List<string>>();
    List<string> currentBlockLines = new List<string>();

    for (int i = 0; i < lines.Length; i++)
    {
      var thisLineMatches = blockLineRegex.IsMatch(lines[i]);
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

  /*
    given a block such as:
    e|-------5-7-----7-|-8-----8-2-----2-|-0---------0-----|-----------------|
    B|-----5-----5-----|---5-------3-----|---1---1-----1---|-0-1-1-----------|
    G|---5---------5---|-----5-------2---|-----2---------2-|-0-2-2---2-------|
    D|-7-------6-------|-5-------4-------|-3---------------|-----------------|
    A|-----------------|-----------------|-----------------|-2-0-0---0--/8-7-|
    E|-----------------|-----------------|-----------------|-----------------|

    return a list where each element is one measure with the beginning space removed, such as:
    ------5-7-----7-
    ----5-----5-----
    --5---------5---
    7-------6-------
    ----------------
    ----------------
    (this is one element of the list, each line is in a different string)
  */
  private static List<List<string>> convertBlockToTabMeasures(List<string> block)
  {
    List<string> remaining = block.Select(x => x).ToList();
    List<List<string>> tabMeasures = new List<List<string>>();

    while (true)
    {
      int leftBorder = remaining[0].IndexOf('|');
      int rightBorder = remaining[0].IndexOf('|', leftBorder + 1);

      if (rightBorder == -1) break;

      // confirm lines match
      for (int i = 1; i < remaining.Count; i++)
      {
        int rightBorderThisLine = remaining[i].IndexOf('|', leftBorder + 1);
        if (remaining[i][leftBorder] != '|' || rightBorderThisLine != rightBorder)
        {
          throw new Exception("Found block where measure borders do not match between lines");
        }
      }

      int numSpacers = remaining
        .Select(l => notSpacerRegex.Match(l.Substring(leftBorder + 1)).Index)
        .Min();
      
      int afterSpacers = leftBorder + numSpacers + 1;
      int cleanMeasureLength = rightBorder - afterSpacers;
      tabMeasures.Add(remaining.Select(l => l.Substring(afterSpacers, cleanMeasureLength)).ToList());

      // slurp
      remaining = remaining.Select(l => l.Substring(rightBorder)).ToList();
    }

    return tabMeasures;
  }

  /*
    given a measure such as:
    ------5-7-----7-
    ----5-----5-----
    --5---------5---
    7-------6-------
    ----------------
    ----------------

    parse it into a Measure object
  */
  private static Measure parseTabMeasure(List<string> tabMeasure, int[] tuning) {
    var measure = new Measure();

    // read notes from each line of the block
    for (int lineIndex = 0; lineIndex < tabMeasure.Count; lineIndex++)
    {
      // trim to the current measure
      var measureLine = tabMeasure[lineIndex];
      var noteMatches = noteRegex.Matches(measureLine);

      foreach (Match noteMatch in noteMatches)
      {
        var note = new Note();
        note.pitch = tuning[lineIndex] + Int32.Parse(noteMatch.Value);
        note.measureStart = ((float)noteMatch.Index) / (float) measureLine.Length; // TODO simple rhythm guessing
        measure.notes.Add(note);
      }
    }

    // sort measure by note order
    measure.notes.Sort((Note a, Note b) =>
    {
      return a.measureStart.CompareTo(b.measureStart);
    });

    return measure;
  }

  public override bool Equals(object obj)
  {
    if (obj.GetType() != typeof(Song)) return false;

    return this.measures.SequenceEqual(((Song)obj).measures);
  }
}