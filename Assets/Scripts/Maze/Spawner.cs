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
    private GameObject _bossRoom;
    [SerializeField]
    float _zOuterRoomOffset;
    [SerializeField]
    private GameObject _bossRoomDoors;

    // Start is called before the first frame update
    void Awake()
    {
        // TODO load enemy and item prefabs from resources; prob not;
    }

    public void SpawnEnemy(Vector3 position)
    {
        Instantiate(_enemyPrefab, position, Quaternion.identity);
    }

    public void SpawnItem(Vector3 position, int itemID)
    {
        ItemObject item = GameManager.Instance.GetItemObjectByID(itemID);
        GroundItem groundItem = Instantiate(_groundItemPrefab, position, Quaternion.identity).GetComponent<GroundItem>();
        groundItem.SetVariables(item);
    }

    public void SpawnReturnPortal(Vector3 position)
    {
        Instantiate(_returnPortal, position, Quaternion.identity);
    }

    public void SpawnBossRoom(Vector3 position, int side, float distanceBetweenCells)
    {
        float yRotation = side * 90f;

        position = AddDistanceInMainDirection(position, side, distanceBetweenCells * 0.5f);
        Instantiate(_bossRoomDoors, position, Quaternion.Euler(0, yRotation, 0));

        // TODO Spawn boss too?

        position = AddDistanceInMainDirection(position, side, distanceBetweenCells * _zOuterRoomOffset);
        Instantiate(_bossRoom, position, Quaternion.Euler(0, yRotation, 0));
    }

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

    public void SpawnBoss(Vector3 position, Vector3 rotation)
    {
        Debug.Log("Spawn boss");
    }
}
