using UnityEngine;
using UnityEngine.InputSystem;

namespace TCCGame.Core.Input
{
    public static class GameInput
    {
        private const string MoveAction = "Player/Move";
        private const string InteractAction = "Player/Interact";
        private const string SubmitAction = "UI/Submit";
        private const string CancelAction = "UI/Cancel";


        public static Vector2 ReadMovement()
        {
            InputAction action = FindAction(MoveAction);
            return action?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public static bool WasInteractPressedThisFrame()
        {
            return FindAction(InteractAction)?.WasPressedThisFrame() ??
                false;
        }

        public static bool WasSubmitPressedThisFrame()
        {
            return FindAction(SubmitAction)?.WasPressedThisFrame() ?? false;
        }

        public static bool WasCancelPressedThisFrame()
        {
            return FindAction(CancelAction)?.WasPressedThisFrame() ?? false;
        }

        private static InputAction FindAction(string actionName)
        {
            return InputSystem.actions?.FindAction(
                actionName,
                throwIfNotFound: false
            );
        }
    }
}
