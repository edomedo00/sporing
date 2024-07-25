using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float rotationSpeed = 1;
    [SerializeField] float gravity = 9.8f;
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] LayerMask fallMask;
    Vector3 direction = Vector3.forward;
    PlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        FungiManager.Singleton.FollowPlayer();
    }

    void PlayerMovement()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 direction = new(input.x, 0, input.y);

        Quaternion camRotation = Quaternion.Euler(new(0, Camera.main.transform.eulerAngles.y, 0));

        this.direction = camRotation * direction;
        controller.Move(speed * Time.deltaTime * this.direction + GravityVector());

        if (direction.magnitude < 0.01) return;

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(new(0, angle, 0));
        Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }

    float fallVelocity = 0;
    Vector3 GravityVector()
    {
        if (controller.isGrounded)
        {
            fallVelocity = 0;
            return Vector3.down * fallVelocity;
        }
        
        fallVelocity += gravity * Time.deltaTime;

        return fallVelocity * Time.deltaTime * Vector3.down;
    }
}
