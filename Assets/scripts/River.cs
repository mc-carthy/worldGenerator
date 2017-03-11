using UnityEngine;
using System.Collections.Generic;

public enum Direction {
    Right,
    Top,
    Left,
    Bottom
}


public class River {

	public int length;
    public List<Tile> tiles;
    public int ID;

    public int intersections;
    public float turnCount;
    public Direction CurrentDirection;

    public River (int id)
    {
        ID = id;
        tiles = new List<Tile>();
    }

    public void AddTile (Tile t)
    {
        t.SetRiverPath (this);
        tiles.Add (t);
    }
}

public class RiverGroup {
    
    public List<River> rivers = new List<River> ();

}
