using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour, IPlayerControl
{
    public Camera mainCamera;

    /// <summary>
    /// 임시 : slide event 받아서 speed 처리
    /// </summary>
    [SerializeField] private SlideEvent slider;

    [Header("Camera Movement Speed by Key Control")] [SerializeField]
    private float movementSpeed;

    [Header("Camera Rotation Speed by Mouse Control")] [SerializeField]
    private float rotationSpeed;



    [Header("Jump Speed by Key Control")] [SerializeField]
    private float jumpSpeed;

    [SerializeField] private float gravity;


    private float horizontal = 0f;
    private float vertical = 0f;

    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 projCamForward;
    private Quaternion rotationToCam;

    private bool isJumped;
    private bool isPaused;


    // rSpeed : movementSpeed 에 speedSlide를 곱한 값.
    // rGravity : gravity 에 speedSlide를 곱한 값.
    // inspector 에서 정의된 "기본값"에 speedSlide를 곱한 값을 유동적으로 사용하기 위해 prefix r변수 를 사용함.
    private float rSpeed;
    private float rGravity;
    private float rJumpSpeed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        slider.OnChangeSlide += OnSpeedChange;
        OnSpeedChange(1.0f); //r변수에 기본값 정의를 위해 parma(1.0)으로 함수 실행.
    }

    void OnSpeedChange(float speedScale)
    {
        rJumpSpeed = jumpSpeed * speedScale;
        rSpeed = movementSpeed * speedScale;
        rGravity = gravity * speedScale;
    }

    void OnEnable()
    {
        InputControl.GetInstance.OnMoveInputEvent += OnMoveInput;
        InputControl.GetInstance.OnJumpKeyAction += OnJumpInput;
    }

    void OnDisable()
    {
        try
        {
            InputControl.GetInstance.OnMoveInputEvent -= OnMoveInput;
            InputControl.GetInstance.OnJumpKeyAction -= OnJumpInput;
        }
        catch (NullReferenceException ex)
        {
            Debug.Log("InputControl is null");
        }
    }

    public void OnMoveInput(float x, float y)
    {
//        Debug.Log($"movement:{x}, {y}");
        horizontal = x;
        vertical = y;
    }

    public void OnJumpInput()
    {
        //RoomManager.GetInstance.debugTxt.text = "isGround : " + controller.isGrounded+ " /  isJumped : " + isJumped;
        if (controller.isGrounded) isJumped = true;
    }

    public void OnPaused(bool status)
    {
        isPaused = status;
    }

    public void MovementPlayer()
    {
        if (controller.isGrounded)
        {
            moveDirection = Vector3.forward * vertical+ Vector3.right * horizontal;
            projCamForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up);
            Quaternion rotationToCam = Quaternion.LookRotation(projCamForward, Vector3.up);
            moveDirection = rotationToCam * moveDirection;

            transform.rotation = Quaternion.LookRotation(projCamForward, Vector3.up);
            // transform.position += moveDirection * movementSpeed * Time.deltaTime;

            // Move 스피드 적용
            moveDirection *= rSpeed;

            // 캐릭터 점프
            if (isJumped)
            {
                moveDirection.y = rJumpSpeed;
                isJumped = false;
            }
        }

        // 캐릭터에 중력 적용.
        moveDirection.y -= rGravity * Time.deltaTime;
        // 점프 초기화 
        //if (moveDirection.y<= 0) isJumped = false;

        // 캐릭터 떨어지는 경우
        if (moveDirection.y <= -30f)
        {
            // Debug.Log("  moveDirection.y  : " + moveDirection.y + " / transform.position.y : " + transform.position.y);
            //moveDirection.y = 50f;
            transform.position = new Vector3(0, 1f, 2f);
            moveDirection = Vector3.zero;
        }
        else
        {
            // 캐릭터 움직임.
            controller.Move(moveDirection * Time.deltaTime);
        }
    }

    private void Update()
    {
        if (isPaused) return;
        MovementPlayer();
    }
}