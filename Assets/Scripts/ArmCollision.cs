using UnityEngine;

namespace StarterAssets
{
    public class ArmCollision : MonoBehaviour
    {
        // this will call the HandleHit method in the ArmsHolder's
        // attached script, collisions are only detected on the game
        // object themselves, and not the parent, so we need to
        // create this script to conform to Unity, the Left arm will
        // obviously have this bool value set to true, collision
        // handling can thusly be found in the ArmMovement.cs file
        public ArmMovement armMovement;
        public bool isLeftArm;

        void OnTriggerEnter(Collider other)
        {
            armMovement.handleHit(other, isLeftArm);
        }
        // handles the case where the hitbox is already isnide fo the other
        // object, because onTriggerEnter only happens on contact
        void OnTriggerStay(Collider other)
        {
            armMovement.handleHit(other, isLeftArm);
        }
    }
}