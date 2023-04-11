using System.Text.RegularExpressions;

class Song
{
  public List<Measure> measures { get; }

  public Song(String tabText)
  {
    // parse tabText into a Song

    // break the text into "blocks"
    // blocks are consecutive lines that are part of the actual tab
    // since the text will have filler stuff like song section names, lyrics, legend

    // starts and ends with |
    // includes - | / \ ( ) _ and alphanumeric
    // | is the measure end character
    // - is a space/rest character
    // /\ are for slides
    // () are for grouping
    // _ is for sustaining a note
    // 0-9 are the fret numbers
    // hp are for hammer-on and pickup
    // br are for bend and release

    int[] tuning = new[] { 40, 45, 50, 55, 59, 64 };

    // find consecutive lines
    string[] lines = tabText.Split('\n');
    List<List<string>> blocks = new List<List<string>>();
    List<string> currentBlockLines = new List<string>();

    for (int i = 0; i < lines.Length; i++)
    {
      // TODO where to store regexes
      Regex blockLineRx = new Regex(@"\|[\-|\/\\(\)_0-9hpbr]*\|");
      if (blockLineRx.IsMatch(lines[i]))
      {
        currentBlockLines.Add(lines[i]);
      }
      else if (currentBlockLines.Count > 0)
      {
        // handle end of block
        blocks.Add(currentBlockLines);
        currentBlockLines = new List<string>();
      }
    }

    this.measures = new List<Measure>();

    // todo split into organized functions
    // parse blocks into measures
    foreach (List<string> block in blocks)
    {
      int startIndex = block[0].IndexOf('|') + 2; // skip start of first measure (assumes extra spacer)
      while (startIndex < block[0].Length - 1)
      {
        Measure measure = new Measure();
        int measureEnd = block[0].IndexOf('|', startIndex);

        // validate that remaining lines have the same length of measures
        for (int lineIndex = 1; lineIndex < block.Count; lineIndex++)
        {
          int thisLineMeasureEnd = block[0].IndexOf('|', startIndex);
          if (thisLineMeasureEnd != measureEnd) throw new Exception("malformed block (measures are not the same length)");
        }

        // TODO won't work with multiple digit frets
        // assumes extra dash at start
        int numParts = measureEnd; // the denominator of the fraction of a measure that each character represents
        // iterate through each column and take the note TODO multiple digit notes
        for (int i = 0; i < measureEnd; i++)
        {
          // iterate through each block line
          for (int j = 0; j < block.Count; j++)
          {
            string currentChar = block[j][i] + "";
            Regex noteRx = new Regex(@"[0-9]");
            if (noteRx.IsMatch(currentChar))
            {
              Note note = new Note();
              note.measureStart = ((float)i) / ((float)numParts);
              note.pitch = tuning[j] + Int32.Parse(currentChar);
              measure.notes.Add(note);
            }
          }
        }

        this.measures.Add(measure);
        startIndex = measureEnd + 1;
      }
    }
  }
}