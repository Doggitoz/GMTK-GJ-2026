using UnityEngine;

public class TurtleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _turtlePrefab;
    [SerializeField]
    Vector3 _turtleSpawn;
    private GameObject _spawnedTurtle;

    private void Start()
    {
        GameManager.Instance.OnGameStart += SpawnTurtle;
        GameManager.Instance.OnGameStop += DestroyTurtle;
    }

    private void SpawnTurtle()
    {
        _spawnedTurtle = Instantiate(_turtlePrefab, _turtleSpawn, Quaternion.identity);
    }

    private void DestroyTurtle()
    {
        Destroy(_spawnedTurtle);
    }
}
