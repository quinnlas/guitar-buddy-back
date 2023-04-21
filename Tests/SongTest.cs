// official examples: https://github.com/nunit/nunit-csharp-samples/blob/master/money/MoneyTest.cs

[TestFixture]
public class SongTest
{
  private string trivialTab;
  private Song trivialSong;

  /// <summary>
  /// Initializes Song test objects
  /// </summary>
  /// 
  [SetUp]
  protected void SetUp()
  {
    trivialTab = "|-1-|\n|-1-|\n|-1-|\n|-1-|\n|-1-|\n|-1-|";
    trivialSong = new Song();
    Measure trivialSongMeasure = new Measure();
    foreach (int stringPitch in Song.STANDARD_TUNING)
    {
      trivialSongMeasure.notes.Add(new Note(stringPitch + 1, 0));
    }
    trivialSong.measures.Add(trivialSongMeasure);
  }

  /// <summary>
  /// Test song parsing with an easy example
  /// </summary>
  /// 
  [Test]
  public void TrivialTab()
  {
    Song song = new Song(trivialTab, Song.STANDARD_TUNING);
    Assert.That(song, Is.EqualTo(trivialSong));
  }

  // tests for finding blocks
  [Test]
  public void startsAndEndsWithUnrelatedLines()
  {
    string tab = "\n" + trivialTab + "\n";

    Song song = new Song(tab, Song.STANDARD_TUNING);
    Assert.That(song, Is.EqualTo(trivialSong));
  }

  // test with characters before the block on the same line (such as tuning letters)
  [Test]
  public void charactersBeforeBlock()
  {
    // a|-1-|
    string tab = String.Join('\n', trivialTab.Split('\n').Select((line, i) => "abcdef"[i] + line));

    Song song = new Song(tab, Song.STANDARD_TUNING);
    Assert.That(song, Is.EqualTo(trivialSong));
  }

  // tests for parsing blocks
  [Test]
  public void noBeginningSpacer()
  {
    // |1-|
    string tab = String.Join('\n', trivialTab.Split('\n').Select(line => line.Remove(1, 1)));

    Song song = new Song(tab, Song.STANDARD_TUNING);
    Assert.That(song, Is.EqualTo(trivialSong));
  }

  [Test]
  public void twoMeasuresInOneBlock()
  {
    // |-1-|-1-|
    string tab = String.Join('\n', trivialTab.Split('\n').Select(line => line + line.Substring(1)));

    Song expected = new Song();
    expected.measures.Add(trivialSong.measures[0]);
    expected.measures.Add(trivialSong.measures[0]);

    Song song = new Song(tab, Song.STANDARD_TUNING);
    Assert.That(song, Is.EqualTo(expected));
  }

  [Test]
  public void noteInMiddle()
  {
    // |-1-1-|
    string tab = String.Join('\n', trivialTab.Split('\n').Select(line => line.Substring(0, 4) + line.Substring(2)));

    Song expected = new Song();
    Measure expectedMeasure = new Measure();
    foreach (int stringPitch in Song.STANDARD_TUNING)
    {
      expectedMeasure.notes.Add(new Note(stringPitch + 1, 0));
      expectedMeasure.notes.Add(new Note(stringPitch + 1, 0.5));
    }

    expectedMeasure.notes.Sort((Note a, Note b) =>
      {
        return a.measureStart.CompareTo(b.measureStart);
      });
    expected.measures.Add(expectedMeasure);

    Song song = new Song(tab, Song.STANDARD_TUNING);

    Assert.That(song, Is.EqualTo(expected));
  }

  // test for parsing two digit note
  [Test]
  public void twoDigitNotes()
  {
    // |-11-|
    string tab = String.Join('\n', trivialTab.Split('\n').Select(line => line.Substring(0, 3) + line.Substring(2)));

    Song expected = new Song();
    Measure expectedMeasure = new Measure();
    foreach (int stringPitch in Song.STANDARD_TUNING)
    {
      expectedMeasure.notes.Add(new Note(stringPitch + 11, 0));
    }
    expected.measures.Add(expectedMeasure);

    Song song = new Song(tab, Song.STANDARD_TUNING);

    Assert.That(song, Is.EqualTo(expected));
  }

  // test block where measure borders don't match up (throws)
  [Test]
  public void badLeftBorder()
  {
    // |-1-|
    // a|-1|
    string tab = String.Join('\n', trivialTab.Split('\n').Select((line, i) => i == 1 ? "a|-1|" : line));

    Assert.Throws<Exception>(() => new Song(tab, Song.STANDARD_TUNING));
  }

  // test block where measure borders don't match up (throws)
  [Test]
  public void badRightBorder()
  {
    // |-1-|
    // |-1|
    string tab = String.Join('\n', trivialTab.Split('\n').Select((line, i) => i == 1 ? "|-1|" : line));

    Assert.Throws<Exception>(() => new Song(tab, Song.STANDARD_TUNING));
  }
}