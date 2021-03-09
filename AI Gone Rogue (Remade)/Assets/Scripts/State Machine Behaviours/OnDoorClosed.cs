using UnityEngine;

public class OnDoorClosed : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Door doorInstance = animator.GetComponentInParent<Door>();
        doorInstance.DoorMoving = true;
        doorInstance.IsOpen = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Door doorInstance = animator.GetComponentInParent<Door>();
        doorInstance.DoorMoving = false;
        doorInstance.ChangeLockIndicationMaterial(doorInstance.autoOpenDoorMaterial, 1);
    }
}
