using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DuckPhysics : MonoBehaviour
{
    [SerializeField] bool DebugLog = true;
   // [SerializeField] float PositionalCorrectionFactor = 0.2f;   // usually 20% to 80%
    //[SerializeField] float MaxColliderPenetration = 0.01f;      // usually 0.01 to 0.1

    List<DuckRigidbody> RegisteredBodies = new List<DuckRigidbody>();
    List<DuckCollision> UnresolvedCollisions = new List<DuckCollision>();

    static DuckPhysics _instance;
    public static DuckPhysics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<DuckPhysics>();
            }
            return _instance;
        }    
    }

    public bool AABBIntersectCheck(AABB a, AABB b)
    {
        if (a.Max.x < b.Min.x || a.Min.x > b.Max.x)
        {
            return false;
        }
        if (a.Max.y > b.Min.y || a.Min.y < b.Max.y)
        {
            return false;
        }
        return true;
    }    

    public bool CircleIntersectCheck(DuckCircleCollider a, DuckCircleCollider b)
    {
        if (Vector2.Distance(a.transform.position, b.transform.position) < a.Radius + b.Radius)     //Perhaps bad for performance when lots of objects. Works for now though.
        {
            return true;
        }
        return false;
    }

    public void ResolveCollision(DuckCollision col)
    {
        //Calculate masses
        float Mass_A = col.a.Mass;
        float Mass_B = col.b.Mass;     
        float invMass_A = 1f / Mass_A;
        float invMass_B = 1f / Mass_B;      

        //Calculate relative velocity
        Vector2 rv = col.b.Velocity - col.a.Velocity;

        //Calculate relative velocity in terms of the collision normal direction
        float velAlongNormal = Vector2.Dot(rv, col.Normal);

        //Do not resolve if velocities are seperating
        if (velAlongNormal > 0)
        {
            return;
        }

        //Calculate restitution
        float e = Mathf.Min(col.a.Bounciness, col.b.Bounciness);

        //Calculate impulse scalar
        float j = -(1f + e) * velAlongNormal;
        j /= invMass_A + invMass_B;

        //Apply impulse to non-kinematic bodies      
        float mass_sum = Mass_A + Mass_B;         //Calculate ratio of velocity applied to each body
        float ratio_A = Mass_A / mass_sum;
        float ratio_B = Mass_B / mass_sum;

        Vector2 impulse = j * col.Normal;        
        col.a.Velocity -= ratio_B * impulse;        
        col.b.Velocity += ratio_A * impulse;
    }  
    
    public void RegisterRigidbody(DuckRigidbody rb)
    {
        RegisteredBodies.Add(rb);          
    }
    
    public void DeregisterRigidbody(DuckRigidbody rb)
    {
        RegisteredBodies.Remove(rb);
    }

    void FixedUpdate()
    {
        Tick();
    }

    /// <summary>
    /// Perform one DuckPhysics tick. 
    /// </summary>
    public void Tick()
    {
        /*
        //Apply positional corrections to resolved collisions from last tick to prevent "sinking"
        foreach (DuckCollision col in UnresolvedCollisions.Where(r => r.a.Collider.GetType() == typeof(DuckCircleCollider)).Where(r => r.b.Collider.GetType() == typeof(DuckCircleCollider)))
        {
            //Calculate penetration depth between the two bodies
            float radiusSum = (col.a.Collider as DuckCircleCollider).Radius + (col.b.Collider as DuckCircleCollider).Radius;
            float penetrationDepth = Mathf.Abs(Vector2.Distance(col.a.transform.position, col.b.transform.position) - radiusSum);

            Vector2 correction = Mathf.Max(penetrationDepth - MaxColliderPenetration, 0) / ((1f / col.a.Mass) + (1f / col.b.Mass)) * PositionalCorrectionFactor * col.Normal;

            if (penetrationDepth > MaxColliderPenetration)
            {
                Debug.Log("DuckPhysics: Max penetration exceeded : " + penetrationDepth);
            }

            col.a.transform.position -= (1f / col.a.Mass) * (Vector3)correction;
            col.b.transform.position -= (1f / col.b.Mass) * (Vector3)correction;
        }*/     //DOesn't seem to work yet

        //Determine list of unresolved collisions
        UnresolvedCollisions.Clear();
        foreach (DuckRigidbody rb in RegisteredBodies)  
        {
            foreach (DuckRigidbody otherRB in RegisteredBodies.Where(other => other != rb))
            {
                //Check if this DuckRigidbody intersects colliders of any other registered bodies
                if (AABBIntersectCheck(rb.Collider.WorldAABB, otherRB.Collider.WorldAABB))
                {
                    //Axis-aligned bounding boxes intersected. Investigate further to see if a collision has occured:
                    if (DebugLog)
                    {
                        Debug.Log("DuckPhysics: AABB Intersect detected: " + rb.name + ", " + otherRB.name);
                    }

                    //If Circle-Circle collision type
                    if (rb.Collider.GetType() == typeof(DuckCircleCollider) && otherRB.Collider.GetType() == typeof(DuckCircleCollider))
                    {
                        DuckCircleCollider a = (DuckCircleCollider)rb.Collider;
                        DuckCircleCollider b = (DuckCircleCollider)otherRB.Collider;

                        //If collision has occured, create new DuckCollision object and add it to list of unresolved collisions:
                        if (CircleIntersectCheck(a, b))
                        {                            
                            //Calculate point of collision - use the radii of the circles to give true x and y coordinates of the collision point
                            Vector2 colPosition = new Vector2(
                                (a.transform.position.x * b.Radius + b.transform.position.x * a.Radius) / (a.Radius + b.Radius), 
                                (a.transform.position.y * b.Radius + b.transform.position.y * a.Radius) / (a.Radius + b.Radius));

                            //Calculate collision normal  - simple for Circle-Circle collision
                            Vector2 colNormal = (otherRB.transform.position - rb.transform.position).normalized;    
                            
                            //Create new DuckCollision
                            DuckCollision newCollision = new DuckCollision(rb, otherRB, colPosition, colNormal);                            
                            UnresolvedCollisions.Add(newCollision);

                            if (DebugLog)
                            {
                                Debug.Log("DuckPhysics: Collision occured: " + rb.name + ", " + otherRB.name);
                            }
                        }
                    }                                                                                                    
                }
            }           
        }

        //Resolve collisions by applying impulse to objects
        foreach (DuckCollision collision in UnresolvedCollisions)
        {
            ResolveCollision(collision);
        }

        //Update positions of DuckRigidbodies
        foreach (DuckRigidbody drb in RegisteredBodies)
        {
            drb.UpdatePosition();
        }
    }
}

public struct DuckCollision
{
    public DuckRigidbody a;
    public DuckRigidbody b;
    public Vector2 Point;
    public Vector2 Normal;

    public DuckCollision(DuckRigidbody a, DuckRigidbody b, Vector2 point, Vector2 normal)
    {
        this.a = a;
        this.b = b;
        this.Point = point;
        this.Normal = normal;
    }

}

public struct AABB
{
    public Vector2 Min;
    public Vector2 Max;  

    public AABB(Vector2 min, Vector2 max)
    {
        this.Min = min;
        this.Max = max;
    }
}

