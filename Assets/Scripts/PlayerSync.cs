using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    [HideInInspector]
    public string id;

    public void ChangePosition(Vector3 position) {
        transform.position = position;
    }

    public void SetId(string newId) {
        id = newId;
    }
}
