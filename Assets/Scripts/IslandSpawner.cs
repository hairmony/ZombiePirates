using UnityEngine;

public class IslandSpawner : MonoBehaviour
{
    public GameObject[] islandPrefabs;
    public int islandCount = 10;
    
    public float spawnRangeX = 50f;
    public float spawnRangeY = 50f;

    void Start()
    {
        for(int i=0; i<islandCount; i++) 
        {
            Vector2 pos = new Vector2(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(-spawnRangeY, spawnRangeY)
            );

            GameObject island = islandPrefabs[
                Random.range(0, islandPrefabs.Length)
            ];

            Instantiate(island, pos, Quaternion.identity)
        }
    }
}
