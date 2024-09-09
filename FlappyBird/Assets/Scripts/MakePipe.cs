using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakePipe : MonoBehaviour
{
    [SerializeField] GameObject pipe;
    [SerializeField] float timeDiff = 1.0f;
    float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > timeDiff)
        {
            GameObject newpipe = Instantiate(pipe);
            newpipe.transform.position = new Vector3 (4.5f, Random.Range(-2.0f, 4.0f), 0f);
            timer = 0f;
            Destroy(newpipe, 5.0f);
        }
    }
}
