using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player_controller : MonoBehaviour
{
    public Transform[] wheelTransforms; // Assign FL, FR, RL, RR in Inspector

    public GameObject[] wheelObjects; //Wheel gameobjects of car (visual)

    public float maxSuspensionLength = 1f; // Reduced for more realistic suspension


    float[] oldDist = new float[4]; 

    public float suspensionMultiplier = 2000f;
    public float dampSensitivity = 0.005f;
    public float maxDamp = 500f;

    public float brakeForce = 2000f;

    public float speed = 35f;

    public float rotationSpeed = 5;

    public LayerMask groundLayer = ~0;

    public float tractionStrength = 50f;
    Rigidbody rb;

    private Vector2 inputVector;

    private bool isBraking = false;

    private bool isDrifting = false;

    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        //rb.drag = 0f; // Ensure no drag interferes
        //rb.angularDrag = 0.05f; // Small angular drag for stability
    }

    void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
        Debug.Log("Move input: " + inputVector);
    }

    void OnBrake(InputValue value)
    {
        isBraking = value.isPressed;
        Debug.Log("Brake pressed: " + isBraking);
    }

    void OnDrift(InputValue value)
    {
        isDrifting = value.isPressed;
        Debug.Log("Drift button pressed");
    }

    bool IsGroundedViaForces() //check if car is grounded by checking the suspension forces on each wheel
    {
        float totalForce = 0f;
        for (int i = 0; i < 4; i++)
        {
            if (Physics.Raycast(wheelTransforms[i].position,
                          -transform.up,
                          out RaycastHit hit,
                          maxSuspensionLength))
            {
                totalForce += Mathf.Clamp(maxSuspensionLength - hit.distance, 0, 1); //
            }
        }

        return totalForce > 0.1f; //significant suspension forces detected (grounded), true if total force is greater than 0.1
    }


    void FixedUpdate()
    {
        Vector3 carVelocity = rb.velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(carVelocity); //velocity relative to car's forward (transform world space to local space)
        float sidewaySpeed = localVelocity.x;
        float forwardSpeed = localVelocity.z;

        for (int i = 0; i < 4; i++)
        {
            Vector3 wheelPos = wheelTransforms[i].position;

            if (Physics.Raycast(wheelPos, -transform.up, out RaycastHit hit, maxSuspensionLength)) //checks to see if raycast hits ground, max range is maxSuspensionLength
            {
                //calculate force 
                float springForce = Mathf.Clamp(maxSuspensionLength - hit.distance, 0, 1) * suspensionMultiplier; //always upward force
                float dampForce = Mathf.Clamp((oldDist[i] - hit.distance) * dampSensitivity, 0, maxDamp); //opposes motion of the car

                wheelObjects[i].transform.position = hit.point + new Vector3(0,0.15f,0); //set wheel position to hit point



                //if (carVelocity.y > 0) //applies damping in opposite direction (if moving up damp is down, if moving down damp is up)
                //{
                    //dampForce = dampForce * -1; //oppose motion
                //}

                rb.AddForceAtPosition((springForce + dampForce) * transform.up, wheelPos); //add force to each wheel --no need for time.deltaTime for fixedUpdate                

            }
            else
            {
                rb.AddForceAtPosition(Vector3.zero, wheelPos); //otherwise don't add force
                wheelObjects[i].transform.position = wheelTransforms[i].position - wheelTransforms[i].up * maxSuspensionLength/3; //fully extended
            }

            oldDist[i] = hit.distance; //set the oldDist to current distance for next check

        }

        bool grounded = IsGroundedViaForces();

        if (grounded && isDrifting == true)//drift mode
        {
            if (isBraking == true)
            {
                if (rb.velocity.magnitude > 0) //only if moving
                {
                    rb.AddForce(-rb.velocity.normalized * brakeForce); //apply brakeforce in direction of motion
                    Debug.Log("Braking");
                }
            }

            else
            {
                Debug.Log("Grounded detected");
                // Apply normal driving forces
                Vector3 movement = transform.forward * inputVector.y * speed;
                rb.AddForce(movement, ForceMode.Acceleration);

                if (Mathf.Abs(forwardSpeed) > 0.1f) //only turn if moving forward/backward is pressed **new
                {
                    Vector3 turning = transform.up * inputVector.x * rotationSpeed;
                    rb.AddTorque(turning, ForceMode.Acceleration);
                }
            }

        }
        else if (grounded && isDrifting == false)
        {
            if (isBraking == true)
            {
                if (rb.velocity.magnitude > 0) //only if moving
                {
                    rb.AddForce(-rb.velocity.normalized * brakeForce); //apply brakeforce in opposite direction to motion
                    Debug.Log("Braking");
                }
            }

            else
            {

                Vector3 movement = transform.forward * inputVector.y * speed;
                rb.AddForce(movement, ForceMode.Acceleration);

                if (Mathf.Abs(forwardSpeed) > 0.1f) //only turn if moving forward/backwards is pressed **new
                {

                    Vector3 turning = transform.up * inputVector.x * rotationSpeed;
                    rb.AddTorque(turning, ForceMode.Acceleration);
                }

                Vector3 counterForce = transform.right * -sidewaySpeed * tractionStrength; //counteract the sideway force so car moves in straight line, need to multiply by tractionStrength as sidewaySpeed is in m.s-1
                rb.AddForce(counterForce, ForceMode.Acceleration);

            }
        }

    }
                 
        
}
