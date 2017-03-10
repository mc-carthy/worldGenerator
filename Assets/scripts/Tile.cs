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

public class Tile {

	public HeightType heightType;
	public float heightValue { get; set; }
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
