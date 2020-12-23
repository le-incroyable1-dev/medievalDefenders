using System.Collections;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    public AudioSource game_audio;
    public GameObject WinStruct;
    public GameObject LoseStruct;


    // Use this for initialization
    void Start()
    {
        game_audio = GetComponent<AudioSource>();
        SetAudio();
        //LoseStruct.SetActive(false);
        //WinStruct.SetActive(false);
        DontDestroyOnLoad(gameObject);
    
    }

    public void OnWin()
    {
        LoseStruct.SetActive(false);
        WinStruct.SetActive(true);
    }

    public void OnLose()
    {
        WinStruct.SetActive(false);
        LoseStruct.SetActive(true);
    } 

    public void SetAudio()
    {
        game_audio.volume = 1;
    }


}
