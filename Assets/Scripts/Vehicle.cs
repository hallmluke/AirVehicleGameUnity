using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour
{
    public Rigidbody rb;
    public Transform CameraRig;
    public float CameraSpeed;

    [Header("Suspension Points")]
    public Transform BackLeft;
    public Transform BackRight;
    public Transform FrontLeft;
    public Transform FrontRight;

    float PreviousBackLeftDistance;
    RaycastHit BackLeftHit;
    bool IsBackLeftOnGround;
    
    float PreviousBackRightDistance;
    RaycastHit BackRightHit;
    bool IsBackRightOnGround;
    
    float PreviousFrontLeftDistance;
    RaycastHit FrontLeftHit;
    bool IsFrontLeftOnGround;
    
    float PreviousFrontRightDistance;
    RaycastHit FrontRightHit;
    bool IsFrontRightOnGround;

    
    [Header("Suspension")]
    [Tooltip("Distance the suspension raycasts downward. Effectively controls how high the vehicle hovers off the ground.")]
    public float RaycastDistance;
    [Tooltip("Controls how much force each suspension point applies. Reasonable base: 4.")]
    public float SuspensionStrength;
    [Tooltip("Controls how 'springy' the suspension is. Higher value is more rigid. Reasonable base: 1.2")]
    public float SuspensionDamping;


    [Header("Ground Handling")]  
    public float GroundAcceleration;
    public float GroundBrakeRate;
    [Tooltip("Controls how much horizontal frictional force is applied. Higher value means less drift. Reasonable base: 5")]
    public float FrictionForce;
    public float TurnRate;
    public float BrakingTurnRate;
    public float TopGroundSpeed;
    [Tooltip("Controls how effectively top speed is limited. Only applied after vehicle has reached top speed. Higher value means hard cap, lower means boundless velocity. Reasonable base: 1 to 2")]
    public float GroundDragCoefficient;
    
    [Header("Aerial Handling")]
    [Tooltip("The thrust of the vehicle while midair. Typically higher than ground accel so it can stay afloat. Reasonable base: 8")]
    public float GlidingAcceleration;
    public float GlidingRollRate;
    public float GlidingPitchRate;
    [Tooltip("Affects how quickly/easily vehicle can take off. This is multiplied by horizontal velocity squared to get upward force. Reasonable base: .045")]
    public float LiftCoefficient;
    public float OptimalLiftAngle;
    public float TopAerialHorizontalSpeed;
    public float AerialHorizontalDragCoefficient;
    public float TopAerialVerticalSpeed;
    public float AerialVerticalDragCoefficient;
    public float TerminalVelocity;
    public float TerminalVelocityDragCoefficient;

    // State Booleans
    bool IsBraking;
    bool IsGliding;

    // Start is called before the first frame update
    void Start()
    {
        rb.centerOfMass = new Vector3(0, -1.5f, 0);
        InvokeRepeating("SendSpeedNotification", 0.0f, 0.2f);
    }

    // Using fixed update due to physics system
    void FixedUpdate()
    {
        //TODO Separate camera logic, add more robust camera system
        CameraRig.position = Vector3.Lerp(CameraRig.position, rb.position, CameraSpeed * Time.deltaTime);
        Vector3 TargetRotationEuler = rb.rotation.eulerAngles;

        if(!IsGliding) {
            TargetRotationEuler.x = 0;
            TargetRotationEuler.z = 0;
        }
        CameraRig.rotation = Quaternion.Slerp(CameraRig.rotation, Quaternion.Euler(TargetRotationEuler), 0.04f);

        ApplySuspension(BackLeft, ref PreviousBackLeftDistance, ref BackLeftHit, ref IsBackLeftOnGround);
        ApplySuspension(BackRight, ref PreviousBackRightDistance, ref BackRightHit, ref IsBackRightOnGround);
        ApplySuspension(FrontLeft, ref PreviousFrontLeftDistance, ref FrontLeftHit, ref IsFrontLeftOnGround);
        ApplySuspension(FrontRight, ref PreviousFrontRightDistance, ref FrontRightHit, ref IsFrontRightOnGround);

        IsGliding = !IsBackLeftOnGround && !IsBackRightOnGround && !IsFrontLeftOnGround && !IsFrontRightOnGround;

        if (!IsGliding)
        {
            Vector3 GroundForwardVec = transform.forward;

            if (IsFrontLeftOnGround && IsBackLeftOnGround)
            {
                GroundForwardVec = FrontLeftHit.point - BackLeftHit.point;
            }

            // TODO: More robust input system
            IsBraking = Input.GetKey(KeyCode.Space);
            float AppliedTurnRate = BrakingTurnRate;
            if (!IsBraking)
            {
                rb.AddForce(GroundForwardVec.normalized * GroundAcceleration);
                AppliedTurnRate = TurnRate;
            } else {
                rb.AddForce(-GroundForwardVec * GroundBrakeRate);
            }

            float HorizontalVal = Input.GetAxisRaw("Horizontal");

            rb.AddTorque(transform.up * HorizontalVal * AppliedTurnRate * Time.deltaTime);

           
            Vector3 ProjectedVelocity = Vector3.ProjectOnPlane(rb.velocity, transform.up);
            float theta = Vector3.Angle(transform.right, ProjectedVelocity);
            Vector3 HorizontalVelocity = Mathf.Cos(theta * Mathf.Deg2Rad) * transform.right;

            rb.AddForce(-HorizontalVelocity * FrictionForce);
            Debug.DrawRay(transform.position, HorizontalVelocity, Color.red);

            if(rb.velocity.magnitude > TopGroundSpeed) {
                rb.AddForce(-rb.velocity * GroundDragCoefficient);
            }
            
        } else {

            rb.AddForce(transform.forward * GlidingAcceleration);
            Vector3 HorizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
            /*Vector3 InclineVector = Vector3.ProjectOnPlane(transform.forward, transform.right);*/
            //Debug.DrawRay(transform.position, InclineVector, Color.green);
            Vector3 HorizontalVec = transform.forward;
            HorizontalVec.y = 0;
            float InclineAngle = Vector3.SignedAngle(transform.forward, HorizontalVec, transform.right);
            Debug.Log("Incline Angle: " + InclineAngle);
            float InclineModifier = 1 - Mathf.Min(1, Mathf.Abs((OptimalLiftAngle - InclineAngle) / 90.0f));
            Debug.Log("Incline Modifier: " + InclineModifier);
            float LiftMagnitude = HorizontalVelocity.magnitude * HorizontalVelocity.magnitude * LiftCoefficient * InclineModifier;

            rb.AddForce(Vector3.up * LiftMagnitude);
            float Pitch = Input.GetAxisRaw("Vertical");
            rb.AddTorque(transform.right * Pitch * GlidingPitchRate * Time.deltaTime);
            float Roll = Input.GetAxisRaw("Horizontal");
            rb.AddTorque(-transform.forward * Roll * GlidingRollRate * Time.deltaTime);

            if(HorizontalVelocity.magnitude > TopAerialHorizontalSpeed) {
                rb.AddForce(-HorizontalVelocity * AerialHorizontalDragCoefficient);
            }

            float VerticalSpeed = rb.velocity.y;
            if(VerticalSpeed > TopAerialVerticalSpeed) {
                rb.AddForce(Vector3.down * AerialVerticalDragCoefficient);
            } else if (VerticalSpeed < -TerminalVelocity) {
                Debug.Log("Reached terminal velocity");
                rb.AddForce(Vector3.up * TerminalVelocityDragCoefficient);
            }

        }

    }

    void SendSpeedNotification() {
        this.PostNotification("VehicleSpeed", rb.velocity.magnitude);
    }

    void ApplySuspension(Transform CornerTransform, ref float PreviousDistance, ref RaycastHit hit, ref bool isHitting)
    {
        isHitting = Physics.Raycast(CornerTransform.position, -Vector3.up, out hit, RaycastDistance);
        if (isHitting)
        {
            Debug.DrawRay(CornerTransform.position, -Vector3.up * hit.distance, Color.blue);
            float Distance = RaycastDistance - hit.distance;
            float kx = SuspensionStrength * Distance;
            float bv = 0;
            if (Time.fixedDeltaTime != 0)
            {
                bv = SuspensionDamping * ((Distance - PreviousDistance) / Time.fixedDeltaTime);
            }

            float Force = kx + bv;

            PreviousDistance = Distance;

            rb.AddForceAtPosition(Vector3.up * Force, CornerTransform.position, ForceMode.Force);

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // TODO: This is supposed to make walls feel "bouncy", but currently just makes the vehicle go spastic on collisions
        List<ContactPoint> contactPoints = new List<ContactPoint>();
        collision.GetContacts(contactPoints);
        rb.AddForce(collision.impulse, ForceMode.Impulse);

    }

}
