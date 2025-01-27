using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 15f;
    public float speedAfterJump = 7f;
    public float movementSpeed;
    public float gravity = -9.81f;
    [SerializeField] Transform groundCheck;
    public float groundDistance = 1f;
    public LayerMask groundMask;
    public float jumpHeight = 5f;
    float timeToWait = 1f;
 


    Vector3 velocity;



    void Start()
    {

        movementSpeed = speed;
       


    }
    void Update()
    {
       
        
        velocity.y = -2f;
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        
       

        
        controller.Move(move * movementSpeed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }
    IEnumerator SlowMouvement()
    {
        movementSpeed = speedAfterJump;

        yield return new WaitForSeconds(timeToWait);
        movementSpeed = speed;

    }


    // dez other objects fehom rb

    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    Rigidbody rb = hit.collider.attachedRigidbody;

    //}

}
