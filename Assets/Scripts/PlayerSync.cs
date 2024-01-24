using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSync : MonoBehaviour
{
    public Transform gunHolder;

    [HideInInspector]
    public string id;

    [HideInInspector]
    public int health = 100;

    public Canvas canvas;
    public TextMeshProUGUI healthText;

    private server server;

    void Start() {
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;

        healthText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        healthText.text = health.ToString();

        server = FindFirstObjectByType<server>();
    }

    void Update() {
        canvas.gameObject.transform.LookAt(Camera.main.gameObject.transform);
    }

    public void ChangePosition(Vector3 position, Vector3 rotation) {
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, rotation.y, 0);
        gunHolder.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }

    public void SetHealth(int newHealth) {
        health = newHealth;
        healthText.text = health.ToString();
    }

    public void SetId(string newId) {
        id = newId;
    }

    public void GetDemaged(int demage) {
        SetHealth(health - demage);

        server.SendDemage(id, health);
    }
}
