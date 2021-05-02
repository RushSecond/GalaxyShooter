using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 8f;

    void Update()
    {
        // move up by my speed every second
        transform.Translate(Vector3.up * _mySpeed * Time.deltaTime);

        // if the laser y position is greater than 5.5f, destroy it
        // and destroy parent too if it has one
        if (transform.position.y > 5.5f)
        {
            Transform myParent = transform.parent;
            if (transform.parent)
            {
                Destroy(myParent.gameObject);
            }
            Destroy(gameObject);
        }
    }
}
