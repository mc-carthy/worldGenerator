using UnityEngine;
using System.Collections.Generic;
using AccidentalNoise;

public class Generator : MonoBehaviour {

	// Adjustable variables
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    // TODO
    [SerializeField]
    private int terrainOcatves;
    // TODO
    [SerializeField]
    private double terrainFrequency;
	[SerializeField]
	private int seed;
	[SerializeField]
	private bool useRandomSeed;

    [HeaderAttribute ("Height Map")]
	[SerializeField]
	private float deepWater = 0.2f;
	[SerializeField]
	private float shallowWater = 0.4f;	
	[SerializeField]
	private float sand = 0.5f;
	[SerializeField]
	private float grass = 0.7f;
	[SerializeField]
	private float forest = 0.8f;
	[SerializeField]
	private float rock = 0.9f;

	[HeaderAttribute ("Heat Map")]
	[SerializeField]
	private int heatOctaves = 4;
	[SerializeField]
	private double heatFrequency = 3.0;
	[SerializeField]
	private float coldestValue = 0.05f;
	[SerializeField]
	private float colderValue = 0.18f;
	[SerializeField]
	private float coldValue = 0.4f;
	[SerializeField]
	private float warmValue = 0.6f;
	[SerializeField]
	private float warmerValue = 0.8f;

	[HeaderAttribute ("Moisture Map")]
	[SerializeField]
	private int moistureOctaves = 4;
	[SerializeField]
	private double moistureFrequency = 3.0;
	[SerializeField]
	private float drierValue = 0.27f;
	[SerializeField]
	private float dryValue = 0.4f;
	[SerializeField]
	private float wetValue = 0.6f;
	[SerializeField]
	private float wetterValue = 0.8f;
	[SerializeField]
	private float wettestValue = 0.9f;

    [HeaderAttribute ("Rivers")]
    [SerializeField]
    private int totalRiverCount = 40;
	[SerializeField]
	private float minRiverHeight = 0.6f;
	[SerializeField]
	private int maxRiverAttempts = 1000;
	[SerializeField]
	private int minRiverTurns = 18;
	[SerializeField]
	private int minRiverLength = 20;
	[SerializeField]
	private int maxRiverIntersections = 2;


    private ImplicitFractal heightMap;
    private ImplicitCombiner heatMap;
    private ImplicitFractal moistureMap;

    private MapData heightData;
    private MapData heatData;
    private MapData moistureData;
    
    private Tile [,] tiles;
    
    private MeshRenderer heightMapRenderer;
    private MeshRenderer heatMapRenderer;
    private MeshRenderer moistureMapRenderer;

    private List<TileGroup> waters = new List<TileGroup> ();
    private List<TileGroup> lands = new List<TileGroup> ();

    private List<River> rivers = new List<River> ();
    private List<RiverGroup> riverGroups = new List<RiverGroup> ();

