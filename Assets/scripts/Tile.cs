public enum HeightType
{
    DeepWater = 1,
    ShallowWater = 2,
    Shore = 3,
    Sand = 4,
    Grass = 5,
    Forest = 6,
    Rock = 7,
    Snow = 8
}

public enum HeatType {
    Coldest = 0,
    Colder = 1,
    Cold = 2,
    Warm = 3,
    Warmer = 4,
    Warmest = 5
}

public enum MoistureType {
    Wettest,
    Wetter,
    Wet,
    Dry,
    Drier,
    Driest
}

public class Tile {

	public HeightType heightType;
	public float heightValue { get; set; }

    public HeatType heatType;
    public float heatValue { get; set; }

    public MoistureType moistureType;
    public float moistureValue { get; set; }
    
    public int x, y;

    public Tile right, top, left, bottom;
    public int bitmask;

    public bool isCollidable;
    public bool isFloodFilled;

    public Tile ()
    {

    }

    public void UpdateBitmask ()
    {
        int count = 0;

        if (top.heightType == heightType)
        {
            count += 1;
        }
        if (right.heightType == heightType)
        {
            count += 2;
        }
        if (bottom.heightType == heightType)
        {
            count += 4;
        }
        if (left.heightType == heightType)
        {
            count += 8;
        }

        bitmask = count;
    }

}
