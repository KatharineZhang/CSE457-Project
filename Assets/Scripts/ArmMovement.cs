using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class ArmMovement : MonoBehaviour
    {
        // the two consts are used in armSwing rotations, not important
        private const float LEFT = -1.0f;
        private const float RIGHT = 1.0f;
        public FirstPersonController playerController;
        private CharacterController characterController;
        public Transform leftArm;
        public Transform rightArm;
        // change these later
        public Vector3 rightArmIdlePosition = new Vector3(0.8f,-1f,0.8f);
        public Vector3 leftArmIdlePosition = new Vector3(-0.8f,-1f,0.8f);
        // Settings to control the bob amount during walking and sprinting
        public float walkBobSpeed = 10f;
        public float walkBobAmountX = 0.2f;
        public float walkBobAmountY = 0.15f;
        public float sprintBobSpeed = 14f;
        public float sprintBobAmountX = 0.3f;
        public float sprintBobAmountY = 0.2f;
        // how quickly arms return and blend
        public float bobBlend = 10f;
        public float returnSmoothing = 6f;

        // Internal state trackers
        private float _bobTimer;
        private Vector3 _leftArmCurrentOffset;
        private Vector3 _rightArmCurrentOffset;

        // variables to control swinging
        public float swingSpeed = 600f;
        public float swingArc = 90f;
        public float returnSpeed = 4f;
        public float swingPercentage = 0.65f;
        // If true, swings both arms alternating, if false just right arm
        public bool alternateArms = true;
        private float _leftSwingProgress;
        private float _rightSwingProgress;
        private bool _leftSwinging;
        private bool _rightSwinging;
        private bool _nextSwingIsLeft;

        void Start()
        {
            characterController = playerController.GetComponent<CharacterController>();
            _leftArmCurrentOffset = Vector3.zero;
            _rightArmCurrentOffset = Vector3.zero;
        }
        // Update is called before LateUpdate, any kind of special movement
        // that we assign will have its corresponding action method called here
        // for example: if we add a special key binding for crouching, or picking up
        // an item it will be called here every frame
        void Update()
        {
            handleSwingInput();
        }
        void LateUpdate()
        {
            // safeguard so the script doesn't throw a NRE if the controllers get messed up
            if (playerController == null || characterController == null) return;
            float currentSpeed = getHorizontalSpeed();
            bool isSprinting = currentSpeed > playerController.MoveSpeed + 0.5f;
            bool isGrounded = playerController.Grounded;
            float bobSpeed, bobX, bobY;
            Vector3 leftBob = Vector3.zero;
            Vector3 rightBob = Vector3.zero;
            if (currentSpeed > 0.1f && isGrounded)
            {
                // assign bob accordingly
                if (isSprinting)
                {
                    bobSpeed = sprintBobSpeed;
                    bobX = sprintBobAmountX;
                    bobY = sprintBobAmountY;
                } else
                {
                    bobSpeed = walkBobSpeed;
                    bobX = walkBobAmountX;
                    bobY = walkBobAmountY;
                }
                float speedRatio = currentSpeed / playerController.SprintSpeed;
                // advance the bob timer based on speed
                _bobTimer += Time.deltaTime * bobSpeed * speedRatio;
                // A little convoluted, but the left arm and right arm should be opposite
                // because of how feet move
                float sinVal = Mathf.Sin(_bobTimer);
                float cosVal = Mathf.Cos(_bobTimer);
                // Right arm bobs with Sin, whilst the left arm is offset
                // by pi to have opposite phases, still playing with this
                // to figure out natural arm movement
                rightBob = new Vector3(cosVal*bobX, -Mathf.Abs(sinVal) * bobY, 0f);
                leftBob = new Vector3(cosVal*bobX, -Mathf.Abs(Mathf.Sin(_bobTimer + Mathf.PI)) * bobY, 0f);
            } else
            {
                // Not moving, slow down the bob timer so arms naturally go
                // back to rest position
                _bobTimer = _bobTimer + (-_bobTimer * Time.deltaTime * 2f);
            }
            float smooth;
            if (currentSpeed > 0.1f)
            {
                smooth = bobBlend;
            } else
            {
                smooth = returnSmoothing;
            }
            // Vector3.lerp (a,b,t) returns a value -> a + ((b-a)*t), so when t = 0
            // you just get a, and when t = 1 you get b, and when t= 0.5 you get the midpoint
            // Below, the smooth multiplier controls how aggressive the blending is for moving towards the
            // final position, so that arms will move fast in the beginning but slow doen as they reach
            // their final position if you aren't moving, or move normally if you are moving
            _leftArmCurrentOffset = Vector3.Lerp(_leftArmCurrentOffset, leftBob, Time.deltaTime * smooth);
            _rightArmCurrentOffset = Vector3.Lerp(_rightArmCurrentOffset, rightBob, Time.deltaTime * smooth);
            // NOW APPLY FINAL POSITIONS and call swing animations
            // NOTE that refs in C# are like reference "&" in C++ function params, the more you know!
            // (Sorry if you already knew the above fact)
            if (leftArm != null)
            {
                leftArm.localPosition = leftArmIdlePosition + _leftArmCurrentOffset;
                animateSwing(leftArm, ref _leftSwingProgress, ref _leftSwinging, LEFT);
            }
            if (rightArm != null)
            {
                rightArm.localPosition = rightArmIdlePosition + _rightArmCurrentOffset;
                animateSwing(rightArm, ref _rightSwingProgress, ref _rightSwinging, RIGHT);
            }

        }
        // Finds out if the player character is currently moving parallel
        // to the ground
        float getHorizontalSpeed()
        {
            Vector3 velocity = characterController.velocity;
            return new Vector3(velocity.x, 0f, velocity.z).magnitude;
        }
        // if the input is a left click, the program checks if dual wielding is on
        // or not, then checks which arm should be swinging next if dual wielding is on
        void handleSwingInput()
        {
            // this abomination of a key check is part of the UnityEngine.InputSystem
            // which we must use to not override the existing controller, ugh
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (alternateArms == true)
                {
                    if (_nextSwingIsLeft && !_leftSwinging)
                    {
                        _leftSwinging = true;
                        _leftSwingProgress = 0f;
                        _nextSwingIsLeft = false;
                    }
                    else if (!_nextSwingIsLeft && !_rightSwinging)
                    {
                        _rightSwinging = true;
                        _rightSwingProgress = 0f;
                        _nextSwingIsLeft = true;
                    }
                } else
                {
                    // while I don't technically need the _rightSwinging logic
                    // it does help for the animateArms method I call in LateUpdate
                    // and not having to write a bunch of split versions in other functions
                    if (!_rightSwinging)
                    {
                        _rightSwinging = true;
                        _rightSwingProgress = 0f;
                    }
                }
            }
        }
        // the function that controls swing animations
        void animateSwing(Transform arm, ref float progress, ref bool swinging, float side)
        {
            // if swinging, advance progress incrementally by the current swing speed
            if (swinging)
            {
                progress += (swingSpeed / swingArc) * Time.deltaTime;
                // if the progress value gets over this value, the abck of the arm model
                // comes into view and it looks bad, so we cap it at 0.85f
                if (progress >= swingPercentage)
                {
                    progress = swingPercentage;
                    swinging = false;
                }
            }
            else if (progress > 0.001f)
            {
                progress = Mathf.Lerp(progress, 0f, returnSpeed * Time.deltaTime*1.3f);
            }
            else
            {
                progress = 0f;
            }
            float angle;
            if (progress < 0.3f)
            {
                // Phase 1: Wind up (0 to 0.3) — arm raises backward
                float windUpT = progress / 0.3f;
                angle = -Mathf.Sin(windUpT * Mathf.PI * 0.5f) * swingArc * 0.4f;
            }
            else
            {
                // Phase 2: Swing down (0.3 to 1.0) — arm swings forward and down
                float swingT = (progress - 0.3f) / 0.7f;
                angle = Mathf.Lerp(-swingArc * 0.4f, swingArc, swingT);
            }
            Vector3 swingAxis = new Vector3(1f, 0f, side).normalized;
            arm.localRotation = Quaternion.AngleAxis(angle, swingAxis);
            float extend = Mathf.Sin(progress * Mathf.PI) * 0.5f;
            arm.localPosition += new Vector3(0f, 0f, extend);
        }

        // because I have two bools _leftSwinging and _rightSwinging,
        // this method is handy and can be generalized for other script behaviors
        // that rely on swinging, essentially, I am adding this here to future
        // proof if we decide to add bats and such
        public bool isSwinging(bool isLeft)
        {
            if(isLeft)
            {
                return _leftSwinging;
            }
            else
            {
                return _rightSwinging;
            }
        }

        // Collision function that is called from the child arm objects
        // please add collision logic here, for now I just make objects
        // with the tag breakable be able to break, currently
        // there is no separate logic for left arm, but we can add it if necessary
        public void handleHit(Collider other, bool isLeftArm)
        {
            // just exits function early if the arm isn't swinging
            // and happens to collide with a breakable object
            if (!isSwinging(isLeftArm)) return;
            Debug.Log("Hit: " + other.gameObject.name);
            if(other.gameObject.CompareTag("Breakable"))
            {
                Destroy(other.gameObject);
            }
        }
        // public accessors
        public float CurrentSpeed => getHorizontalSpeed();
        public bool isGrounded => playerController != null && playerController.Grounded;
    }
}
