using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Player player;

    void Start()
    {
        SpawnEntities();
    }

    void SpawnEntities()
    {
        EntitySpawn[] entitySpawns = FindObjectsByType<EntitySpawn>(FindObjectsSortMode.None);
        foreach (EntitySpawn entitySpawn in entitySpawns)
        {
            entitySpawn.SpawnEntity();
        }
    }

    void Update()
    {
        
    }
}
