using System.Collections;
using CavrnusSdk.API;
using CavrnusSdk.Setup;
using Unity.PolySpatial;
using UnityEngine;

namespace CavrnusSdk.AppleVisionPro
{
    /*
     * This script is the way around the microphone RTC issue where
     * audio is not transmitted to clients. As it stands, this is an issue w/
     * Polyspatial MixedReality (bounded/unbounded) projects. Virtual Reality mode works fine.
     */
    
    [RequireComponent(typeof(AudioSource))]
    public class CavrnusAppleVisionProSpaceJoin : MonoBehaviour
    {
        [Header("Cavrnus Info")]
        [SerializeField] private string spaceJoinId;
        
        [Header("Polyspatial Components")]
        [SerializeField] private VolumeCamera vc;
        
        private bool isFocused;
        private CavrnusSpaceConnection spaceConnection;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            
            var connector = FindObjectOfType<CavrnusSpatialConnector>();
            if (connector != null)
                connector.SpaceJoinMethod = CavrnusSpatialConnector.SpaceJoinOption.Custom;
        }

        private void Start()
        {
            vc.OnWindowEvent.AddListener(ws =>
            {
                if (isFocused == ws.IsFocused)
                    return;

                if ((isFocused = ws.IsFocused))
                    StartCoroutine(InitializeMicrophone());
                else
                    StopMicrophone();
            });
        }
        
        private IEnumerator InitializeMicrophone()
        {
            yield return new WaitUntil(() => Microphone.devices.Length > 0);
    
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
                    CavrnusFunctionLibrary.JoinSpace(spaceJoinId, connection =>
                    {
                        spaceConnection = connection;
                        spaceConnection.FetchAudioInputs(list =>
                        {
                            foreach (var device in list)
                                Debug.Log($"CAVRNUS_DEBUG: Microphone device found: {device.Name}");
                
                            spaceConnection.UpdateAudioInput(list[0]); // there should only be one (iPhoneInput)
                            Debug.Log($"CAVRNUS_DEBUG: Microphone started successfully after {Time.time - startTime} seconds.");
                        });
                    }, s =>
                    {
                        
                    });
             
                    yield break;
                }
            }

            Debug.LogError("CAVRNUS_DEBUG: Failed to start microphone after maximum retries.");
        }
             
        private void StopMicrophone()
        {
            Microphone.End(null);
            spaceConnection?.ExitSpace();
        }
    }
}