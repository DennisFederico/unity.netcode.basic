using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Dennis.Unity.Utils.Singletons;
using UnityEngine;

public class PlayerCameraFollow : Singleton<PlayerCameraFollow>
{
    [SerializeField]
    private float perlinAmplitudeGain = 0.5f;

    [SerializeField]
    private float perlinFrequencyGain = 0.5f;
    private CinemachineVirtualCamera vCamera;

    private void Awake() {
        vCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void AttachTo(Transform transform) {
        vCamera.Follow = transform;
        var perlin = vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = perlinAmplitudeGain;
        perlin.m_FrequencyGain = perlinFrequencyGain;
    }

}
