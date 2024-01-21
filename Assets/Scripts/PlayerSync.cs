using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    [HideInInspector]
    public string id;

    public void ChangePosition(Vector3 position, Vector3 rotation) {
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, rotation.y, 0);
    }

    public void SetId(string newId) {
        id = newId;
    }
}
