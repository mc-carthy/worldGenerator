public class MapData {

	public float [,] data;
    public float min { get; set; }
    public float max { get; set; }

    public MapData (int width, int height)
    {
        data = new float [width, height];
        min = float.MaxValue;
        max = float.MinValue;
    }

}
