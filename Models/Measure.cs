public class Measure
{
  public List<Note> notes { get; }

  public Measure()
  {
    this.notes = new List<Note>();
  }

  public override bool Equals(object obj) {
    if (obj.GetType() != typeof(Measure)) return false;

    var other = (Measure) obj;

    if (other.notes.Count != this.notes.Count) return false;
    for (int i = 0; i < this.notes.Count; i++) {
      if (!other.notes[i].Equals(this.notes[i])) return false;
    }

    return true;
  }
}
