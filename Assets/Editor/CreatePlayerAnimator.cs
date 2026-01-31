using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace GameEditor
{
    public static class CreatePlayerAnimator
    {
        [MenuItem("Tools/Create Player Animator Controller")]
        public static void Create()
        {
            string path = "Assets/Prefabs/Characters/Animations/Explorer/PlayerAnimator.controller";

            // Create the Animator Controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Add parameters
            controller.AddParameter("State", AnimatorControllerParameterType.Int);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Land", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WallCling", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WallJump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);

            // Get the root state machine
            var rootStateMachine = controller.layers[0].stateMachine;

            // Create states
            var movingState = rootStateMachine.AddState("Moving", new Vector3(200, 0, 0));
            var jumpingState = rootStateMachine.AddState("Jumping", new Vector3(200, -80, 0));
            var fallingState = rootStateMachine.AddState("Falling", new Vector3(200, -160, 0));
            var wallClingState = rootStateMachine.AddState("WallCling", new Vector3(400, -80, 0));
            var wallJumpState = rootStateMachine.AddState("WallJump", new Vector3(400, -160, 0));

            // Set Moving as default state
            rootStateMachine.defaultState = movingState;

            // Create transitions based on State parameter
            // Moving -> Jumping (State == 1)
            var toJumping = movingState.AddTransition(jumpingState);
            toJumping.AddCondition(AnimatorConditionMode.Equals, 1, "State");
            toJumping.hasExitTime = false;
            toJumping.duration = 0.1f;

            // Moving -> Falling (State == 2)
            var toFalling = movingState.AddTransition(fallingState);
            toFalling.AddCondition(AnimatorConditionMode.Equals, 2, "State");
            toFalling.hasExitTime = false;
            toFalling.duration = 0.1f;

            // Jumping -> Falling (State == 2)
            var jumpToFall = jumpingState.AddTransition(fallingState);
            jumpToFall.AddCondition(AnimatorConditionMode.Equals, 2, "State");
            jumpToFall.hasExitTime = false;
            jumpToFall.duration = 0.1f;

            // Jumping -> Moving (State == 0)
            var jumpToMove = jumpingState.AddTransition(movingState);
            jumpToMove.AddCondition(AnimatorConditionMode.Equals, 0, "State");
            jumpToMove.hasExitTime = false;
            jumpToMove.duration = 0.1f;

            // Falling -> Moving (State == 0)
            var fallToMove = fallingState.AddTransition(movingState);
            fallToMove.AddCondition(AnimatorConditionMode.Equals, 0, "State");
            fallToMove.hasExitTime = false;
            fallToMove.duration = 0.1f;

            // Falling -> WallCling (State == 3)
            var fallToWallCling = fallingState.AddTransition(wallClingState);
            fallToWallCling.AddCondition(AnimatorConditionMode.Equals, 3, "State");
            fallToWallCling.hasExitTime = false;
            fallToWallCling.duration = 0.1f;

            // WallCling -> WallJump (State == 4)
            var clingToWallJump = wallClingState.AddTransition(wallJumpState);
            clingToWallJump.AddCondition(AnimatorConditionMode.Equals, 4, "State");
            clingToWallJump.hasExitTime = false;
            clingToWallJump.duration = 0.1f;

            // WallCling -> Falling (State == 2)
            var clingToFall = wallClingState.AddTransition(fallingState);
            clingToFall.AddCondition(AnimatorConditionMode.Equals, 2, "State");
            clingToFall.hasExitTime = false;
            clingToFall.duration = 0.1f;

            // WallJump -> Falling (State == 2)
            var wallJumpToFall = wallJumpState.AddTransition(fallingState);
            wallJumpToFall.AddCondition(AnimatorConditionMode.Equals, 2, "State");
            wallJumpToFall.hasExitTime = false;
            wallJumpToFall.duration = 0.1f;

            // WallJump -> Moving (State == 0)
            var wallJumpToMove = wallJumpState.AddTransition(movingState);
            wallJumpToMove.AddCondition(AnimatorConditionMode.Equals, 0, "State");
            wallJumpToMove.hasExitTime = false;
            wallJumpToMove.duration = 0.1f;

            // Save
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created Player Animator Controller at: {path}");
            Selection.activeObject = controller;
        }
    }
}
