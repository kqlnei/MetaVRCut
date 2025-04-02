using System.Collections;
using UnityEngine;

public class Haptics : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Haptics Settings")]
    public float defaultDuration = 0.5f; // 振動時間
    public float defaultFrequency = 1f;  // 周波数
    public float defaultAmplitude = 1f;  // 振幅

    private OVRHapticsClip hapticsClipRight;
    private OVRHapticsClip hapticsClipLeft;

    private void Start()
    {
        // AudioSourceのクリップをハプティクス用のクリップに変換
        if (audioSource != null && audioSource.clip != null)
        {
            hapticsClipRight = new OVRHapticsClip(audioSource.clip);
            hapticsClipLeft = new OVRHapticsClip(audioSource.clip);
        }
        else
        {
            Debug.LogWarning("AudioSourceまたはAudioClipが設定されてないから振動効果音が再生されないお");
        }
    }

    /// <summary>
    /// 指定したコントローラーの振動をトリガー
    /// </summary>
    public void TriggerHaptics(OVRInput.Controller controller, float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        // 引数が指定されていない場合はデフォルト
        duration = (duration < 0) ? defaultDuration : duration;
        frequency = (frequency < 0) ? defaultFrequency : frequency;
        amplitude = (amplitude < 0) ? defaultAmplitude : amplitude;

        // ハプティクスクリップの選択
        OVRHapticsClip clip = (controller == OVRInput.Controller.RTouch) ? hapticsClipRight : hapticsClipLeft;

        // 振動開始
        StartCoroutine(VibrateController(controller, duration, frequency, amplitude));

        // ハプティクスクリップを再生
        if (clip != null)
        {
            if (controller == OVRInput.Controller.RTouch)
                OVRHaptics.RightChannel.Mix(clip);
            else if (controller == OVRInput.Controller.LTouch)
                OVRHaptics.LeftChannel.Mix(clip);
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// 右手の振動
    /// </summary>
    public void TriggerHapticsRight(float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        TriggerHaptics(OVRInput.Controller.RTouch, duration, frequency, amplitude);
    }

    /// <summary>
    /// 左手の振動
    /// </summary>
    public void TriggerHapticsLeft(float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        TriggerHaptics(OVRInput.Controller.LTouch, duration, frequency, amplitude);
    }

    /// <summary>
    /// 指定したコントローラーで振動
    /// </summary>
    private IEnumerator VibrateController(OVRInput.Controller controller, float duration, float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
}