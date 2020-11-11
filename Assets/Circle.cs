using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    //private void Start()
    //{
    //    //print("Circle detection started!!!");
    //}

    private void OnTriggerEnter(Collider other)
    {
        print("Agent broke circle!!!");
        if (other.tag == "Agent")
        {
            print("Agent broke circle!!!");
        }
    }
}
