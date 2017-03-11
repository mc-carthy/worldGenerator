using UnityEngine;
using System.Collections.Generic;

public enum HeightType
{
    DeepWater = 1,
    ShallowWater = 2,
    Shore = 3,
    Sand = 4,
    Grass = 5,
    Forest = 6,
    Rock = 7,
    Snow = 8,
    River = 9
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

public enum BiomeType {
	Desert,
	Savanna,
	TropicalRainforest,
	Grassland,
	Woodland,
	SeasonalForest,
	TemperateRainforest,
	BorealForest,
	Tundra,
	Ice
}

public class Tile {

	public HeightType heightType;
	public float heightValue { get; set; }

    public HeatType heatType;
    public float heatValue { get; set; }

    public MoistureType moistureType;
    public float moistureValue { get; set; }

    public BiomeType biomeType;
    public int biomeBitmask;
    
    public int x, y;

    public Tile right, top, left, bottom;
    public int bitmask;

    public bool isCollidable;
    public bool isFloodFilled;

    public Color colour = Color.blue;

    public List<River> rivers = new List<River> ();

    public int riverSize { get; set; }

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

	public void UpdateBiomeBitmask ()
	{
		int count = 0;
		
		if (isCollidable && right != null && right.biomeType == biomeType)
			count += 1;
		if (isCollidable && top != null && top.biomeType == biomeType)
			count += 2;
		if (isCollidable && left != null && left.biomeType == biomeType)
			count += 4;
		if (isCollidable && bottom != null && bottom.biomeType == biomeType)
			count += 8;
		
		biomeBitmask = count;
	}

    public Direction GetLowestNeighbour ()
    {
        if (
            right.heightValue < top.heightValue &&
            right.heightValue < left.heightValue &&
            right.heightValue < bottom.heightValue
        )
        {
            return Direction.Right;
        }
        else if (
            top.heightValue < right.heightValue &&
            top.heightValue < left.heightValue &&
            top.heightValue < bottom.heightValue
        )
        {
            return Direction.Top;
        }
        else if (
            left.heightValue < right.heightValue &&
            left.heightValue < top.heightValue &&
            left.heightValue < bottom.heightValue
        )
        {
            return Direction.Left;
        }
        else if (
            bottom.heightValue < right.heightValue &&
            bottom.heightValue < top.heightValue &&
            bottom.heightValue < left.heightValue
        )
        {
            return Direction.Bottom;
        }
        else
        {
            return Direction.Bottom;
        }
    }

    public int GetRiverNeighbourCount (River river)
    {
        int count = 0;

        if (right.rivers.Count > 0 && right.rivers.Contains (river))
        {
            count++;
        }
        if (top.rivers.Count > 0 && top.rivers.Contains (river))
        {
            count++;
        }
        if (left.rivers.Count > 0 && left.rivers.Contains (river))
        {
            count++;
        }
        if (bottom.rivers.Count > 0 && bottom.rivers.Contains (river))
        {
            count++;
        }

        return count;
    }

    public void SetRiverPath (River river)
    {
        if (!isCollidable)
        {
            return;
        }

        if (!rivers.Contains (river))
        {
            rivers.Add (river);
        }
    }

    public void DigRiver (River river, int width)
    {
        SetRiverTile (river);
        riverSize = width;

        if (width == 1) {
			bottom.SetRiverTile (river);
			right.SetRiverTile (river);
			bottom.right.SetRiverTile (river);				
		}

		if (width == 2) {
			bottom.SetRiverTile (river);
			right.SetRiverTile (river);
			bottom.right.SetRiverTile (river);
			top.SetRiverTile (river);
			top.left.SetRiverTile (river);
			top.right.SetRiverTile (river);
			left.SetRiverTile (river);
			left.bottom.SetRiverTile (river);
		}

		if (width == 3) {
			bottom.SetRiverTile (river);
			right.SetRiverTile (river);
			bottom.right.SetRiverTile (river);
			top.SetRiverTile (river);
			top.left.SetRiverTile (river);
			top.right.SetRiverTile (river);
			left.SetRiverTile (river);
			left.bottom.SetRiverTile (river);
			right.right.SetRiverTile (river);
			right.right.bottom.SetRiverTile (river);
			bottom.bottom.SetRiverTile (river);
			bottom.bottom.right.SetRiverTile (river);
		}

		if (width == 4) {
			bottom.SetRiverTile (river);
			right.SetRiverTile (river);
			bottom.right.SetRiverTile (river);
			top.SetRiverTile (river);
			top.right.SetRiverTile (river);
			left.SetRiverTile (river);
			left.bottom.SetRiverTile (river);
			right.right.SetRiverTile (river);
			right.right.bottom.SetRiverTile (river);
			bottom.bottom.SetRiverTile (river);
			bottom.bottom.right.SetRiverTile (river);
			left.bottom.bottom.SetRiverTile (river);
			left.left.bottom.SetRiverTile (river);
			left.left.SetRiverTile (river);
			left.left.top.SetRiverTile (river);
			left.top.SetRiverTile (river);
			left.top.top.SetRiverTile (river);
			top.top.SetRiverTile (river);
			top.top.right.SetRiverTile (river);
			top.right.right.SetRiverTile (river);
		}	
    }

    private void SetRiverTile (River river)
    {
        SetRiverPath (river);
        heightType = HeightType.River;
        heightValue = 0;
        isCollidable = false;
    }

}
