using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{

    public void SetTarget(Transform target)
    {
        GetComponent<CinemachineCamera>().Follow = target;
    }
}
