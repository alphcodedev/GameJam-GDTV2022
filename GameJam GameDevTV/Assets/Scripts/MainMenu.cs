
using UnityEngine;

public class MainMenu : MonoBehaviour
{
   public SceneFader fader;

   private void Start()
   {
      AudioManager.instance.PlaySound("MainMenuTheme");
   }
      
   public void Play()
   {
      fader.FadeTo("LevelSelect");
   }

   public void Quit()
   {
      Debug.Log("Quit");
      Application.Quit();
   }
   
   
}
