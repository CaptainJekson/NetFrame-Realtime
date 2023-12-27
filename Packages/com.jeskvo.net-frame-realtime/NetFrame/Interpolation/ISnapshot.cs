namespace NetFrame.Interpolation
{
    public interface ISnapshot
    {
        double RemoteTime { get; set; }
        double LocalTime { get; set; }
    }
}
