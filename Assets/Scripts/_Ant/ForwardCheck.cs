using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ForwardCheck : MonoBehaviour
{
    [FormerlySerializedAs("detected")] public bool anotherCarDetected;
    public bool giveSide;

    public float value;
    [SerializeField] private AntBehaviour ant;

    private void Awake()
    {
        ant = GetComponentInParent<AntBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            anotherCarDetected = true;
        }

        other.TryGetComponent(out ForwardCheck forwardCheck);
        if (forwardCheck)
        {
            giveSide = true;
            if (value > forwardCheck.value)
            {
                ant.MoveRight();
                Debug.Log("move Right");
            }

            if (value < forwardCheck.value)
            {
                Debug.Log("move Left ");
                ant.MoveLeft();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            anotherCarDetected = false;
        }

        other.TryGetComponent(out ForwardCheck forwardCheck);
        if (forwardCheck)
        {
            giveSide = false;
        }
    }
}