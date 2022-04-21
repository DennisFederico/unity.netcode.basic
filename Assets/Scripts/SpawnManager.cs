using System.Collections;
using System.Collections.Generic;
using Dennis.Netcode.Extensions;
using Dennis.Unity.Utils.Singletons;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkSingleton<SpawnManager>
{
    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private int maxBallSpawnCount = 5;

    [SerializeField]
    private Vector2 SpawnXArea;
    [SerializeField]
    private Vector2 SpawnZArea;

    private void Awake() {
        //Initialize pool
    }

    public void SpawnBalls() {
        if (!IsServer) return;

        for (int i = 0; i< maxBallSpawnCount; i++) {
            // GameObject go = Instantiate(ballPrefab, new Vector3(Random.Range(SpawnXArea.x, SpawnXArea.y), 10f, Random.Range(SpawnZArea.x, SpawnZArea.y)), Quaternion.identity);
            // go.GetComponent<NetworkObject>().Spawn();
            //Using Object Pool
            NetworkObjectPool.Instance.GetNetworkObject(ballPrefab, new Vector3(Random.Range(SpawnXArea.x, SpawnXArea.y), 10f, Random.Range(SpawnZArea.x, SpawnZArea.y)), Quaternion.identity).Spawn();
        }
    }
}
