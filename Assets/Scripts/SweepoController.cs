using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SweepoController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public class Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [Serializable]
    public class Brush
    {
        public GameObject brushModel;
        public float rotationSpeed;
    }

    public float maxAcceleration = 30f;
    public float brakeAcceleration = 50f;
    public float turnSenstivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;
    public List<Brush> brushes;

    private float moveInput;
    private float steerInput;
    private bool isBrushRotating = false;
    private bool isPlayerDriving = false; // Flag to check if the player is driving

    private Rigidbody sweeperRb;

    void Start()
    {
        sweeperRb = GetComponent<Rigidbody>();
        sweeperRb.centerOfMass = _centerOfMass;
    }

    void Update()
    {
        if (isPlayerDriving)
        {
            GetInputs();
            AnimateWheels();
            AnimateBrushes();
        }
    }

    void LateUpdate()
    {
        if (isPlayerDriving)
        {
            Move();
            Steer();
            Brake();
        }
    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.T))
        {
            isBrushRotating = !isBrushRotating; // Toggle brush rotation on T key press
        }
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSenstivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = brakeAcceleration; // Apply brake torque directly
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void AnimateBrushes()
    {
        if (isBrushRotating)
        {
            for (int i = 0; i < brushes.Count; i++)
            {
                var brush = brushes[i];
                // Check if the brush is on the left or right side
                float rotationDirection = i % 2 == 0 ? 1 : -1; // Even index brushes rotate clockwise, odd index brushes rotate counterclockwise
                brush.brushModel.transform.Rotate(Vector3.up * brush.rotationSpeed * Time.deltaTime * rotationDirection);
            }
        }
    }

    // Methods to set the isPlayerDriving flag
    public void StartDriving()
    {
        isPlayerDriving = true;
    }

    public void StopDriving()
    {
        isPlayerDriving = false;
    }

    // Handle collision with cleanable items
    private void OnTriggerEnter(Collider other)
    {
        if (isBrushRotating && other.CompareTag("Cleanable"))
        {
            StartCoroutine(DeleteAfterDelay(other.gameObject, 1f));
        }
    }

    // Coroutine to delete the game object after a delay
    private IEnumerator DeleteAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        GarbageManager.Instance.CollectFloorRubbish();
        Destroy(obj);
    }
}