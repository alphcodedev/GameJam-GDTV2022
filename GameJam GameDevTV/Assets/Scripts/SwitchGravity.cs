using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchGravity : MonoBehaviour
{
   private Rigidbody2D _rb;

   private void Start()
   {
      _rb = GetComponent<Rigidbody2D>();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.I))
      {
         Physics2D.gravity = new Vector2(0f, 9.81f);
         transform.eulerAngles = new Vector3(0, 0, 180);
      }
      if (Input.GetKeyDown(KeyCode.K))
      {
         Physics2D.gravity = new Vector2(0f, -9.81f);
         transform.eulerAngles = Vector3.zero;

      }
      if (Input.GetKeyDown(KeyCode.J))
      {
         Physics2D.gravity = new Vector2(-9.81f, 0f);
         transform.eulerAngles = new Vector3(0, 0, -90);

      }
      if (Input.GetKeyDown(KeyCode.L))
      {
         Physics2D.gravity = new Vector2(9.81f, 0f);
         transform.eulerAngles = new Vector3(0, 0, 90);

      }
      
   }
}
