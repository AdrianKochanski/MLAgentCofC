﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Agent")
        {
            BallAgent agent = other.GetComponent<BallAgent>();
            agent.EndEpisodeWithReward(0.2f);
        }
    }
}
