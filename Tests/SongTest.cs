[TestFixture]
public class MoneyTest{
  /// <summary>
  /// Test song parsing with an easy example
  /// </summary>
  /// 
  [Test]
  public void TrivialTab() 
  {
    string tab = "|-1-|\n|-1-|\n|-1-|\n|-1-|\n|-1-|\n|-1-|";
    Song song = new Song(tab, Song.STANDARD_TUNING);

    Assert.AreEqual(1, song.measures.Count);
    
    Measure expectedMeasure = new Measure();
    
    foreach (int stringPitch in Song.STANDARD_TUNING) {
      expectedMeasure.notes.Add(new Note(stringPitch + 1, 0));
    }

    // Assert.AreEqual(expectedMeasure, song.measures[0]);
    Assert.That(song.measures[0], Is.EqualTo(expectedMeasure));
  }
}