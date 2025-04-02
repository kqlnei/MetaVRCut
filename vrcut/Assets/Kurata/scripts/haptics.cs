using System.Collections;
using UnityEngine;

public class Haptics : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Haptics Settings")]
    public float defaultDuration = 0.5f; // �U������
    public float defaultFrequency = 1f;  // ���g��
    public float defaultAmplitude = 1f;  // �U��

    private OVRHapticsClip hapticsClipRight;
    private OVRHapticsClip hapticsClipLeft;

    private void Start()
    {
        // AudioSource�̃N���b�v���n�v�e�B�N�X�p�̃N���b�v�ɕϊ�
        if (audioSource != null && audioSource.clip != null)
        {
            hapticsClipRight = new OVRHapticsClip(audioSource.clip);
            hapticsClipLeft = new OVRHapticsClip(audioSource.clip);
        }
        else
        {
            Debug.LogWarning("AudioSource�܂���AudioClip���ݒ肳��ĂȂ�����U�����ʉ����Đ�����Ȃ���");
        }
    }

    /// <summary>
    /// �w�肵���R���g���[���[�̐U�����g���K�[
    /// </summary>
    public void TriggerHaptics(OVRInput.Controller controller, float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        // �������w�肳��Ă��Ȃ��ꍇ�̓f�t�H���g
        duration = (duration < 0) ? defaultDuration : duration;
        frequency = (frequency < 0) ? defaultFrequency : frequency;
        amplitude = (amplitude < 0) ? defaultAmplitude : amplitude;

        // �n�v�e�B�N�X�N���b�v�̑I��
        OVRHapticsClip clip = (controller == OVRInput.Controller.RTouch) ? hapticsClipRight : hapticsClipLeft;

        // �U���J�n
        StartCoroutine(VibrateController(controller, duration, frequency, amplitude));

        // �n�v�e�B�N�X�N���b�v���Đ�
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
    /// �E��̐U��
    /// </summary>
    public void TriggerHapticsRight(float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        TriggerHaptics(OVRInput.Controller.RTouch, duration, frequency, amplitude);
    }

    /// <summary>
    /// ����̐U��
    /// </summary>
    public void TriggerHapticsLeft(float duration = -1f, float frequency = -1f, float amplitude = -1f)
    {
        TriggerHaptics(OVRInput.Controller.LTouch, duration, frequency, amplitude);
    }

    /// <summary>
    /// �w�肵���R���g���[���[�ŐU��
    /// </summary>
    private IEnumerator VibrateController(OVRInput.Controller controller, float duration, float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
}