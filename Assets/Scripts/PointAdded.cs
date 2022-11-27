using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAdded : MonoBehaviour
{

    private float pointLifeTime = 1f;
    private float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, pointLifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 old_position = gameObject.transform.position;
        gameObject.transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
    }
}
