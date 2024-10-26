using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Depth : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    float timer = 1f;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(this.transform.position).y * -1;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            timer = 1f;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>() || other.gameObject.tag == "Hero")
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            timer = 1f;
        }
    }
}
