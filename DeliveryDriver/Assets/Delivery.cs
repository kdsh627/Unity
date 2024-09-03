using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery : MonoBehaviour
{
    [SerializeField] Color32 hasPackageColor = new Color32(1, 1, 1, 1);
    [SerializeField] Color32 noPackageColor = new Color32(1, 1, 1, 1);
    [SerializeField] float destroyDelay = 0.5f; 
    //bool���� false�� ����Ʈ
    bool hasPackage = false;

    SpriteRenderer spriteRenderer;

    private void Start()
    {
        //��������Ʈ �������� ������
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Package" && !hasPackage)
        {
            Debug.Log("Package Pick up");
            Destroy(other.gameObject, destroyDelay);
            hasPackage = true;
            spriteRenderer.color = hasPackageColor;
        }

        if(other.tag == "Customer" && hasPackage)
        {
            Debug.Log("Package Delivery");
            hasPackage = false;
            spriteRenderer.color = noPackageColor;
        }
    }
}
