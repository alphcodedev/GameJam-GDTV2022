using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WinLevel : MonoBehaviour
{
    public Light2D doorLight;
    private float timer = 0f;
    private bool inPosition = false;
    
    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inPosition = true;
            timer = 0f;
            doorLight.intensity = 0;
            while (timer < 1 && inPosition)
            {
                timer += Time.deltaTime;
                var value = Mathf.Lerp(0, 3, timer);
                doorLight.intensity = value;
                yield return null;
            }

            if(inPosition)
                LevelManager.instance.WinLevel();
        }
    }

    private IEnumerator OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inPosition = false;
            while (timer > 0 && !inPosition)
            {
                timer -= Time.deltaTime;
                var value = Mathf.Lerp(0, 3, timer);
                doorLight.intensity = value;
                yield return null;
            }
            
            timer = 0f;
            // doorLight.intensity = 0;
        }
    }
}
