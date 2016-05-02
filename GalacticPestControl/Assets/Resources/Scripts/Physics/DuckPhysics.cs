using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DuckPhysics : MonoBehaviour
{
    [SerializeField] bool DebugLog = true;
    [SerializeField] float PositionalCorrectionFactor = 0.2f;   // usually 20% to 80%
    [SerializeField] float MaxColliderPenetration = 0.01f;      // usually 0.01 to 0.1

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

    public bool CircleIntersectsBox(DuckBoxCollider A, DuckCircleCollider B, out Vector2 Normal, out float Penetration)
    {
        // Vector from A to B
        Vector2 n = B.transform.position - A.transform.position;

        // Closest point on A to center of B
        Vector2 closest = n;

        // Calculate half extents along each axis of box
        float x_extent = (A.WorldAABB.Max.x - A.WorldAABB.Min.x) / 2f;
        float y_extent = (A.WorldAABB.Max.y - A.WorldAABB.Min.y) / 2f;

        // Clamp point to edges of the AABB
        closest.x = Mathf.Clamp(closest.x, -x_extent, x_extent);
        closest.y = Mathf.Clamp(closest.y, -y_extent, y_extent);

        bool inside = false;

        // Circle is inside the AABB, so we need to clamp the circle's center to the closest edge
        if (n == closest)
        {
            inside = true;

            //Find closest axis
            if (Mathf.Abs(n.x) > Mathf.Abs(n.y))
            {
                //Clamp to closest extent
                if (closest.x > 0)
                {
                    closest.x = x_extent;
                }
                else
                {
                    closest.x = -x_extent;
                }
            }
            //Y axis is shorter
            else
            {
                //Clamp to closest extent
                if (closest.y > 0)
                {
                    closest.y = y_extent;
                }
                else
                {
                    closest.y = -y_extent;
                }
            }
        }

        Vector2 normal = n - closest;
        float d = normal.sqrMagnitude;
        float r = B.Radius;

        // Early out of the radius is shorter than distance to closest point and
        // Circle not inside the AABB
        if (d > (r * r) && !inside)
        {
            Normal = Vector2.zero;
            Penetration = 0;
            return false;
        }

        //Avoid sqrt until needed
        d = Mathf.Sqrt(d);

        // Collision normal needs to be flipped to point outside if circle was
        // inside the AABB       
        if (inside)
        {
            Normal = -n;           
        }
        else
        {
            Normal = n;
        }
        Penetration = r - d;
        return true;
    }

    public void ResolveCollision(DuckCollision col)
    {
        //Calculate masses
        float Mass_A = col.a.Mass;
        float Mass_B = col.b.Mass;
        float invMass_A = col.a.InvMass;
        float invMass_B = col.b.InvMass;      

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

        /*
        //Correct position (i.e, reverse penetration)
        Vector2 correction = (Mathf.Max(col.Penetration - MaxColliderPenetration, 0) / (col.a.InvMass + col.b.InvMass)) * PositionalCorrectionFactor * col.Normal;
        col.a.transform.position -= col.a.InvMass * (Vector3)correction;
        col.b.transform.position -= col.b.InvMass * (Vector3)correction;*/
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
                            /*
                            //Calculate point of collision - use the radii of the circles to give true x and y coordinates of the collision point
                            Vector2 colPosition = new Vector2(
                                (a.transform.position.x * b.Radius + b.transform.position.x * a.Radius) / (a.Radius + b.Radius), 
                                (a.transform.position.y * b.Radius + b.transform.position.y * a.Radius) / (a.Radius + b.Radius));*/

                            //Calculate collision normal  - simple for Circle-Circle collision
                            Vector2 colNormal = (otherRB.transform.position - rb.transform.position).normalized;

                            //Calculate penetration depth between the two bodies
                            float penetrationDepth = Mathf.Abs(Vector2.Distance(a.transform.position, b.transform.position) - (a.Radius + b.Radius));

                            //Create new DuckCollision
                            DuckCollision newCollision = new DuckCollision(rb, otherRB, colNormal, penetrationDepth);
                            UnresolvedCollisions.Add(newCollision);

                            if (DebugLog)
                            {
                                Debug.Log("DuckPhysics: Collision occured: " + rb.name + ", " + otherRB.name);
                            }
                        }
                    }
                    //If Box-Box collision type   .
                    if (rb.Collider.GetType() == typeof(DuckBoxCollider) && otherRB.Collider.GetType() == typeof(DuckBoxCollider))
                    {
                        DuckCollider a = rb.Collider;
                        DuckCollider b = otherRB.Collider;

                        //Calculate collision normal 
                        Vector2 colNormal = (otherRB.transform.position - rb.transform.position).normalized;
                        float penetration = 0f;

                        // Calculate half extents along x axis for each object
                        float a_extent_x = (a.WorldAABB.Max.x - a.WorldAABB.Min.x) / 2f;
                        float b_extent_x = (b.WorldAABB.Max.x - b.WorldAABB.Min.x) / 2f;

                        // Calculate overlap on x axis
                        float x_overlap = a_extent_x + b_extent_x - Mathf.Abs(colNormal.x);

                        // SAT test on x axis
                        if (x_overlap > 0)
                        {
                            // Calculate half extents along y axis for each object
                            float a_extent_y = (a.WorldAABB.Max.y - a.WorldAABB.Min.y) / 2f;
                            float b_extent_y = (b.WorldAABB.Max.y - b.WorldAABB.Min.y) / 2f;

                            // Calculate overlap on y axis
                            float y_overlap = a_extent_y + b_extent_y - Mathf.Abs(colNormal.y);

                            // SAT test on y axis
                            if (y_overlap > 0)
                            {
                                // Find out which axis is axis of least penetration
                                if (x_overlap > y_overlap)
                                {
                                    // Point towards B knowing that normal points from A to B
                                    if (colNormal.x < 0)
                                    {
                                        colNormal = new Vector2(-1, 0);
                                    }
                                    else
                                    {
                                        colNormal = new Vector2(1, 0);
                                    }
                                    penetration = x_overlap;
                                }
                                else
                                {
                                    // Point towards B knowing that normal points from A to B
                                    if (colNormal.y < 0)
                                    {
                                        colNormal = new Vector2(0, -1);
                                    }
                                    else
                                    {
                                        colNormal = new Vector2(0, 1);
                                    }
                                    penetration = y_overlap;
                                }
                            }
                        }

                        //Create new DuckCollision
                        DuckCollision newCollision = new DuckCollision(rb, otherRB, colNormal, penetration);
                        UnresolvedCollisions.Add(newCollision);

                        if (DebugLog)
                        {
                            Debug.Log("DuckPhysics: Collision occured: " + rb.name + ", " + otherRB.name);
                        }
                    }
                    //If Circle-Box collision type
                    if (rb.Collider.GetType() == typeof(DuckCircleCollider) && otherRB.Collider.GetType() == typeof(DuckBoxCollider))
                    {
                        Vector2 normal;
                        float penetration;
                        if (CircleIntersectsBox((DuckBoxCollider) otherRB.Collider, (DuckCircleCollider)rb.Collider, out normal, out penetration))
                        {
                            UnresolvedCollisions.Add(new DuckCollision(otherRB, rb, normal, penetration));
                        }
                    }
                    //or Box-Circle collision type
                    if (rb.Collider.GetType() == typeof(DuckBoxCollider) && otherRB.Collider.GetType() == typeof(DuckCircleCollider))
                    {
                        Vector2 normal;
                        float penetration;
                        if (CircleIntersectsBox((DuckBoxCollider)rb.Collider, (DuckCircleCollider)otherRB.Collider, out normal, out penetration))
                        {
                            UnresolvedCollisions.Add(new DuckCollision(rb, otherRB, normal, penetration));
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
            drb.UpdateMotion();
        }
    }    
}

public struct DuckCollision
{
    public DuckRigidbody a;
    public DuckRigidbody b;
    //public Vector2 Point;
    public Vector2 Normal;
    public float Penetration;

    public DuckCollision(DuckRigidbody a, DuckRigidbody b, /*Vector2 point,*/ Vector2 normal, float penetration)
    {
        this.a = a;
        this.b = b;
        //this.Point = point;
        this.Normal = normal;
        this.Penetration = penetration;
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

