using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostFence : MonoBehaviour
{
    public bool fadeSprite = true;

    private void Update()
    {
        if (LevelManager.instance.player.isGhost)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            if (fadeSprite)
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        }
        else
        {
            GetComponent<BoxCollider2D>().enabled = true;
            if (fadeSprite)
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }
}