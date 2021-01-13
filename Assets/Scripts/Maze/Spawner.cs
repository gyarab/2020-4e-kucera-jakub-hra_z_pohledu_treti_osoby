using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // TODO add parents?
    [SerializeField]
    private GameObject _enemyPrefab; // TODO field
    [SerializeField]
    private GameObject _groundItemPrefab;
    [SerializeField]
    private GameObject _returnPortal;
    [SerializeField]
    private GameObject _bossRoom, _bossRoomDoors, _boss;
    [SerializeField]
    float _zOuterRoomOffset, _zBossOffset, _yDoorOffset, _yGroundItemOffset;

    // Vytvoří instanci protivníka na určité pozici
    public void SpawnEnemy(Vector3 position)
    {
        Instantiate(_enemyPrefab, position, Quaternion.identity);
    }

    // Vytvoří instanci předmětu na určité pozici
    public void SpawnItem(Vector3 position, int itemID)
    {
        ItemObject item = GameManager.Instance.GetItemObjectByID(itemID);
        GroundItem groundItem = Instantiate(_groundItemPrefab, new Vector3(position.x, position.y + _yGroundItemOffset, + position.x), Quaternion.identity).GetComponent<GroundItem>();
        groundItem.SetVariables(item);
    }

    // Vytvoří instanci portálu na určité pozici
    public void SpawnReturnPortal(Vector3 position)
    {
        Instantiate(_returnPortal, position, Quaternion.identity);
    }

    // Vytvoří vedlejší místnost s bossem na určité pozici
    public GameObject SpawnBossRoom(Vector3 position, int side, float distanceBetweenCells)
    {
        float yRotation = side * 90f;

        position = AddDistanceInMainDirection(position, side, distanceBetweenCells * 0.5f);
        Instantiate(_bossRoomDoors, new Vector3(position.x, position.y + _yDoorOffset, position.z), Quaternion.Euler(0, yRotation, 0));

        position = AddDistanceInMainDirection(position, side, _zOuterRoomOffset);
        Instantiate(_bossRoom, position, Quaternion.Euler(0, yRotation, 0));

        position = AddDistanceInMainDirection(position, side, _zBossOffset - _zOuterRoomOffset);
        GameObject boss = Instantiate(_boss, position, Quaternion.Euler(0, yRotation - 180, 0));
        return boss;
    }

    // Posune vektor o danou vzdálenost daným směrem
    private Vector3 AddDistanceInMainDirection(Vector3 position, int side, float distanceToAdd)
    {
        switch (side)
        {
            case 0:
                position.z += distanceToAdd;
                break;
            case 1:
                position.x += distanceToAdd;
                break;
            case 2:
                position.z -= distanceToAdd;
                break;
            case 3:
                position.x -= distanceToAdd;
                break;
        }

        return position;
    }
}
