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
	private float DeepWater = 0.2f;
	[SerializeField]
	private float ShallowWater = 0.4f;	
	[SerializeField]
	private float Sand = 0.5f;
	[SerializeField]
	private float Grass = 0.7f;
	[SerializeField]
	private float Forest = 0.8f;
	[SerializeField]
	private float Rock = 0.9f;

	[Header("Heat Map")]
	[SerializeField]
	int heatOctaves = 4;
	[SerializeField]
	double heatFrequency = 3.0;
	[SerializeField]
	float coldestValue = 0.05f;
	[SerializeField]
	float colderValue = 0.18f;
	[SerializeField]
	float coldValue = 0.4f;
	[SerializeField]
	float warmValue = 0.6f;
	[SerializeField]
	float warmerValue = 0.8f;


    private ImplicitFractal heightMap;
    private ImplicitCombiner heatMap;

    private MapData heightData;
    private MapData heatData;
    
    private Tile [,] tiles;
    
    private MeshRenderer heightMapRenderer;
    private MeshRenderer heatMapRenderer;

    private List<TileGroup> waters = new List<TileGroup> ();
    private List<TileGroup> lands = new List<TileGroup> ();

    private void Start ()
    {
        heightMapRenderer = transform.Find ("heightTexture").GetComponent<MeshRenderer> ();
        heatMapRenderer = transform.Find ("heatTexture").GetComponent<MeshRenderer> ();

        Initialise ();
        GetData ();
        LoadTiles ();

        UpdateNeighbours ();
        UpdateBitmasks ();
        FloodFill ();

        heightMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateHeightMapTexture (width, height, tiles);
        heatMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateHeatMapTexture (width, height, tiles);
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


    }

    // Get data from noise module
    private void GetData ()
    {
        heightData = new MapData (width, height);
        heatData = new MapData (width, height);

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

                // Keep track of the min/max values
                heightData.max = (heightValue > heightData.max) ? heightValue : heightData.max;
                heightData.min = (heightValue < heightData.min) ? heightValue : heightData.min;

                heatData.max = (heatValue > heatData.max) ? heatValue : heatData.max;
                heatData.min = (heatValue < heatData.min) ? heatValue : heatData.min;

                heightData.data [x, y] = heightValue;
                heatData.data [x, y] = heatValue;
            }
        }
    }

    // Build Tile array based on heightData
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

                float value = heightData.data [x, y];

                // Normalize value between 0 and 1
                value = (value - heightData.min) / (heightData.max - heightData.min);

                t.heightValue = value;

				//HeightMap Analyze
				if (value < DeepWater)
                {
					t.heightType = HeightType.DeepWater;
                    t.isCollidable = false;
				}
				else if (value < ShallowWater)
                {
					t.heightType = HeightType.ShallowWater;
                    t.isCollidable = false;
				}
				else if (value < Sand)
                {
					t.heightType = HeightType.Sand;
                    t.isCollidable = true;
				}
				else if (value < Grass)
                {
					t.heightType = HeightType.Grass;
                    t.isCollidable = true;
				}
				else if (value < Forest)
                {
					t.heightType = HeightType.Forest;
                    t.isCollidable = true;
				}
				else if (value < Rock)
                {
					t.heightType = HeightType.Rock;
                    t.isCollidable = true;
				}
				else
                {
					t.heightType = HeightType.Snow;
                    t.isCollidable = true;
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

                tiles [x, y] = t;
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
