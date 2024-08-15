using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private GameObject boss;

    // Start is called before the first frame update
    void Start()
    {
        if (boss.activeSelf == true)
        {
            boss.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (boss.activeSelf == false)
            {
                boss.SetActive(true);
            }
        }
    }
}
