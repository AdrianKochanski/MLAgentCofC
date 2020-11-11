using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAgent : MonoBehaviour
{
    public Transform agent;
    public float turnSpeed = 4f;
    [Range(0, 1)]
    public float histereza = 0.01f;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = new Vector3(agent.position.x, agent.position.y, agent.position.z);
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        float delta = Mathf.DeltaAngle(Mathf.Abs(agent.eulerAngles.y), Mathf.Abs(transform.eulerAngles.y))/360f;
        
        if(Mathf.Abs(delta) > histereza)
        {
            offset = Quaternion.AngleAxis(delta * turnSpeed, Vector3.up) * offset;
        }
        transform.position = agent.position + offset;
        transform.LookAt(agent.position);
    }
}
