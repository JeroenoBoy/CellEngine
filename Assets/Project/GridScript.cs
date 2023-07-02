using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;


public class GridScript : MonoBehaviour
{
    [SerializeField] private bool _canLayoutChange = true;
    [SerializeField] private int _untilNewLayout;
    private int _layoutCounter = 1;

    [SerializeField] private Tile[] _tiles;
    [HideInInspector] public Tilemap _dMap;

    [SerializeField] private UnityEvent _startDuel;
    [SerializeField] private UnityEvent _showNewRoom;

    public List<TileBase> _tileBase = new List<TileBase>();

    [Header("Room Requirements")]
    [SerializeField] private int _killGoalOfThisRoom;

    private void OnEnable()
    {
        if (_dMap == null) { _dMap = GetComponent<Tilemap>(); }
    }

    /// <summary>
    /// PlayerMoved is called when the player has made an input that corresponds 
    /// to movement this code only activates when the move is allowed by PositionIsATile()
    /// </summary>
    /// <param name="playerPos"></param>
    public void PlayerMoved(Vector3 playerPos)
    {
        Vector3Int cellPos = _dMap.WorldToCell(playerPos);

        if (_dMap.GetTile(cellPos).name == _tiles[0].name) { _startDuel.Invoke(); }

        if (_canLayoutChange)
        {
            _layoutCounter--;

            if (_layoutCounter == 0)
            {
                SetWorldTiles(_dMap.WorldToCell(playerPos));
                _layoutCounter = _untilNewLayout;
            }
        }
    }

    public void SpawnKey()
    {
        SetWorldTiles(_dMap.WorldToCell(Vector3.zero), false);
    }

    public void SpawnDefaultTile()
    {
        SetWorldTiles(_dMap.WorldToCell(Vector3.zero), false);
    }

    /// <summary>
    /// SetWorldTiles is used to generate the random layouts enemies can appear in
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="playerRequest"></param>
    public void SetWorldTiles(Vector3Int pos, bool playerRequest = true)
    {
        BoundsInt bounds = _dMap.cellBounds;
        TileBase[] allTiles = _dMap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                
                if (tile != null) // If there is a tile you can apply the following code
                {
                    Vector3Int cellPos = _dMap.WorldToCell(new Vector3(x, y));
                    if (playerRequest)
                    {
                        if (tile.name == "DungeonSet_1" || tile.name == "EnemySpawn")
                        {
                            int randVal = Random.Range(0, 100);

                            if (randVal <= 15 && cellPos != pos) { _dMap.SetTile(cellPos, _tiles[0]); }
                            else { _dMap.SetTile(cellPos, _tiles[1]); }
                        }
                    }
                    else
                    {
                        if (tile.name == "SpawnFloorTile")
                        {
                            _dMap.SetTile(cellPos, _tiles[1]);
                        }
                        else if (tile.name == "SpawnKeyTile")
                        {
                            _dMap.SetTile(cellPos, _tiles[2]);
                        }
                    }
                }
            }
        }
    }

    public Vector3Int FindStartTile()
    {
        BoundsInt bounds = _dMap.cellBounds;
        TileBase[] allTiles = _dMap.GetTilesBlock(bounds);
        Vector3Int cellPos = new Vector3Int();

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];

                if (tile != null && tile.name == "StartTile")
                {
                    cellPos = _dMap.WorldToCell(new Vector3(x, y));
                }
            }
        }
        return cellPos;
    }

}
