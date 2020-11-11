using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class BallAgent : Agent
{
    public Transform pointer;
    public Transform target;
    public float MaxSpeed = 6f;
    public float LookSpeed = 5f;
    public float UpSpeed = 5f;
    public float MovingSpeed= 5f;
    public float MaxMovingSpeed = 5f;
    public float RotationSpeed = 35f;
    public float RotationSpeedChange = 4f;
    [Range(0,0.7f)]
    public float JumpSpaceTime = 0.3f;

    private Rigidbody rigidbody;
    private float _forwardSpeed = 0f;
    private float _sideAngle = 0f;
    private float XSpeed = 0f;
    private float ZSpeed = 0f;
    private bool throwUp = false;
    private float lastJumpTime = 0f;
    private Quaternion lookRotation;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        LookAtTargetLocation(pointer, target, LookSpeed);
        UpdateAgentAttributes();
        UpdateTimers();
    }
    private void UpdateTimers() {
        lastJumpTime += Time.deltaTime;
    }
    private void UpdateAgentAttributes()
    {
        float deltaYAngle = Mathf.PI * transform.eulerAngles.y / 180f;
        float ZSpeed = Mathf.Cos(deltaYAngle) * _forwardSpeed;
        float XSpeed = Mathf.Sin(deltaYAngle) * _forwardSpeed;

        if(throwUp) {
            rigidbody.velocity = new Vector3(XSpeed, UpSpeed, ZSpeed);
            throwUp = false;
        }
        
        float velY = rigidbody.velocity.y;
        if (velY != 0f && Mathf.Abs(velY) > MaxSpeed) {
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, new Vector3(XSpeed, (velY / Mathf.Abs(velY)) * MaxSpeed, ZSpeed), Time.deltaTime * RotationSpeedChange);
        } else {
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, new Vector3(XSpeed, velY, ZSpeed), Time.deltaTime * RotationSpeedChange);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, _sideAngle, transform.eulerAngles.z);
    }

    private void LookAtTargetLocation(Transform arrow, Transform lookAt, float lookSpeed)
    {
        Vector3 _direction = (lookAt.position - transform.position).normalized;
        Quaternion _lookRotation = Quaternion.LookRotation(_direction);
        lookRotation = _lookRotation;
        arrow.rotation = Quaternion.Slerp(arrow.rotation, _lookRotation, Time.deltaTime * lookSpeed);
    }
    public override void Initialize()
    {
        _forwardSpeed = 0;
        _sideAngle = 0;
    }
    public override void OnEpisodeBegin()
    {
        Initialize();
        transform.position = new Vector3(Random.Range(-2f, 2f), Random.Range(2f, 5f), Random.Range(-4f, -2f));
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.velocity = Vector3.zero;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lookRotation.x);
        sensor.AddObservation(lookRotation.y);
        sensor.AddObservation(Vector3.Distance(transform.position, target.position));
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        if (Mathf.Approximately(vectorAction[0], 1f))
        {
            if(lastJumpTime > JumpSpaceTime)
            {
                throwUp = true;
                lastJumpTime = 0f;
            }
        }
        if (Mathf.Approximately(vectorAction[1], 1f))
        {
            _forwardSpeed += Time.deltaTime * MovingSpeed;
            _forwardSpeed = Mathf.Min(_forwardSpeed, MaxMovingSpeed);
        }
        if (Mathf.Approximately(vectorAction[2], 1f))
        {
            _forwardSpeed -= Time.deltaTime * MovingSpeed;
            _forwardSpeed = Mathf.Max(_forwardSpeed, -MaxMovingSpeed);
        }
        if (Mathf.Approximately(vectorAction[3], 1f))
        {
            _sideAngle -= Time.deltaTime * RotationSpeed;
        }
        if (Mathf.Approximately(vectorAction[4], 1f))
        {
            _sideAngle += Time.deltaTime * RotationSpeed;
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        actionsOut[1] = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
        actionsOut[2] = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
        actionsOut[3] = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
        actionsOut[4] = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
    }
}
