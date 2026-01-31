using UnityEngine;

public class EntitySpawn : MonoBehaviour
{
    [SerializeField]
    private Vector3 boxSize = Vector3.one;
    [SerializeField]
    private Color spawnColor = Color.white;

    public GameObject entityPrefab;
    private GameManager gm;

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
    }

    public virtual void SpawnEntity()
    {
        if (entityPrefab)
        {
            Vector3 entityPos = transform.position;
            Quaternion entityRot = transform.rotation;
            Instantiate(entityPrefab, entityPos, entityRot);
            Debug.Log("Spawned entity '" + entityPrefab.name + "' at position " + entityPos);
        }
        else
        {
            Debug.LogAssertion(
                "Entity prefab not found! Couldn't spawn entity on instance " + transform.GetInstanceID()
                );
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = spawnColor;

        Vector3 boxPos = new Vector3(
            0,
            boxSize.y * 0.5f,
            0
        );
        Gizmos.DrawWireCube(boxPos, boxSize);
    }
}
