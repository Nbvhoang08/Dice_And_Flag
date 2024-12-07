using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnItemManger : MonoBehaviour
{
    public GameObject ItemBox;
    public Tilemap tilemap;
    public float spawnInterval = 5f; // Thời gian giữa các lần spawn
    private int maxItems = 3; // Số lượng hộp item tối đa cùng lúc
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnItems());
    }

    IEnumerator SpawnItems()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (spawnedItems.Count < maxItems)
            {
                Vector3Int randomCell = GetRandomCell();
                Vector3 spawnPosition = tilemap.CellToWorld(randomCell) + tilemap.tileAnchor;
                GameObject newItem = Instantiate(ItemBox, spawnPosition, Quaternion.identity);
                spawnedItems.Add(newItem);
            }
        }
    }

    Vector3Int GetRandomCell()
    {
        BoundsInt bounds = tilemap.cellBounds;
        List<Vector3Int> allCells = new List<Vector3Int>();

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                allCells.Add(pos);
            }
        }

        if (allCells.Count > 0)
        {
            int randomIndex = Random.Range(0, allCells.Count);
            return allCells[randomIndex];
        }

        return Vector3Int.zero; // Trả về vị trí mặc định nếu không tìm thấy ô nào
    }
}
