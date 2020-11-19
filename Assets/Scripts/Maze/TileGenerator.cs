using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour, ITileGenerator
{
    [SerializeField]
    private TilesSO[] _tileSets;

    private SubcellData _subcellData;

    public void GenerateTiles(SubcellData subcellData)
    {
        _subcellData = subcellData;

        SpawnTiles();
    }

    private void SpawnTiles() // TODO choose one method
    {
        for (int i = 0; i < _subcellData.Subcells.Length; i++)
        {
            SpawnTilePrefab(_subcellData.Subcells[i]);
        }

        /*Vector2 dimensions;

        for (int i = 0; i < _mazeSettings.length; i++)
        {
            for (int j = 0; j < _mazeSettings.width; j++)
            {
                if (_cellData.Cells[i, j] != null)
                {

                    dimensions = GetDimensions(i, j);
                    for (int k = 0; k < dimensions.x * dimensions.y; k++)
                    {
                        SpawnTilePrefab(_pathfindingNodes[_cellData.Cells[i, j].lowestPathfindingNodeID + k]);
                    }
                }
            }
        }*/
    }

    private void SpawnTilePrefab(Subcell subcell)
    {
        if (subcell != null) // TODO do smth about that?
        {
            int firstDoor = subcell.GetFirstDoor();

            switch (subcell.GetDoorCount())
            {
                case 1:
                    Instantiate(_tileSets[subcell.TileType].tiles[0], subcell.Position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    break;
                case 2:
                    if (subcell.Neighbours[((firstDoor + 2) * 2) % 8] == null)
                    {
                        Instantiate(_tileSets[subcell.TileType].tiles[1], subcell.Position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    else
                    {
                        Instantiate(_tileSets[subcell.TileType].tiles[2], subcell.Position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    break;
                case 3:
                    Instantiate(_tileSets[subcell.TileType].tiles[3], subcell.Position, Quaternion.Euler(new Vector3(0, (firstDoor - 1) * 90, 0)), transform);
                    break;
                case 4:
                    Instantiate(_tileSets[subcell.TileType].tiles[4], subcell.Position, Quaternion.identity, transform);
                    break;
                default:
                    throw new System.Exception("Cell can't have more than four doors");
            }
        }
    }
}

