using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

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


    //TEMPORARY confirm leaving area menu
    public GameObject moveMenuUI;
    private bool moveMenuOpen = false;
    Vector3 nextSpawnPoint;
    private bool zoomOut = false;
    private bool thisZoomOut = false;
    public CinemachineVirtualCamera cam1;
    public CinemachineVirtualCamera cam2;
    //private GameObject currentArea;
    //private GameObject nextArea;

    // Start is called before the first frame update
    private void Start()
    {
        //carrotCount = 0;
        //setCountText();
        moveMenuUI.SetActive(false);
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

        //Close confirm to leave menu and set time back to 1
        if (Input.GetKeyDown(KeyCode.Escape) && moveMenuOpen)
        {
            moveMenuUI.SetActive(false);
            moveMenuOpen = false;
            Time.timeScale = 1f;
        }
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
        GameObject collider = collision.gameObject;
        //Debug.Log("collided w something " + collision.gameObject.tag);
        if (collider.tag == "Door")
        {
            if (!collider.GetComponentInChildren<ChangeArea>().locked)
            {
                //Debug.Log("Encountered Door");
                moveMenuUI.SetActive(true);
                moveMenuOpen = true;
                Time.timeScale = 0f;


                
                GameObject spawnp = collider.GetComponentInChildren<ChangeArea>().spawnPoint;
                nextSpawnPoint = spawnp.transform.position;
                thisZoomOut = collider.GetComponentInChildren<ChangeArea>().zoomOut;
                //this.currentArea = collider.GetComponentInChildren<ChangeArea>().currentArea;
                //this.nextArea = collider.GetComponentInChildren<ChangeArea>().nextArea;
                Debug.Log("spawnp: " + nextSpawnPoint.ToString());

            }
        }
    }

    public void ChangeCam()
    {
        if (cam1.gameObject.activeSelf)
        {
            cam1.gameObject.SetActive(false);
            cam2.gameObject.SetActive(true);
        }
        else
        {
            cam2.gameObject.SetActive(false);
            cam1.gameObject.SetActive(true);
        }

    }

    public void LeaveArea()
    {
        if (zoomOut != thisZoomOut)
        {
            ChangeCam();
            zoomOut = thisZoomOut;
        }
        rb.transform.position = nextSpawnPoint;
        moveMenuOpen = false;
        moveMenuUI.SetActive(false);
        Time.timeScale = 1f;        
    }

    public void StayInArea()
    {
        moveMenuUI.SetActive(false);
        moveMenuOpen = false;
        Time.timeScale = 1f;
    }

    
}
