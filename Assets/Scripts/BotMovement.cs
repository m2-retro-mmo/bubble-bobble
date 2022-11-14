using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement : MonoBehaviour
{
    public bool isWalking;

    public float walkTime;

    public float waitTime;

    [SerializeField] 
    private float speed = 3f;

    private Rigidbody2D rb;

    private float walkCounter;

    private float waitCounter;

    private int walkDirection;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;
            
            switch (walkDirection)
            {
                case 0:
                    rb.velocity = new Vector2(0, speed);
                    break;
                case 1:
                    rb.velocity = new Vector2(speed, 0);
                    break;
                case 2:
                    rb.velocity = new Vector2(0, -speed);
                    break;
                case 3:
                    rb.velocity = new Vector2(-speed, 0);
                    break;
            }

            if (walkCounter < 0f)
            {
                isWalking = false;
                waitCounter = waitTime;
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;
            rb.velocity = Vector2.zero;

            if (waitCounter < 0f)
            {
                ChooseDirection();
            }
        }
    }

    public void ChooseDirection()
    {
        walkDirection = Random.Range(0, 4);
        isWalking = true;
        walkCounter = walkTime;
    }
}
