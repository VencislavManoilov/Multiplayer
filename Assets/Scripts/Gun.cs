using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    
    
    private bool mousePressed = false;

    void Update() {
        if(Input.GetMouseButtonDown(0) && !mousePressed) {
            Shoot();
            mousePressed = true;
        }

        if(Input.GetMouseButtonUp(0)) {
            mousePressed = false;
        }
    }

    private void Shoot() {
        Transform mainCameraTrans = Camera.main.gameObject.transform;

        Ray ray = new Ray(mainCameraTrans.position, mainCameraTrans.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Enemy"))) {
            PlayerSync enemy = hit.collider.GetComponent<PlayerSync>();

            if (enemy != null) {
                enemy.GetDemaged(10);
            }
        }
    }
}
