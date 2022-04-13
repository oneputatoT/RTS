using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    Rigidbody rd;
    Vector3 moveInput;
    [SerializeField] float  moveSpeed;

    private void Start()
    {
        rd = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate()
    {
        rd.velocity = moveInput.normalized * moveSpeed * Time.fixedDeltaTime *10;
    }
}
