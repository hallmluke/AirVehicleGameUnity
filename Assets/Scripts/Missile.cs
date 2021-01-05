using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Missile : SpawnableItem
{
    public Transform HomingTarget;
    public float Speed;
    public float MaxSpeed;
    public float Constant;
    Rigidbody rigidBody;

    public float pFactor, iFactor, dFactor;
		
	float integral;
	float lastError;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        HomingTarget = AcquireHomingTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 DirectionToTarget = HomingTarget.position - transform.position;
        Vector3 CurrentDirection = transform.forward;
        Vector3 CrossProd = Vector3.Cross(CurrentDirection, DirectionToTarget.normalized);

        Debug.DrawRay(transform.position, transform.forward, Color.green);
        Debug.DrawRay(transform.position, CrossProd, Color.red);

        float AngleError = Vector3.Angle(DirectionToTarget, CurrentDirection);
        float AngleErrorChange = 0;
        if(Time.fixedDeltaTime != 0) {
            AngleErrorChange = (AngleError - lastError) / Time.fixedDeltaTime;
        }
        //integral += AngleError * Time.fixedDeltaTime;
        lastError = AngleError;
        rigidBody.AddTorque(CrossProd * (AngleError * pFactor + integral * iFactor + AngleErrorChange * dFactor));

        if(AngleError < 20) {
            Vector3 DesiredVelocity = (HomingTarget.position - transform.position).normalized * MaxSpeed;
            Vector3 Error = DesiredVelocity - rigidBody.velocity;
            Vector3 Force = Error * Constant;
            rigidBody.AddForce(Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Explode
        float radius = 5.0f;
        float power = 15.0f;
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 5.0F, ForceMode.Impulse);
        }
        Destroy(gameObject);
    }

    Transform AcquireHomingTarget() {
        // TODO: Implement real target acquisition
        GameObject dummy = GameObject.Find("Dummy");
        return dummy.transform;
    }
}
