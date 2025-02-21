using UnityEngine;
using System.Collections.Generic;

public class EnemyArea : MonoBehaviour
{
    public GameObject blockingStone;
    private List<EnemyHealth> enemies = new List<EnemyHealth>();
    private bool areaActive = false;

    private void Start()
    {
        blockingStone.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !areaActive)
        {
            ActivateArea();
        }
    }

    void ActivateArea()
    {
        areaActive = true;
        blockingStone.SetActive(true);

        // Find only enemies inside the EnemyArea
        enemies.Clear();
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().bounds.size, 0);

        foreach (var col in colliders)
        {
            EnemyHealth enemy = col.GetComponent<EnemyHealth>();
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                enemy.OnEnemyDeath += () => RemoveEnemy(enemy); // Attach event
            }
        }

        DebugEnemiesList("After Activation");
        CheckEnemies(); // Ensure stone is checked in case no enemies are there
    }

    void RemoveEnemy(EnemyHealth enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }

        DebugEnemiesList("After Removing Enemy");
        CheckEnemies();
    }

    void CheckEnemies()
    {
        if (enemies.Count == 0)
        {
            blockingStone.SetActive(false);
        }
    }

    private void DebugEnemiesList(string context)
    {
        Debug.Log($"[{context}] Enemy count: {enemies.Count}");
        foreach (var enemy in enemies)
        {
            Debug.Log($"[{context}] Enemy in list: {enemy?.name}");
        }
    }
}
