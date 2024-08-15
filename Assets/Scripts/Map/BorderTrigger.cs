using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderTrigger : MonoBehaviour
{
    [SerializeField] private GameObject border;

    // Start is called before the first frame update
    void Start()
    {
        if (border.activeSelf == true)
        {
            border.SetActive(false);
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
            if (border.activeSelf == false)
            {
                border.SetActive(true);
            }
        }
    }
}
