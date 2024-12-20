using System.Collections;
using CavrnusSdk.API;
using Unity.PolySpatial;
using UnityEngine;

namespace CavrnusDemo
{
    [RequireComponent(typeof(AudioSource))]
    public class CavrnusMicrophoneTest : MonoBehaviour
    {
        [SerializeField] private VolumeCamera vc;
        
        private AudioSource audioSource;
        private CavrnusSpaceConnection spaceConnection;

        private bool isFocused;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc =>
            {
                spaceConnection = sc;
                // StartCoroutine(InitializeMicrophone());
                      
                vc.OnWindowEvent.AddListener(ws =>
                {
                    if (isFocused == ws.IsFocused)
                        return;

                    if ((isFocused = ws.IsFocused))
                    {
                        Debug.Log($"CAVRNUS_DEBUG: Starting Microphone");
                        StartCoroutine(InitializeMicrophone());
                    }
                    else
                    {
                        Debug.Log($"CAVRNUS_DEBUG: Stopping Microphone");
                        StopMicrophone();
                    }
                });
            });
        }

        private IEnumerator InitializeMicrophone()
        {
            StopMicrophone();
    
            var startTime = Time.time;

            // Check for any microphones
            yield return new WaitUntil(() => Microphone.devices.Length > 0);
    
            Debug.Log($"CAVRNUS_DEBUG: Time to detect microphones: {Time.time - startTime} seconds");
            Debug.Log($"CAVRNUS_DEBUG: # Microphones found {Microphone.devices.Length}");

            StartCoroutine(TryStartMicrophone());
        }
        
        private IEnumerator TryStartMicrophone()
        {
            var retryCount = 0;
            const int maxRetries = 5;

            while (retryCount < maxRetries)
            {
                var startTime = Time.time;

                audioSource.clip = Microphone.Start(null, true, 10, 44100);
                while (!(Microphone.GetPosition(null) > 0))
                {
                    if (Time.time - startTime > 5f) // Timeout for this attempt
                    {
                        Debug.LogWarning($"CAVRNUS_DEBUG: Microphone start timed out. Retrying ({retryCount + 1}/{maxRetries})...");
                        retryCount++;
                        break;
                    }
                    yield return null;
                }

                if (Microphone.IsRecording(null))
                {
                    spaceConnection.FetchAudioInputs(list =>
                    {
                        foreach (var device in list)
                            Debug.Log($"CAVRNUS_DEBUG: Microphone device found: {device.Name}");
                
                        spaceConnection.UpdateAudioInput(list[0]); // guessing here...
                        audioSource.Play();
                        Debug.Log($"CAVRNUS_DEBUG: Microphone started successfully after {Time.time - startTime} seconds.");
                    });
                    
                    yield break;
                }
            }

            Debug.LogError("CAVRNUS_DEBUG: Failed to start microphone after maximum retries.");
        }


        private void StopMicrophone()
        {
            Microphone.End(null);
        }
    }
}