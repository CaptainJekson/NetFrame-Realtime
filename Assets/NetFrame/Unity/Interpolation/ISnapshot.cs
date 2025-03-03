namespace NetFrame.Unity.Interpolation
{
    public interface ISnapshot
    {
        double RemoteTime { get; set; }
        double LocalTime { get; set; }
    }
}
