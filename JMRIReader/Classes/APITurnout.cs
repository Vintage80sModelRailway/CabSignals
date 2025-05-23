
    public class APITurnout
    {
    public string name { get; set; }
    public string userName { get; set; }
    public string comment { get; set; }
    public string[] properties { get; set; }
    public bool inverted { get; set; }
    public int state { get; set; }
    public int feedbackMode { get; set; }
    public int[] feedbackModes { get; set; }

}

public class APIBaseTurnout
{
    public int state { get; set; }
}
