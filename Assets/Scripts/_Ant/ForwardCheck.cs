using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardCheck : MonoBehaviour
{
    public bool detected;
    public bool giveSide;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            detected = true;
        }

        other.TryGetComponent(out ForwardCheck forwardCheck);
        if (forwardCheck)
        {
            giveSide = true; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            detected = false;
        }

        other.TryGetComponent(out ForwardCheck forwardCheck);
        if (forwardCheck)
        {
            giveSide = false; 
        }
    }
}