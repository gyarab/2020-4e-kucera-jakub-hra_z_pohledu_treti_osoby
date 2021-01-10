using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour, ITileGenerator
{
    [SerializeField]
    private TilesSO[] _tileSets;

    // Vytvoří instance částí mísntností na mapě
    public void GenerateTiles(SubcellData subcellData, int count)
    {
        SpawnTiles(subcellData.Subcells, count);
    }

    // Pro každou podbuňku zavolá metodu, která umístí na mapě část místnosti
    private void SpawnTiles(Subcell[] array, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (array[i] != null)
            {
                SpawnTilePrefab(array[i]);
            }
        }
    }

    // Vytvoří instanci části místnosti na mapě podle poču otevřených zdí
    private void SpawnTilePrefab(Subcell subcell)
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

    // Vytvoří instanci části místnosti na mapě podle parametrů oředaných metodě
    public void SpawnSingleTile(Vector3 position, float yRotation, int tilePrefabNumber,int tileType)
    {
        Instantiate(_tileSets[tileType].tiles[tilePrefabNumber], position, Quaternion.Euler(new Vector3(0, yRotation, 0)), transform);
    }
}

