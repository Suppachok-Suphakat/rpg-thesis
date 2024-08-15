using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;

    public void DropItems()
    {
        foreach (GameObject itemPrefab in itemPrefabs)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }
}