    private void Start ()
    {
        heightMapRenderer = transform.Find ("heightTexture").GetComponent<MeshRenderer> ();
        heatMapRenderer = transform.Find ("heatTexture").GetComponent<MeshRenderer> ();
        moistureMapRenderer = transform.Find ("moistureTexture").GetComponent<MeshRenderer> ();

        Initialise ();
        GetData ();
        LoadTiles ();

        UpdateNeighbours ();

        GenerateRivers ();

        UpdateBitmasks ();
        FloodFill ();

        heightMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateHeightMapTexture (width, height, tiles);
        heatMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateHeatMapTexture (width, height, tiles);
        moistureMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateMoistureMapTexture (width, height, tiles);
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            Initialise ();
            GetData ();
            LoadTiles ();
        }
    }

    private void Initialise ()
    {
		if (useRandomSeed)
		{
			seed = (int) System.DateTime.Now.Ticks;
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());

        // Initialise heightMap
        heightMap = new ImplicitFractal (
            FractalType.MULTI,
            BasisType.SIMPLEX,
            InterpolationType.QUINTIC,
            terrainOcatves,
            terrainFrequency,
            pseudoRandom.Next (0, int.MaxValue)
        );

        // Initialise heatMap
        ImplicitGradient gradient  = new ImplicitGradient (1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
        ImplicitFractal heatFractal = new ImplicitFractal (
            FractalType.MULTI,
            BasisType.SIMPLEX,
            InterpolationType.QUINTIC,
            heatOctaves,
            heatFrequency,
            pseudoRandom.Next (0, int.MaxValue)
        );

        heatMap = new ImplicitCombiner (CombinerType.MULTIPLY);
        heatMap.AddSource (gradient);
        heatMap.AddSource (heatFractal);

        // Initialise moistureMap
		moistureMap = new ImplicitFractal (
            FractalType.MULTI, 
		    BasisType.SIMPLEX,
            InterpolationType.QUINTIC, 
            moistureOctaves, 
            moistureFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );


    }

    // Get data from noise module
    private void GetData ()
    {
        heightData = new MapData (width, height);
        heatData = new MapData (width, height);
        moistureData = new MapData (width, height);

        // Get height data
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Noise range
                float x1 = 0, x2 = 1;
                float y1 = 0, y2 = 1;

                float dx = x2 - x1;
                float dy = y2 - y1;

                // Get samples at smaller intervals
                float s = x / (float) width;
                float t = y / (float) height;

                // Calculate 4D coords
                float nx = x1 + Mathf.Cos (s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float ny = y1 + Mathf.Cos (t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                float nz = x1 + Mathf.Sin (s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float nw = y1 + Mathf.Sin (t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);

                float heightValue = (float) heightMap.Get (nx, ny, nz, nw);
                float heatValue = (float) heatMap.Get (nx, ny, nz, nw);
				float moistureValue = (float) moistureMap.Get (nx, ny, nz, nw);

                // Keep track of the min/max values
                heightData.max = (heightValue > heightData.max) ? heightValue : heightData.max;
                heightData.min = (heightValue < heightData.min) ? heightValue : heightData.min;

                heatData.max = (heatValue > heatData.max) ? heatValue : heatData.max;
                heatData.min = (heatValue < heatData.min) ? heatValue : heatData.min;

                moistureData.max = (moistureValue > moistureData.max) ? moistureValue : moistureData.max;
                moistureData.min = (moistureValue < moistureData.min) ? moistureValue : moistureData.min;

                heightData.data [x, y] = heightValue;
                heatData.data [x, y] = heatValue;
                moistureData.data [x, y] = moistureValue;
            }
        }
    }

    // Build Tile array based on data maps
    private void LoadTiles ()
    {
        tiles = new Tile [width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = new Tile ();
                t.x = x;
                t.y = y;

                float heightValue = heightData.data [x, y];

                // Normalize value between 0 and 1
                heightValue = (heightValue - heightData.min) / (heightData.max - heightData.min);

                t.heightValue = heightValue;

				//HeightMap Analyze
				if (heightValue < deepWater)
                {
					t.heightType = HeightType.DeepWater;
                    t.isCollidable = false;
				}
				else if (heightValue < shallowWater)
                {
					t.heightType = HeightType.ShallowWater;
                    t.isCollidable = false;
				}
				else if (heightValue < sand)
                {
					t.heightType = HeightType.Sand;
                    t.isCollidable = true;
				}
				else if (heightValue < grass)
                {
					t.heightType = HeightType.Grass;
                    t.isCollidable = true;
				}
				else if (heightValue < forest)
                {
					t.heightType = HeightType.Forest;
                    t.isCollidable = true;
				}
				else if (heightValue < rock)
                {
					t.heightType = HeightType.Rock;
                    t.isCollidable = true;
				}
				else
                {
					t.heightType = HeightType.Snow;
                    t.isCollidable = true;
				}

                // Adjust heat map based on height, higher is colder
                if (t.heightType == HeightType.Forest)
                {
                    heatData.data [x, y] -= 0.1f * t.heightValue;
                }
                else if (t.heightType == HeightType.Rock)
                {
                    heatData.data [x, y] -= 0.25f * t.heightValue;
                }
                else if (t.heightType == HeightType.Snow)
                {
                    heatData.data [x, y] -= 0.4f * t.heightValue;
                }
                else
                {
                    heatData.data [x, y] += 0.1f * t.heightValue;
                }

                // Set heat value
                float heatValue = heatData.data [x, y];
                heatValue = (heatValue - heatData.min) / (heatData.max - heatData.min);
                t.heatValue = heatValue;

                // Set heat type
                if (heatValue < coldestValue)
                {
                    t.heatType = HeatType.Coldest;
                }
                else if (heatValue < colderValue)
                {
                    t.heatType = HeatType.Colder;
                }
                else if (heatValue < coldValue)
                {
                    t.heatType = HeatType.Cold;
                }
                else if (heatValue < warmValue)
                {
                    t.heatType = HeatType.Warm;
                }
                else if (heatValue < warmerValue)
                {
                    t.heatType = HeatType.Warmer;
                }
                else
                {
                    t.heatType = HeatType.Warmest;
                }

                // Adjust moisture map based on height
                if (t.heightType == HeightType.DeepWater)
                {
                    moistureData.data [x, y] += 8f * t.heightValue;
                }
                else if (t.heightType == HeightType.ShallowWater)
                {
                    moistureData.data [x, y] += 3f * t.heightValue;
                }
                else if (t.heightType == HeightType.Shore)
                {
                    moistureData.data [x, y] += 1f * t.heightValue;
                }
                else if (t.heightType == HeightType.Sand)
                {
                    moistureData.data [x, y] += 0.25f * t.heightValue;
                }

                // Set moisture value
                float moistureValue = moistureData.data [x, y];
                moistureValue = (moistureValue - moistureData.min) / (moistureData.max - moistureData.min);
                t.moistureValue = moistureValue;

                if (moistureValue < drierValue)
                {
                    t.moistureType = MoistureType.Driest;
                }
                else if (moistureValue < dryValue)
                {
                    t.moistureType = MoistureType.Drier;
                }
                else if (moistureValue < wetValue)
                {
                    t.moistureType = MoistureType.Dry;
                }
                else if (moistureValue < wetterValue)
                {
                    t.moistureType = MoistureType.Wet;
                }
                else if (moistureValue < wettestValue)
                {
                    t.moistureType = MoistureType.Wetter;
                }
                else
                {
                    t.moistureType = MoistureType.Wettest;
                }

                tiles [x, y] = t;
            }
        }
    }

    private void GenerateRivers ()
    {
        int attempts = 0;
        int riverCount = totalRiverCount;
        rivers = new List<River> ();

        while (riverCount > 0 && attempts < maxRiverAttempts)
        {
            int x = Random.Range (0, width);
            int y = Random.Range (0, height);
            Tile tile = tiles [x, y];

            // Validation
            if (!tile.isCollidable)
            {
                continue;
            }
            if (tile.rivers.Count > 0)
            {
                continue;
            }

            if (tile.heightValue > minRiverHeight)
            {
                River river = new River (riverCount);

                // Figure out initial direction
                river.CurrentDirection = tile.GetLowestNeighbour ();

                // Recursively find way to water
                FindPathToWater (tile, river.CurrentDirection, ref river);

                // Validate generated river
                if (river.turnCount < minRiverTurns || river.tiles.Count < minRiverLength || river.intersections > maxRiverIntersections)
                {
                    // Remove this river
                    for (int i = 0; i < river.tiles.Count; i++)
                    {
                        Tile t = river.tiles [i];
                        t.rivers.Remove (river);
                    }
                }
                else
                {
                    // Add river to list
                    rivers.Add (river);
                    tile.rivers.Add (river);
                    riverCount--;
                }
            }
            attempts++;
        }
    }

    private void FindPathToWater (Tile tile, Direction direction, ref River river)
    {
        if (tile.rivers.Contains (river))
        {
            return;
        }

        if (tile.rivers.Count > 0)
        {
            river.intersections++;
        }

        river.AddTile (tile);

        // Get neighbours
        Tile right = GetRight (tile);
        Tile top = GetTop (tile);
        Tile left = GetLeft (tile);
        Tile bottom = GetBottom (tile);

        float rightValue = float.MaxValue;
        float topValue = float.MaxValue;
        float leftValue = float.MaxValue;
        float bottomValue = float.MaxValue;

        // Check height value of neighbours
        if (right.GetRiverNeighbourCount (river) < 2 && !river.tiles.Contains (right))
        {
            rightValue = right.heightValue;
        }
        if (top.GetRiverNeighbourCount (river) < 2 && !river.tiles.Contains (top))
        {
            topValue = top.heightValue;
        }
        if (left.GetRiverNeighbourCount (river) < 2 && !river.tiles.Contains (left))
        {
            leftValue = left.heightValue;
        }
        if (bottom.GetRiverNeighbourCount (river) < 2 && !river.tiles.Contains (bottom))
        {
            bottomValue = bottom.heightValue;
        }

        // If the neighbour tile has a river that is not this one, flow into it
        if (right.rivers.Count == 0 && !right.isCollidable)
        {
            rightValue = 0;
        }
        if (top.rivers.Count == 0 && !top.isCollidable)
        {
            topValue = 0;
        }
        if (left.rivers.Count == 0 && !left.isCollidable)
        {
            leftValue = 0;
        }
        if (bottom.rivers.Count == 0 && !bottom.isCollidable)
        {
            bottomValue = 0;
        }

        // Override flow direction if the neighbour tile is significantly lower
        if (direction == Direction.Right && Mathf.Abs (rightValue - leftValue) < 0.1f)
        {
            rightValue = int.MaxValue;
        }
        if (direction == Direction.Top && Mathf.Abs (topValue - bottomValue) < 0.1f)
        {
            topValue = int.MaxValue;
        }
        if (direction == Direction.Left && Mathf.Abs (rightValue - leftValue) < 0.1f)
        {
            leftValue = int.MaxValue;
        }
        if (direction == Direction.Bottom && Mathf.Abs (topValue - bottomValue) < 0.1f)
        {
            bottomValue = int.MaxValue;
        }

        // Find local miniumum
        float min = Mathf.Min (Mathf.Min (Mathf.Min (rightValue, topValue), leftValue), bottomValue);

        // If no minumum found, break
        if (min == int.MaxValue)
        {
            return;
        }

        // Move to next neighbour
        if (min == rightValue)
        {
            if (right.isCollidable)
            {
                if (river.CurrentDirection != Direction.Right)
                {
                    river.turnCount++;
                    river.CurrentDirection = Direction.Right;
                }
                FindPathToWater (right, direction, ref river);
            }
        }
        else if (min == topValue)
        {
            if (top.isCollidable)
            {
                if (river.CurrentDirection != Direction.Top)
                {
                    river.turnCount++;
                    river.CurrentDirection = Direction.Top;
                }
                FindPathToWater (top, direction, ref river);
            }
        }
        else if (min == leftValue)
        {
            if (left.isCollidable)
            {
                if (river.CurrentDirection != Direction.Left)
                {
                    river.turnCount++;
                    river.CurrentDirection = Direction.Left;
                }
                FindPathToWater (left, direction, ref river);
            }
        }
        else if (min == bottomValue)
        {
            if (bottom.isCollidable)
            {
                if (river.CurrentDirection != Direction.Bottom)
                {
                    river.turnCount++;
                    river.CurrentDirection = Direction.Bottom;
                }
                FindPathToWater (bottom, direction, ref river);
            }
        }
        
    }

    
    private void BuildRiverGroups ()
    {
        // Check each tile to see if owned by multiple rivers
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = tiles [x, y];

                if (t.rivers.Count > 1)
                {
                    RiverGroup group = null;

                    // Check if a river group already exists for this group
                    for (int n = 0; n < t.rivers.Count; n++)
                    {
                        River tileRiver = t.rivers [n];

                        for (int i = 0; i < riverGroups.Count; i++)
                        {
                            for (int j = 0; j < riverGroups [i].rivers.Count; j++)
                            {
                                River river = riverGroups [i].rivers [j];
                                if (river.ID == tileRiver.ID)
                                {
                                    group = riverGroups [i];
                                }
                                if (group != null) break;
                            }
                            if (group != null) break;
                        }
                        if (group != null) break;
                    }
                    // Existing group found, add to it
                    if (group != null)
                    {
                        for (int n = 0; n < t.rivers.Count; n++)
                        {
                            if (!group.rivers.Contains (t.rivers [n]))
                            {
                                group.rivers.Add (t.rivers [n]);
                            }
                        }
                    }
                    // No existing group found, create a new one
                    else
                    {
                        group = new RiverGroup ();
                        for (int n = 0; n < t.rivers.Count; n++)
                        {
                            group.rivers.Add (t.rivers [n]);
                        }
                        riverGroups.Add (group);
                    }
                }
            }
        }
    }

    private void UpdateNeighbours ()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = tiles [x, y];

                t.right = GetRight (t);
                t.top = GetTop (t);
                t.left = GetLeft (t);
                t.bottom = GetBottom (t);
            }
        }
    }

    private void UpdateBitmasks ()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles [x, y].UpdateBitmask ();
            }
        }
    }

    private void FloodFill ()
    {
        Stack<Tile> stack = new Stack<Tile> ();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = tiles [x, y];

                // If the tile has already been flood filled, skip
                if (t.isFloodFilled)
                {
                    continue;
                }

                // Land check
                if (t.isCollidable)
                {
                    TileGroup group = new TileGroup ();
                    group.type = TileGroupType.Land;
                    stack.Push (t);

                    while (stack.Count > 0)
                    {
                        FloodFill (stack.Pop (), ref group, ref stack);
                    }

                    if (group.tiles.Count > 0)
                    {
                        lands.Add (group);
                    }
                }
                // Water check
                else
                {
                    TileGroup group = new TileGroup ();
                    group.type = TileGroupType.Water;
                    stack.Push (t);

                    while (stack.Count > 0)
                    {
                        FloodFill (stack.Pop (), ref group, ref stack);
                    }

                    if (group.tiles.Count > 0)
                    {
                        waters.Add (group);
                    }
                }
            }
        }
    }

    private void FloodFill (Tile tile, ref TileGroup tiles, ref Stack<Tile> stack)
    {
        // Validation
        if (tile.isFloodFilled)
        {
            return;
        }
        if (tiles.type == TileGroupType.Land && !tile.isCollidable)
        {
            return;
        }
        if (tiles.type == TileGroupType.Water && tile.isCollidable)
        {
            return;
        }

        // Add to TileGroup
        tiles.tiles.Add (tile);
        tile.isFloodFilled = true;

        // Flood into neighbours (orthographic)
        Tile t = GetRight (tile);
        if (!t.isFloodFilled && tile.isCollidable == t.isCollidable)
        {
            stack.Push (t);
        }
        t = GetTop (tile);
        if (!t.isFloodFilled && tile.isCollidable == t.isCollidable)
        {
            stack.Push (t);
        }
        t = GetLeft (tile);
        if (!t.isFloodFilled && tile.isCollidable == t.isCollidable)
        {
            stack.Push (t);
        }
        t = GetBottom (tile);
        if (!t.isFloodFilled && tile.isCollidable == t.isCollidable)
        {
            stack.Push (t);
        }
    }

    // Get Tile neighbours
    private Tile GetRight (Tile t)
    {
        return tiles [MathHelper.Mod (t.x + 1, width), t.y];
    }
    private Tile GetTop (Tile t)
    {
        return tiles [t.x, MathHelper.Mod (t.y - 1, height)];
    }
    private Tile GetLeft (Tile t)
    {
        return tiles [MathHelper.Mod (t.x - 1, width), t.y];
    }
    private Tile GetBottom (Tile t)
    {
        return tiles [t.x, MathHelper.Mod (t.y + 1, height)];
    }

}
