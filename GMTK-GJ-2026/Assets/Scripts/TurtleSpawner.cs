using UnityEngine;

public class TurtleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _turtlePrefab;
    [SerializeField]
    Vector3 _turtleSpawn;
    private GameObject _spawnedTurtle;

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            var manager = GameObject.FindFirstObjectByType<GameManager>();
            manager.OnGameStart += SpawnTurtle;
            manager.OnGameStop += DestroyTurtle;
        } else
        {
            GameManager.Instance.OnGameStart += SpawnTurtle;
            GameManager.Instance.OnGameStop += DestroyTurtle;
        }
        ;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStart -= SpawnTurtle;
        GameManager.Instance.OnGameStop -= DestroyTurtle;
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
