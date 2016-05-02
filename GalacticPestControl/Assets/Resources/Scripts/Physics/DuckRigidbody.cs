using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class DuckRigidbody : MonoBehaviour
{
    [Header("Properties")]
    public float Mass = 1f;
    public float Bounciness = 1f;       //Must be value from 0 to 1
    //public float DragRate = 0.01f;      //Value between 0 to 1. Value of 1 means that this object will not be moveable. Value of zero means that it will move forever.
    public DuckCollider Collider;  
    public Vector2 Velocity;     
    
    [HideInInspector] public float InvMass;
    
    void Start()
    {
        //Pre-calcalate inverse mass (small optimization for DuckPhysics)
        InvMass = 1f / Mass;

        //Ensure this DuckRigidBody has a collider        
        if (Collider == null)
        {
            //Attempt to assign collider from this gameobject. If there is none, throw exception.
            DuckCollider selfCollider = GetComponent<DuckCollider>();
            if (selfCollider == null)
            {
                throw new Exception("Quack! The DuckRigidbody known as " + name + " has no collider attached to its script!");
            }
            else
            {
                Collider = selfCollider;
            }
        }

        //Clamp bounciness (restitution coeff) between 0 and 1
        Bounciness = Mathf.Clamp01(Bounciness);

        //Register this DuckRigidbody with DuckPhysics engine
        DuckPhysics.Instance.RegisterRigidbody(this);
    }   
    
    void FixedUpdate()
    {
         
    }    

    void OnDestroy()
    {
        //Deregister this DuckRigidbody with DuckPhysics engine
        if (DuckPhysics.Instance != null)
        {
            DuckPhysics.Instance.DeregisterRigidbody(this);
        }
    }

    public void UpdateMotion()
    {
        //Impede velocity by drag 
       // DragRate = Mathf.Clamp01(DragRate);
       // Velocity *= (1f - DragRate);

        //Move this object according to current velocity
        transform.position += (Vector3)Velocity * Time.deltaTime;
    }    
}

public enum DuckColliderType
{
    Circle,
    AABB
}

