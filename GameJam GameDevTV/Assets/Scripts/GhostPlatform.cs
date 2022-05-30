using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPlatform : MonoBehaviour
{
   public Sprite normalPlatformSprite;
   public Sprite ghostPlatformSprite;

   private void Update()
   {
      if (LevelManager.instance.player.isGhost)
      {
         GetComponent<SpriteRenderer>().sprite = normalPlatformSprite;
         GetComponent<BoxCollider2D>().enabled = true;
      }
      else
      {
         GetComponent<SpriteRenderer>().sprite = ghostPlatformSprite;
         GetComponent<BoxCollider2D>().enabled = false;
      
      }
   }
}
