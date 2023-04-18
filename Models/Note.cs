public class Note
{
  public int pitch { get; set; } // the MIDI number of the note (or what it would be if out of range)
  public float measureStart { get; set; } // the fraction of the measure before the note starts

  public Note() {}
  public Note(int pitch, float measureStart) {
    this.pitch = pitch;
    this.measureStart = measureStart;
  }

  public override bool Equals(object obj) {
    if (obj.GetType() != typeof(Note)) return false;

    Note other = (Note) obj;

    return other.pitch == this.pitch && other.measureStart == this.measureStart;
  }
}