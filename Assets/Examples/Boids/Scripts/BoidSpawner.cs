using UnityEngine;
using UnityEngine.Serialization;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private Boid _boidPrefab;
    [SerializeField] private int _spawnCount = 50;
    [SerializeField] private float _spawnRadius = 10f;

    private void Start()
    {
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * _spawnRadius;
            
            Quaternion randomRot = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            Boid newBoid = Instantiate(_boidPrefab, randomPos, randomRot);
            newBoid.name = "Boid_" + i;
            newBoid.transform.parent = transform;
        }
    }
}