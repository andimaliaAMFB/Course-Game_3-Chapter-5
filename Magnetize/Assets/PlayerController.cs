using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb2D;
    public float moveSpeed = 5f;
    public float pullForce = 100f;
    public float rotateSpeed = 360f;
    private GameObject closestTower;
    private GameObject hookedTower;
    private bool isPulled = false;
    private UIControllerScript uiControl;
    private AudioSource myAudio;
    private bool isCrashed = false;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = this.gameObject.GetComponent<Rigidbody2D>();
        uiControl = GameObject.Find("Canvas").GetComponent<UIControllerScript>();
        myAudio = this.gameObject.GetComponent<AudioSource>();
        startPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Move the object
        rb2D.velocity = -transform.up * moveSpeed;

        if (Input.GetMouseButtonDown(0) && !isPulled)
        {
            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousepos2D = new Vector2(mousepos.x, mousepos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousepos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name); //cuma untuk mengetahui nama tower/ game objek mana yang di klik//
                hookedTower = hit.collider.gameObject;
            }
        }

        if (hookedTower)
        {
            float distance = Vector2.Distance(transform.position, hookedTower.transform.position);

            //Gravitation toward tower
            Vector3 pullDirection = (hookedTower.transform.position - transform.position).normalized;
            float newPullForce = Mathf.Clamp(pullForce / distance, 20, 50);
            rb2D.AddForce(pullDirection * newPullForce);

            //Angular velocity
            rb2D.angularVelocity = -rotateSpeed / distance;
            isPulled = true;
        }
    
        if (Input.GetMouseButtonUp(0))
        {
            isPulled = false;
            hookedTower = null;
            rb2D.angularVelocity = 0;
        }

        if (isCrashed)
        {
            if (!myAudio.isPlaying)
            {
                //Restart scene
                restartPosition();
            }
        }
        else
        {
            //Move the object
            rb2D.velocity = -transform.up * moveSpeed;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            if (!isCrashed)
            {
                //Play SFX
                myAudio.Play();
                rb2D.velocity = new Vector3(0f, 0f, 0f);
                rb2D.angularVelocity = 0f;
                isCrashed = true;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            Debug.Log("Levelclear!");
            uiControl.endGame();
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Tower")
        {
            closestTower = collision.gameObject;

            //Change tower color back to green as indicator
            collision.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (isPulled) return;

        if (collision.gameObject.tag == "Tower")
        {
            closestTower = null;
            hookedTower = null;
            //Change tower color back to normal
            collision.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void restartPosition()
    {
        //Set to start position
        this.transform.position = startPosition;

        //Restart rotation
        this.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        //Set isCrashed to false
        isCrashed = false;

        if (closestTower)
        {
            closestTower.GetComponent<SpriteRenderer>().color = Color.white;
            closestTower = null;
        }

    }
}
