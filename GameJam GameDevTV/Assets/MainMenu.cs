using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
   public SceneFader fader; 
      
      
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
