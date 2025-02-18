using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using vrm;
using FMOD.Studio;
using FMOD;
using FMODUnity;

public class MenuController : MonoBehaviour
{
    [Header("Volume Setting")]
    [SerializeField] private TMP_Text _volumeTextValue = null;
    [SerializeField] private Slider _volumeSlider = null;
    [SerializeField] private GameObject _confirmationPrompt = null;
    [SerializeField] private float _defaultVolume = 1.0f;


    private float _volumeValue;
    private EventInstance _backgroundMusic;
    [Header("Scene to load")]
    public string NewGameLevel;

    public void Awake()
    {
        if (!Methods.IsSceneLoaded("Shared"))
            SceneManager.LoadScene("Shared", LoadSceneMode.Additive);

    }

    public void Start()
    {
        _volumeValue = AudioManager.Instance.MasterVolume;
        _backgroundMusic = AudioManager.Instance.CreateInstance(FMODEvents.Instance.BackgroundUI);
        _backgroundMusic.start();
    }
    public void NewGameDialogSi()
    {
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene(NewGameLevel, LoadSceneMode.Additive);
    }

    public void SceneTestDialogSi()
    {
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioManager.Instance.MasterVolume = volume;
        _volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        _volumeValue = AudioManager.Instance.MasterVolume;
        StartCoroutine(ConfirmationBox());
    }

    public void AudioSettingBack()
    {
        AudioManager.Instance.MasterVolume = _volumeValue;
        _volumeSlider.value = _volumeValue;
        _volumeTextValue.text = _volumeValue.ToString("0.0");
    }

    public void ResetButton(string menuType)
    {
        if (menuType == "Audio")
        {
            AudioManager.Instance.MasterVolume = _defaultVolume;
            _volumeTextValue.text = _defaultVolume.ToString("0.0");
            _volumeSlider.value = _defaultVolume;
        }
    }

    public void ClickSound()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ClickUI);
    }

    public IEnumerator ConfirmationBox()
    {
        _confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        _confirmationPrompt.SetActive(false);
    }
    public void OnDestroy()
    {
        _backgroundMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
