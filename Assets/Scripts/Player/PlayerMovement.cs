using UnityEngine;

/*
 * This is the class we'll be using to manage player movement, namely horizontal movement and jumping.
 * 
 * Known issues:
 * - If we move into a wall, friction allows us to stay attached instead of still falling. Need to figure out a way to fix this
 */

namespace FITF
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Horizontal Movement")]
        public float m_movementSpeed = 5f;                      // Doesn't really have a set measurement, 5 is mostly an arbitrary value afaik
        [Range(0, 1)] public float m_movementLerpFactor = 0.3f; // Factor used for linear interpolation of movement. This applies either when we're grounded or if taking away control mid-air is turned off (i.e. m_enableAirControl is false, or "Enable Air Control" is unchecked in the inspector).

        [Header("Mid-air Movement")]
        public bool m_enableAirControl = true;                  // Determines whether or not we should have less control while airborne. Having this here so we can tinker with how it feels.
        [Range(0, 1)] public float m_airborneLerpFactor = 0.1f; // Same as m_movementLerpFactor, but specifically if m_enableAirControl is true and we're mid-air. This is used to give the effect of having a bit less control over movement while airborne.

        [Header("Jumping and Grounded")]
        public float m_jumpForce = 8.5f; // Force applied to us when we jump. Higher values means higher jumps.

        [Tooltip("How far below the player can ground be and the player still be considered grounded?")]
        public float m_groundedThreshold = 0.02f; // Read the tooltip attribute (the line above this one).

        public LayerMask m_groundLayers = Physics.DefaultRaycastLayers; // Defines what layers count as "ground". I.e. not enemies, not the player themselves.

        [Space]
        public float m_jumpGracePeriod = 0.2f; // Amount of time after leaving ground that the player can still jump, in seconds.

        [HideInInspector] public float m_horizontalInput; // Hidden from inspector, but this is what we adjust to account for player input.

        private Rigidbody2D m_rb;    // Grabbing the rigidbody.
        private BoxCollider2D m_col; // Grabbing our collider.

        private float m_groundedTime; // The time at which we were last grounded. Used for grace period calculations.
        
        // Checks to see whether or not we're grounded. Pretty straight forward, I think.
        public bool IsGrounded
        {
            get
            {
                /*
                 * Basically, we create an imaginary line from the bottom of the player, extending out by m_groundedThreshold units.
                 * 
                 * If the line collides with something in any of the layers in the layer mask, this says that the player is grounded. If not, it says that the player is, of course, NOT grounded.
                 */

                Vector2 startPos = new(transform.position.x, m_col.bounds.min.y);   // Sets point at bottom center of our collider.
                Vector2 rightBound = m_col.bounds.extents.x / 2 * Vector2.right;    // Where on the x-axis is our right bound? We only wanna calculate this once

                // A raycast on the bottom left, bottom center, and bottom right. Makes sure we can jump from ledges or if we're standing on two ledges on either side.
                RaycastHit2D[] casts = {
                    Physics2D.Raycast(startPos, Vector2.down, m_groundedThreshold, m_groundLayers),
                    Physics2D.Raycast(startPos - rightBound, Vector2.down, m_groundedThreshold, m_groundLayers),
                    Physics2D.Raycast(startPos + rightBound, Vector2.down, m_groundedThreshold, m_groundLayers)
                };

                bool hit = false;

                // If even one of these raycasts finds anything, we know we're on ground.
                foreach (var cast in casts)
                {
                    if (cast.collider)
                    {
                        hit = true;
                        break;
                    }
                }

                if (!hit) return false;

                m_groundedTime = Time.time; // Record our grounded time for grace period calculations.
                return true;
            }
        }

        // Called before Start. Used here to grab outside components we need.
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody2D>();
            m_col = GetComponent<BoxCollider2D>();
        }

        // Physics checks are done here. And so is movement, in our case.
        private void FixedUpdate()
        {
            DoMovement();
        }
        
        private void DoMovement()
        {
            float targetX = m_horizontalInput * m_movementSpeed;
            float factor = (IsGrounded || !m_enableAirControl) ? m_movementLerpFactor : m_airborneLerpFactor;

            m_rb.velocity = new(Mathf.Lerp(m_rb.velocity.x, targetX, factor), m_rb.velocity.y);

            // Lerp is short for linear interpolation. Basically, we're using it to smooth out our horizontal velocity
        }

        public void Jump()
        {
            // If we can't jump, don't.
            if (!IsGrounded && Time.time > m_jumpGracePeriod + m_groundedTime) return;

            m_groundedTime = Time.time - m_jumpGracePeriod - 0.01f; // Bump our grounded time past grace period to make sure we can't jump twice when we shouldn't be able to.
            m_rb.velocity = new(m_rb.velocity.x, m_jumpForce); // Jump! (we don't use AddForce because if we need to double jump, that will simply slow a fall (or boost a jump already happening) rather than just jump
        }
    }
}