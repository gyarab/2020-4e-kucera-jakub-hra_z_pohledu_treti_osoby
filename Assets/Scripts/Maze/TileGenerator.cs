using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour, ITileGenerator
{
    [SerializeField]
    private TilesSO[] _tileSets;

    private SubcellData _subcellData;

    public void GenerateTiles(SubcellData subcellData, int count)
    {
        _subcellData = subcellData;

        SpawnTiles(count);
    }

    private void SpawnTiles(int count) // TODO choose one method
    {
        for (int i = 0; i < count; i++)
        {
            SpawnTilePrefab(_subcellData.Subcells[i]);
        }
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

    public void SpawnSingleTile(Vector3 position, float yRotation, int tilePrefabNumber,int tileType)
    {
        Instantiate(_tileSets[tileType].tiles[tilePrefabNumber], position, Quaternion.Euler(new Vector3(0, yRotation, 0)), transform);
    }
}

