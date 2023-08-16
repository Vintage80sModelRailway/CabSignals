
public class SMRootobject
{
    public string type { get; set; }
    public APISignalMast data { get; set; }
}

public class APISignalMast
{
    public string name { get; set; }
    public string userName { get; set; }
    public object comment { get; set; }
    public Property1[] properties { get; set; }
    public string aspect { get; set; }
    public bool lit { get; set; }
    public bool held { get; set; }
    public string state { get; set; }
}

public class Property1
{
    public string intermediateSignal { get; set; }
}
