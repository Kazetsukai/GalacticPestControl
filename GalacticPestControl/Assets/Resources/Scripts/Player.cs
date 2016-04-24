using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] float MoveSpeed = 10f;

    [Header("Weapon")]
    [SerializeField] Transform WeaponHolder;
    [SerializeField] float Radius = 0.5f;
 

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        //Move player
        Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.ForceMove(moveDir * MoveSpeed);

        //Rotate crosshair around player
        Vector3 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        WeaponHolder.transform.position += mouseDelta;        

        /*
        Vector3 mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;        
        WeaponHolder.transform.position = transform.position + (mouseDir * Radius);              
        */
    }
}
