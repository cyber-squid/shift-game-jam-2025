using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public float spawnInterval = 5f;
    public int maxCustomers = 5;
    public Transform[] spawnPoints;

    [Header("Pair spawn")]
    [Tooltip("When true, spawns up to 'pairSize' customers per spawn and arranges them vertically around the spawn point.")]
    public bool spawnPairs = true;
    public int pairSize = 2;
    public float pairSpacing = 0.6f;

    [Header("Pooling (optional)")]
    public bool usePooling = true;
    public int poolSize = 10;

    List<GameObject> pool;

    void Awake()
    {
        if (usePooling && customerPrefab != null)
        {
            pool = new List<GameObject>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                var go = Instantiate(customerPrefab);
                go.SetActive(false);
                pool.Add(go);
            }
        }
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (CountActiveCustomers() < maxCustomers)
                SpawnCustomer();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    int CountActiveCustomers()
    {
        return FindObjectsOfType<Customer>().Length;
    }

    void SpawnCustomer()
    {
        if (customerPrefab == null) return;

        Transform spawn = (spawnPoints != null && spawnPoints.Length > 0) ? spawnPoints[Random.Range(0, spawnPoints.Length)] : null;
        Vector3 basePos = (spawn != null) ? spawn.position : new Vector3(-8f, 0f, 0f);

        int allowed = maxCustomers - CountActiveCustomers();
        if (allowed <= 0) return;

        int toSpawn = spawnPairs ? Mathf.Min(pairSize, allowed) : 1;

        List<Customer> spawned = new List<Customer>();
        for (int i = 0; i < toSpawn; i++)
        {
            float offset = 0f;
            if (toSpawn > 1)
            {
                // center the group vertically around basePos
                float start = -((toSpawn - 1) * pairSpacing) / 2f;
                offset = start + i * pairSpacing;
            }

            Vector3 spawnPos = basePos + new Vector3(0f, offset, 0f);
            GameObject go = null;

            if (usePooling && pool != null)
            {
                go = pool.Find(g => !g.activeInHierarchy);
                if (go == null)
                {
                    go = Instantiate(customerPrefab);
                    pool.Add(go);
                }
                go.transform.position = spawnPos;
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            }

            var cust = go.GetComponent<Customer>();
            if (cust != null)
            {
                // ensure selection indicator is hidden initially
                cust.Deselect();
                spawned.Add(cust);
            }
        }

        // Link spawned customers into pairs/groups so clicking one can target the group
        if (spawned.Count > 1)
        {
            for (int i = 0; i < spawned.Count - 1; i += 2)
            {
                var a = spawned[i];
                var b = spawned[i + 1];
                a.pairedCustomer = b;
                b.pairedCustomer = a;
            }
        }
    }

    public void ReturnToPool(GameObject go)
    {
        if (usePooling && pool != null && pool.Contains(go))
        {
            go.SetActive(false);
        }
        else
        {
            Destroy(go);
        }
    }
}
