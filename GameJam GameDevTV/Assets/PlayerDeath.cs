using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy") && !player.isGhost)
        {
            player.Die();
            LevelManager.instance.Respawn();
        }
    }
}