using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class BallAgent : Agent
{
    public Transform pointer;
    public Transform target;
    public Transform earth;
    public int NumPointsInGame = 3;
    public float MaxSpeed = 6f;
    public float LookSpeed = 5f;
    public float UpSpeed = 5f;
    public float MovingSpeed= 5f;
    public float MaxMovingSpeed = 5f;
    public float RotationSpeedChange = 90f;
    public float RewardFraction = 25f;
    [Range(0,0.7f)]
    public float JumpGapTime = 0.45f;
    [Range(5f, 30f)]
    public float MaxDistanceToTarget = 15f;
    [Range(0, 1f)]
    public float MinDistanceToTarget = 0.3f;
    [Range(0, 1f)]
    public float BreakFraction = 0.1f;
    public float MaintainedAltitudeDiffHeight = 2f;

    private Rigidbody rigidbody;
    private float _forwardSpeed = 0f;
    private float _sideAngle = 0f;
    private float XSpeed = 0f;
    private float ZSpeed = 0f;
    private bool throwUp = false;
    private float lastJumpTime = 0f;
    private Quaternion lookRotation;
    private float distanceToTarget = 1f;
    private int points;
    private int JumpSuccess;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
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
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        float deltaYAngle = Mathf.PI * transform.eulerAngles.y / 180f;

        ZSpeed = Mathf.Cos(deltaYAngle) * _forwardSpeed;
        XSpeed = Mathf.Sin(deltaYAngle) * _forwardSpeed;
        if (throwUp) {
            rigidbody.velocity = new Vector3(XSpeed, UpSpeed, ZSpeed);
            throwUp = false;
        }
        
        float velY = rigidbody.velocity.y;
        if (velY != 0f && Mathf.Abs(velY) > MaxSpeed) {
            rigidbody.velocity = new Vector3(XSpeed, (velY / Mathf.Abs(velY)) * MaxSpeed, ZSpeed);
        } else {
            rigidbody.velocity = new Vector3(XSpeed, velY, ZSpeed);
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
        points = 0;
        JumpSuccess = 0;
    }
    public override void OnEpisodeBegin()
    {
        Initialize();
        transform.position = new Vector3(earth.position.x + Random.Range(-2f, 2f), earth.position.y + Random.Range(3f, 6f), earth.position.z + Random.Range(-2f, 2f));
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.velocity = Vector3.zero;
        NewTargetPosition();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lookRotation.x);
        sensor.AddObservation(lookRotation.y);
        sensor.AddObservation(distanceToTarget);
        sensor.AddObservation(rigidbody.velocity);
        sensor.AddObservation(Quaternion.LookRotation(target.forward, target.up));
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        if (vectorAction[0] == 1)
        {
            if(lastJumpTime > JumpGapTime)
            {
                throwUp = true;
                lastJumpTime = 0f;
                if(JumpSuccess < 10)
                {
                    if (Mathf.Abs(transform.position.y - target.position.y) < MaintainedAltitudeDiffHeight)
                    {
                        AddReward(0.05f);
                    }
                    else AddReward(-0.05f);
                }
            }
        }
        if (vectorAction[1] == 1)
        {
            _forwardSpeed += Time.deltaTime * MovingSpeed;
            _forwardSpeed = Mathf.Min(_forwardSpeed, MaxMovingSpeed);
        }
        if (vectorAction[2] == 1)
        {
            _forwardSpeed -= Time.deltaTime * MovingSpeed;
            _forwardSpeed = Mathf.Max(_forwardSpeed, -MaxMovingSpeed);
        }
        if (vectorAction[3] == 1)
        {
            _sideAngle -= Time.deltaTime * RotationSpeedChange;
        }
        if (vectorAction[4] == 1)
        {
            _sideAngle += Time.deltaTime * RotationSpeedChange;
        }
        if(distanceToTarget > MaxDistanceToTarget)
        {
            EndEpisodeWithReward(-1f);
        }
        if (distanceToTarget < MinDistanceToTarget)
        {
            AddPoint();
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

    public void EndEpisodeWithReward(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }

    private void NewTargetPosition()
    {
        float randAngle = Random.Range(0,6.28f);
        float circleRenderLength = 6f;
        target.position = new Vector3(earth.position.x + Mathf.Sin(randAngle) * circleRenderLength, earth.position.y + Random.Range(4f, 6f), earth.position.z + Mathf.Cos(randAngle) * circleRenderLength);
        target.eulerAngles = new Vector3(-90f, 0, Random.Range(0, 360f));
    }

    public void AddPoint()
    {
        points++;
        if(points >= NumPointsInGame)
        {
            EndEpisodeWithReward(2f);
        } else
        {
            print("Points++ : " + points);
            AddReward(0.5f);
            NewTargetPosition();
        }
    }

    //public void AddSpeedReward()
    //{
    //    if (lastRewardTime > RewardGapTime)
    //    {
    //        lastRewardTime = 0f;
    //        AddReward(_forwardSpeed / (_forwardSpeed* RewardFraction));
    //    }
    //}
}
