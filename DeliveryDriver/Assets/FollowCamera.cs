using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] GameObject thingToFollow;

    //카메라의 위치는 차의 위치와 같아야 한다.
    void LateUpdate()
    {
        if (thingToFollow != null)
        {
            transform.position = thingToFollow.transform.position + new Vector3(0, 0, -10);
        }
    }
}
