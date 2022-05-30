using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExitDoor : MonoBehaviour
{
    public Light2D doorLight;
    private float timer = 0f;
    private bool inPosition = false;
    private SpriteRenderer _sprite;
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _sprite.sprite = LevelManager.instance.keysNeeded == 0 ? openDoorSprite : closedDoorSprite;
    }

    public void OpenDoor()
    {
        _sprite.sprite = openDoorSprite;
        AudioManager.instance.PlaySound("OpenDoor");
    }
    
    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && LevelManager.instance.keysNeeded == 0)
        {
            AudioManager.instance.PlaySound("LevelPassed");
            
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
        if (other.gameObject.CompareTag("Player") && LevelManager.instance.keysNeeded == 0)
        {
            AudioManager.instance.StopSound("LevelPassed");

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
