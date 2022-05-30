using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGravity : MonoBehaviour
{
   private Rigidbody2D _rb;
   private PlayerController _player;

   private GravityState gravityCurrentState = GravityState.Down;

   private readonly Vector2 k_DownGravity = new(0f, -9.81f);
   private readonly Vector2 k_UpGravity = new(0f, 9.81f);
   private readonly Vector2 k_LeftGravity = new(-9.81f, 0f);
   private readonly Vector2 k_RightGravity = new(9.81f, 0f);
   
   private void Start()
   {
      _rb = GetComponent<Rigidbody2D>();
      _player = GetComponent<PlayerController>();
   }

   private void Update()
   {
      if (gravityCurrentState != GravityState.Up && Input.GetKeyDown(KeyCode.I))
      {
         Physics2D.gravity = k_UpGravity;
         transform.eulerAngles = new Vector3(0, 0, 180);
         // _player.FacingRight = !_player.FacingRight;
         gravityCurrentState = GravityState.Up;
         
         _player.VerticalMovement = false;
      }
      else if (gravityCurrentState != GravityState.Down && Input.GetKeyDown(KeyCode.K))
      {
         Physics2D.gravity = k_DownGravity;
         transform.eulerAngles = Vector3.zero;
         // _player.FacingRight = !_player.FacingRight;
         gravityCurrentState = GravityState.Down;
         
         _player.VerticalMovement = false;
      }
      else if (gravityCurrentState != GravityState.Left && Input.GetKeyDown(KeyCode.J))
      {
         Physics2D.gravity = k_LeftGravity;
         transform.eulerAngles = new Vector3(0, 0, -90);
         gravityCurrentState = GravityState.Left;

         _player.VerticalMovement = true;
      }
      else if (gravityCurrentState != GravityState.Right && Input.GetKeyDown(KeyCode.L))
      {
         Physics2D.gravity = k_RightGravity;
         transform.eulerAngles = new Vector3(0, 0, 90);
         gravityCurrentState = GravityState.Right;
         
         _player.VerticalMovement = true;
      }
      
   }
}

public enum GravityState{Up,Down,Left,Right}
