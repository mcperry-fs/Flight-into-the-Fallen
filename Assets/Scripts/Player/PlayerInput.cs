using UnityEngine;

/*
 * We'll be making use of Unity's new Input System.
 * 
 * Things function on triggers instead of just needing to make use of the Input class, but it allows for more flexibility.
 * For example, if you plug in a controller, the controls SHOULD work just fine (i haven't tested controllers yet, just keyboard so far).
 * 
 * This is the class we'll be using to manage input and give values to scripts as needed.
 * 
 * Known issues:
 * - None at the moment. Controller support needs testing, however. (that's not an issue for here, though)
 */

namespace FITF
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInput : MonoBehaviour
    {
        private Controls m_controls;
        private PlayerMovement m_movement;

        private void Awake()
        {
            m_controls = new();
            m_movement = GetComponent<PlayerMovement>();

            m_controls.Game.Jump.performed += ctx => m_movement.Jump();
        }

        private void FixedUpdate()
        {
            m_movement.m_horizontalInput = m_controls.Game.HorizontalMovement.ReadValue<float>();
        }
        private void OnEnable()
        {
            m_controls.Enable();
        }

        private void OnDisable()
        {
            m_controls.Disable();
        }

    }
}