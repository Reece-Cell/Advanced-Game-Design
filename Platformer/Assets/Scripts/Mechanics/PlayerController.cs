using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip jumpReady;
        public bool superjump = false; // Boolean to track if Shift is held for 3 seconds
        private bool isShiftHeld = false; // Track if Shift key is held
        private float shiftHoldStartTime = 0f; // Time when Shift is first held
        public GameObject jumpEffect;
        public GameObject teleportEffect;
        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        private int jumps = 0;
        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        private float originalJump;
        private float teleportCooldown = 5.0f;
        private float lastTeleportTime = -5.0f;
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            originalJump = model.jumpModifier;
        }

        protected override void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (!isShiftHeld)
                {
                    shiftHoldStartTime = Time.time; // Record the time when Shift is first held
                    isShiftHeld = true;
                }

                float timeHeld = Time.time - shiftHoldStartTime;

                if (timeHeld >= 3.0f)
                {
                    superjump = true;
                }
            }
            else
            {
                isShiftHeld = false;
                shiftHoldStartTime = 0f;
                superjump = false;
            }
            if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastTeleportTime >= teleportCooldown)
            {
                float distance;
                if (Input.GetKeyDown(KeyCode.A))
                {
                    distance = -5.0f;
                }
                else
                {
                    distance = 5.0f;
                }
                Instantiate(teleportEffect, transform.position, Quaternion.identity);
                Vector3 newPosition = transform.position + new Vector3(distance, 0.0f, 0.0f);
                Instantiate(teleportEffect, transform.position, Quaternion.identity);
                transform.position = newPosition;
                lastTeleportTime = Time.time;
            }
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if ((jumpState == JumpState.Grounded && Input.GetButtonDown("Jump")) || Input.GetButtonDown("Jump") && jumps < 2)
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumps++;
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumps = 0;
                    isShiftHeld = false;
                    shiftHoldStartTime = 0f;
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (superjump == true && model.jumpModifier == originalJump)
            {
                model.jumpModifier += 0.75f;
                Debug.Log("Jump mod is: " + model.jumpModifier);
            }
            else
            {
                model.jumpModifier = originalJump;
            }
            if (jump && IsGrounded)
            {
                if (superjump == true)
                {
                    Vector3 spawnPosition = transform.position - new Vector3(0f, 0.5f, 0f);
                    Instantiate(jumpEffect, spawnPosition, Quaternion.identity);
                }
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}