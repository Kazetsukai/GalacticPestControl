using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] float MoveSpeed = 3.5f;

    [Header("Weapon")]
    [SerializeField] Transform WeaponPivot;
    [SerializeField] Transform WeaponHolder;
    [SerializeField] float RotRate = 5f;
    [SerializeField] float RotInputDeadzone = 0.4f;

    Rigidbody2D rb;
    float aimAngle;

    SpriteAnimator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<SpriteAnimator>();
    }
    
    void Update()
    {
        //Move player
        Vector2 moveDir = new Vector2(Input.GetAxis("L_XAxis_1"), Input.GetAxis("L_YAxis_1"));
        rb.ForceMove(moveDir * MoveSpeed);

        //Rotate crosshair around player
        Vector3 aimInput = new Vector2(Input.GetAxis("R_XAxis_1"), Input.GetAxis("R_YAxis_1"));

        if (aimInput.magnitude >= RotInputDeadzone)
        {
            aimAngle = Vector3.up.Angle360To(aimInput.normalized) - 90f;      //Joe: I don't know why we need to subtract 90 degrees...
        }
        WeaponPivot.transform.rotation = Quaternion.Lerp(WeaponPivot.transform.rotation, Quaternion.Euler(new Vector3(WeaponPivot.transform.localEulerAngles.x, WeaponPivot.transform.localEulerAngles.y, aimAngle)), aimInput.magnitude * RotRate * Time.deltaTime);   //Multiply by aimInput.magnitude to get suuuper smooooth slow and fast rotation! Hurray! Could go even smoother if using an AnimationCurve multiplier too.  


        //Update animations
        if (moveDir.magnitude > 0f)
        {
            int direction = (int)(Vector3.up.Angle360To(moveDir) / 90f);

            Debug.Log(direction);
            switch (direction)
            {
                case -1: anim.PlayAnim("Left Walk"); break;
                case 0 : anim.PlayAnim("Up Walk"); break;
                case 1: anim.PlayAnim("Right Walk"); break;
                case 2: anim.PlayAnim("Down Walk"); break;               
            }
        }
        else
        {
            anim.PlayAnim("Down Idle");
        }
    }
}
