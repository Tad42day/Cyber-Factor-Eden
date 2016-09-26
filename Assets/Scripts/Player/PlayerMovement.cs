using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {

    Animator animator;
    CharacterController characterController;

    [System.Serializable]
    public class AnimationSettings
    {
        public string verticalVelocityFloat = "Forward";
        public string horizontalVelocityFloat = "Strafe";
        public string groundedBool = "isGrounded";
        public string jumpBool = "isJumping";
    }
    [SerializeField]
    public AnimationSettings animations;

    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModifier = 9.81f;
        public float baseGravity = 50.0f;
        public float resetValueGravity = 1.2f;
    }
    [SerializeField]
    public PhysicsSettings physics;

    [System.Serializable]
    public class MovementSettings
    {
        public float moveSpeed;
        public float jumpSpeed = 6;
        public float jumpTime = 0.25f;
    }
    [SerializeField]
    public MovementSettings movement;

    private bool jumping;
    private bool resetGravity;
    private float gravity;
    private bool isGrounded = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        SetupAnimator();
    }

    void Update()
    {
        ApplyGravity();
        //Mover();
        isGrounded = characterController.isGrounded;
    }

    void Mover()
    {
        float rotateSpeed = 3.0f;

        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = movement.moveSpeed * Input.GetAxis("Vertical");
        characterController.SimpleMove(forward * curSpeed);
    }

    void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            if (!resetGravity)
            {
                gravity = physics.resetValueGravity;
                resetGravity = true;
            }

            gravity += Time.deltaTime * physics.gravityModifier;
        }
        else
        {
            gravity = physics.baseGravity;
            resetGravity = false;
        }

        Vector3 gravityVector = new Vector3();

        if (!jumping)
        {
            gravityVector.y -= gravity;
        }
        else
        {
            gravityVector.y = movement.jumpSpeed;
        }

        characterController.Move(gravityVector * Time.deltaTime);
        //characterController.SimpleMove(gravityVector * movement.moveSpeed);
    }

    void SetupAnimator()
    {
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvatar = wantedAnim.avatar;

        animator.avatar = wantedAvatar;
        Destroy(wantedAnim);
    }

    //public void Move(float forward, float strafe)
    //{
    //    ApplyGravity();
    //    Animate(forward, strafe);

    //    Vector3 moveVector = new Vector3(strafe, -gravity, forward);

    //    moveVector.z *= Time.deltaTime;
    //    moveVector.x *= Time.deltaTime;
    //    moveVector *= 3.0f;
    //    characterController.Move(moveVector);
    //}

    public void Animate(float forward, float strafe)
    {
        animator.SetFloat(animations.verticalVelocityFloat, forward);
        animator.SetFloat(animations.horizontalVelocityFloat, strafe);
        animator.SetBool(animations.groundedBool, isGrounded);
        animator.SetBool(animations.jumpBool, jumping);
    }

    public void Jump()
    {
        if (jumping)
        {
            return;
        }

        if (isGrounded)
        {
            jumping = true;
            StartCoroutine(StopJump());
        }
    }

    IEnumerator StopJump()
    {
        yield return new WaitForSeconds(movement.jumpTime);
        jumping = false;
    }
}
