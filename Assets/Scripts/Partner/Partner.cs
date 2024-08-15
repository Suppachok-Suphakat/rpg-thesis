using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partner : MonoBehaviour
{
    [System.Serializable]
    public class Status
    {
        public float attackDistance = 5;
        public int followSpeed = 2;
        public int chaseSpeed = 2;
    }

    public Transform player;
    public Status status;
    public State currentState = State.follow;

    public enum State
    {
        follow = 0,
        chase = 1,
        attack = 2,
        skill = 3,
        death = 4
    }

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void AttackLogic()
    {
        Debug.Log("Friend Attack");
    }
}
