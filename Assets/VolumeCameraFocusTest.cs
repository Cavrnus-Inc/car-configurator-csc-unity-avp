using Unity.PolySpatial;
using UnityEngine;

public class VolumeCameraFocusTest : MonoBehaviour
{
    [SerializeField] private VolumeCamera volumeCam;

    private bool m_Focused;

    private void Awake()
    {
        volumeCam.OnWindowEvent.AddListener(OnWindowEvent);
    }

    private void OnDestroy()
    {
        volumeCam.OnWindowEvent.RemoveListener(OnWindowEvent);
    }

    private void OnWindowEvent(VolumeCamera.WindowState windowState)
    {
        if (m_Focused == windowState.IsFocused)
            return;
        
        if ((m_Focused = windowState.IsFocused))
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = Microphone.Start("", true, 10, 44100);
            audioSource.Play();
        }
        else
        {
            Microphone.End("");
        }
    }
}
