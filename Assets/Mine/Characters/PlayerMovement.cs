using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    
    Vector2 movement;
    public float runSpeed = 40f;
    public Rigidbody2D rb;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing

    private float xPosition;
    private float yPosition;

    //TEMPORARY confirm leaving area menu
    public GameObject moveMenuUI;
    private bool moveMenuOpen = false;
    Vector3 nextSpawnPoint;
    private bool zoomOut = false;
    private bool thisZoomOut = false;
    private string nextAreaName;
    public CinemachineVirtualCamera cam1;
    public CinemachineVirtualCamera cam2;
    private string areaMessage;

    // Start is called before the first frame update
    private void Start()
    {
        moveMenuUI.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        xPosition = rb.transform.position.x;
        yPosition = rb.transform.position.y;

        bool moving = (movement.x != 0 || movement.y != 0);
        animator.SetBool("Moving", moving);
        
        if (Input.GetKeyDown(KeyCode.Escape) && moveMenuOpen)
        {
            moveMenuUI.SetActive(false);
            moveMenuOpen = false;
            Time.timeScale = 1f;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * runSpeed * Time.fixedDeltaTime);
        if (movement.x > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (movement.x < 0 && m_FacingRight)
        {
            Flip();
        }
    }

    
    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collider = collision.gameObject;
        if (collider.tag == "Door")
        {
            if (!collider.GetComponentInChildren<ChangeArea>().locked)
            {
                moveMenuUI.SetActive(true);
                Player.Instance.closeAllMenus();
                moveMenuOpen = true;
                Time.timeScale = 0f;
                
                GameObject spawnp = collider.GetComponentInChildren<ChangeArea>().spawnPoint;
                nextSpawnPoint = spawnp.transform.position;
                thisZoomOut = collider.GetComponentInChildren<ChangeArea>().zoomOut;
                this.nextAreaName = collider.GetComponentInChildren<ChangeArea>().nextAreaName; 
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
        Player.Instance.areaText.text = nextAreaName;
        Player.Instance.closeAllMenus();
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

    public void PrintMessage()
    {
        if (areaMessage != "")
        {
            Player.Instance.ShowSpeechBubble(areaMessage);
        }
    }
}
