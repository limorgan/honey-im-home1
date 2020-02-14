using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //public CharacterController2D controller;
    public Animator animator;
    public Text countText;

    //float horizontalMove = 0f;
    //float verticalMove = 0f;
    Vector2 movement;
    public float runSpeed = 40f;
    public Rigidbody2D rb;
    //bool jump = false;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing

    //private int carrotCount;

    // Start is called before the first frame update
    private void Start()
    {
        //carrotCount = 0;
        //setCountText();
    }


    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        bool moving = (movement.x != 0 || movement.y != 0);
        animator.SetBool("Moving", moving);
        //animator.SetBool("Jump", jump);

        /*if (Input.GetButtonDown("Jump"))
        {
            //jump = true;
        }*/
    }

    private void FixedUpdate()
    {
        //move char - applies input from update
        //controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump, false);
        //jump = false;
        rb.MovePosition(rb.position + movement * runSpeed * Time.fixedDeltaTime);
        // If the input is moving the player right and the player is facing left...
        if (movement.x > 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (movement.x < 0 && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
    }

    /*private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            //other.gameObject.SetActive(false);
            //carrotCount++;
            //setCountText();
        }

        if (other.gameObject.CompareTag("Deathzone"))
        {

        }
    }

    void setCountText()
    {
        countText.text = "Carrots: " + carrotCount.ToString();
    }*/

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("collided w something " + collision.gameObject.tag);
        if (collision.gameObject.tag == "Door")
        {
            Debug.Log("Encountered Door");
            GameObject spawnp = collision.gameObject.GetComponentInChildren<ChangeArea>().spawnPoint;
            Debug.Log("spawnp: " + spawnp.transform.position);
            rb.transform.position = spawnp.transform.position;
        }
    }
}
