using UnityEngine;

public class VolumeSliders : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void mainVolume(float volume)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MainVolume", volume, true);
    }

    public void musicVolume(float volume)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MusicVolume", volume, true);
    }

    public void sfxVolume(float volume)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SFXVolume", volume, true);
    }
}
