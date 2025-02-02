using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Health = 100f;
    //public float moveSpeed = 6f;
    //public float rotationSpeed = 110f;

    void Update()
    {
        //HandleMovement();
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0f)
        {
            Health = 0f;
            Debug.Log("Player is Dead !");
        }
        //UpdateHealthBar();
    }

    /*private void HandleMovement()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

        float movement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Rotate(0, rotation, 0);

        transform.Translate(0, 0, movement);
    }*/
}
