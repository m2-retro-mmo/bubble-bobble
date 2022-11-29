using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public float followSpeed = 2f;
    public Transform target;
    public int z = -10;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 newPos = new Vector3(target.position.x, target.position.y, z);
            // transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
            transform.position = newPos;
        }
    }
}
