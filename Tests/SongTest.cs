[TestFixture]
public class MoneyTest{
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
    foreach (int stringPitch in Song.STANDARD_TUNING) {
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

    Assert.AreEqual(1, song.measures.Count);
    
    // Assert.AreEqual(expectedMeasure, song.measures[0]);
    Assert.That(song, Is.EqualTo(trivialSong));
  }
}