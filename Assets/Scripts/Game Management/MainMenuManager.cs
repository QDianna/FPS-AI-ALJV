using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public void PlayGame() 
	{
		SceneLoader.Instance.LoadGameWithFade();
	}
}
