using UnityEngine;

public class Treecutting : MonoBehaviour
{
    [SerializeField] private Transform hammerSprite;
    [SerializeField] private Transform idlePose;
    [SerializeField] private Transform swingPose;

    public void SetHammerState(KeyCode key)
    {
        bool swing = key == KeyCode.Q;

        hammerSprite.SetPositionAndRotation(
            swing ? swingPose.position : idlePose.position,
            swing ? swingPose.rotation : idlePose.rotation
            );
    }
}
