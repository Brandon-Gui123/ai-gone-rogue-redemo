using UnityEngine;

public class OnDoorOpened : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<Door>().DoorMoving = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Door doorInstance = animator.GetComponentInParent<Door>();
        doorInstance.DoorMoving = false;
        doorInstance.IsOpen = true;
    }
}