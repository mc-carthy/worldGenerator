using UnityEngine;
using System.Collections.Generic;

public enum TileGroupType {
    Water,
    Land
}

public class TileGroup {

	public TileGroupType type;
    public List<Tile> tiles;

    public TileGroup ()
    {
        tiles = new List<Tile> ();
    }

}
