using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] GameObject thingToFollow;

    //ī�޶��� ��ġ�� ���� ��ġ�� ���ƾ� �Ѵ�.
    void LateUpdate()
    {
        if (thingToFollow != null)
        {
            transform.position = thingToFollow.transform.position + new Vector3(0, 0, -10);
        }
    }
}
