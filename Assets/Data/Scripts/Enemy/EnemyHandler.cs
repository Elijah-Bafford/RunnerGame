using System.Collections.Generic;
using UnityEngine;
using static Enemy;

[DefaultExecutionOrder(-1)]
public class EnemyHandler : MonoBehaviour {
    [SerializeField] private Transform _enemyContainer;

    [System.Serializable]
    public struct EnemyRegister {
        public EnemyType type;
        public GameObject prefab;
    }

    [SerializeField] private EnemyRegister[] _enemyRegister;

    private Dictionary<EnemyType, EnemyRegister> _enemyDictionary = new();
    private List<GameObject> _enemyList = new();

    private void Awake() {
        GameStateHandler.OnLevelRestart += OnLevelRestart;
        SceneHandler.OnLevelLoad += OnLevelLoad;
        BuildDictionary();
    }

    protected virtual void OnDestroy() {
        GameStateHandler.OnLevelRestart -= OnLevelRestart;
        SceneHandler.OnLevelLoad -= OnLevelLoad;
    }


    private void OnLevelLoad(int index) {
        BuildEnemies();
    }

    private void OnLevelRestart() {
        for (int i = 0; i < _enemyList.Count; i++) {
            Destroy(_enemyList[i]);
        }
        _enemyList.Clear();
        BuildEnemies();
    }

    private void BuildDictionary() {
        _enemyDictionary.Clear();
        _enemyDictionary = new Dictionary<EnemyType, EnemyRegister>(_enemyRegister.Length);
        foreach (var register in _enemyRegister)
            _enemyDictionary[register.type] = register;
    }

    private void BuildEnemies() {
        foreach (Transform child in transform) {
            EnemyMarker data = child.GetComponent<EnemyMarker>();
            if (data != null) {
                EnemyType type = data.enemyType;
                _enemyList.Add(CreateEnemy(type, child.transform));
            }
        }
    }

    private GameObject CreateEnemy(EnemyType enemyType, Transform pos) {
        GameObject prefab = null;

        prefab = _enemyDictionary[enemyType].prefab;

        GameObject enemy = Instantiate(prefab, pos.position, pos.rotation);
        enemy.transform.SetParent(_enemyContainer, true);
        enemy.gameObject.SetActive(true);
        return enemy;
    }
}
