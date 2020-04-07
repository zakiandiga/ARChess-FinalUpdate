using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PieceBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Transform enemy;
    const int MAX_HEALTH = 3;
    int health = MAX_HEALTH;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // get reference to NavMeshAgent
        anim = GetComponent<Animator>(); // get reference to Animator
    }

    // function called if the piece is under attack
    public void underAttack()
    {
        health--; // reduce health
        anim.SetTrigger("attack");

        if (health == 1)
        {
            anim.SetTrigger("die");
            GameState.eliminatePiece(transform); // ask Game State to eliminate the piece
            Destroy(gameObject, 3);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (agent.velocity.magnitude > 0.1f) // if speed (that is magnitute of velocity vector) is not zero, then we are moving
                                             // we actually check whether is greater than some small number, because agents tend to get stuck
                                             // on doors and stuff, so they may be "moving" while they are already at destination
        {
            anim.SetTrigger("walk");         // trigger transition from idle to walk in animation engine
        }

        if (agent.velocity.magnitude <= 0.1f) // if agent's velocity is near zero, then piece is not moving, so start animating idle
        {
            anim.SetTrigger("idle");  // trigger animation of idle
        }

        if (enemy) // if the GameState set an enemy for us (the destination piece is occupied by enemy, then 
        {
            anim.SetTrigger("attack"); // trigger attack
            enemy.GetComponent<PieceBehaviour>().underAttack(); // substract health from the enemy's health.
        }
    }
}
