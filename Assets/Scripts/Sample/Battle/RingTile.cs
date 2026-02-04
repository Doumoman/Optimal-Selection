using UnityEngine;
public enum RingTileType
{
    Wall,
    Hole
}

[System.Serializable]
public class RingGrid
{
    public RingTileType[] tiles = new RingTileType[360];

    public RingGrid()
    {
        tiles = new RingTileType[360];
        for (int i = 0; i < 360; i++) tiles[i] = RingTileType.Wall;
    }

    public RingTileType Get(int a) => tiles[(a % 360 + 360) % 360];
    public void Set(int a, RingTileType t) => tiles[(a % 360 + 360) % 360] = t;
}