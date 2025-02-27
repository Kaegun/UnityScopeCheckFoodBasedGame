using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnController : MonoBehaviour
{
    [SerializeField] int spawnGap = 3;

    public int SpawnGap
    {
        set { spawnGap = value; }
        get { return spawnGap; }
    }

    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] WaveManager waveMgr;
    [SerializeField] List<Transform> enemiesInScene;
    public List<Transform> EnemiesInScene
    {
        get { return enemiesInScene; }
    }

    bool waveComplete = false;
    GameObject spawnedEnemies;
    int spawnLocationIndex = 0;



    /*
     * WaveManager now connected to GameManager. The game manager tells 
     * the wave manager when its time to spawn a wave which then
     * tells the SpawnController how many and where
    */
    // Start is called before the first frame update
    private void Awake()
    {
        spawnedEnemies = new GameObject();
        spawnedEnemies.name = "Spawned Enemies";
        enemiesInScene = new List<Transform>();
    }

    /*    private void Update()
        {
            if(enemiesInScene.Count <=0 && waveMgr.wave.waveComplete)
            {
                WaveManager.changeWaveState(WaveManager.WaveState.preWave);
            }
        }*/

    IEnumerator SpawnMonstersWithGap(int gap)
    {
        Debug.Log("SpawnMonstersWithGap Coroutine Started at: " + Time.time);

        while (GameManager.Instance.gameState == GameManager.GameState.gameRunning)
        {

            yield return new WaitForSeconds(gap);

            if (enemiesInScene.Count < waveMgr.wave.maxEnemiesSpawnedDuringWave && !waveMgr.wave.waveComplete)
            {
                var spawnedEnemy = Instantiate<GameObject>(SelectEnemyType(), spawnedEnemies.transform);
                enemiesInScene.Add(spawnedEnemy.transform);
                waveMgr.wave.AddEnemyToWaveCount();
                //This is a solution for the spawned enemy not hitting the correct point due to a navmesh conflict
                // we need to clean this up and make it an event based system.
                spawnedEnemy.GetComponent<NavMeshAgent>().enabled = false;
                spawnedEnemy.transform.position = spawnPoints[spawnLocationIndex].position;
                //same here
                spawnedEnemy.GetComponent<NavMeshAgent>().enabled = true;

                spawnLocationIndex = spawnLocationIndex >= spawnPoints.Count - 1 ? 0 : spawnLocationIndex + 1;
                //spawnedEnemy.transform.parent = spawnedEnemies.transform;
                yield return new WaitForSeconds(gap);
            }
            else
                yield return new WaitUntil(() => enemiesInScene.Count < waveMgr.wave.maxEnemiesSpawnedDuringWave);


        }

        yield break;

    }

    public GameObject SelectEnemyType()
    {
        GameObject selectedEnemyType = null;
        int randomNumber = Random.Range(0, 100);

        switch(randomNumber)
        {
            
            //case int n when n == 0:
            //Debug.Log("I'd spawn a ghost here: {n}");
            //break;
            
            
            case int n when n >= 1 && n <= 5:
                selectedEnemyType = enemyPrefabs[2]; //skele
            break;
            
              
            case int n when n > 5 && n <= 14:
                selectedEnemyType = enemyPrefabs[1]; //vamp
                break;
            
            //If the number is 0 to 80 
            case int n when n > 14:
                selectedEnemyType = enemyPrefabs[0]; //zombo
                break;

            default:
                selectedEnemyType = enemyPrefabs[0]; //zombo
                break;

        }

        return selectedEnemyType;
    }

    public void StartMonsterWithGapCoRoutine(int gap)
    {
        StartCoroutine(SpawnMonstersWithGap(gap));
    }

    public void StopSpawning()
    {
        StopCoroutine(SpawnMonstersWithGap(spawnGap));
    }

    public void RemoveDestroyedEnemy( Transform enemyTrans)
    {
        enemiesInScene.Remove(enemyTrans);
        if(enemiesInScene.Count == 0)
        {
            WaveManager.changeWaveState(WaveManager.WaveState.preWave);
        }
    }
}
