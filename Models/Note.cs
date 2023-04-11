class Note
{
  public float measureStart { get; set; } // the fraction of the measure before the note starts
  public int pitch { get; set; } // the MIDI number of the note (or what it would be if out of range)
}