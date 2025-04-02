using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int maxHits = 3;
    [SerializeField] private List<string> damageTags = new List<string> { "Damage" };
    [SerializeField] private List<string> damageLayers = new List<string> { "Bullet" };

    [Header("Instant Death Settings")]
    [SerializeField] private string instantDeathTag = "LastBoss";
    [SerializeField] private AudioClip instantDeathSound;

    [Header("Visual Feedback")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float fadeDuration = 2.0f; // オーバーレイのフェードアウトにかかる時間

    [Header("Audio Settings")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioSource audioSource;

    [Header("UI Settings")]
    [SerializeField] private Text hitsText;

    private int currentHits = 0;
    private List<int> damageLayerIDs = new List<int>();
    private Coroutine fadeCoroutine;

    private void Start()
    {
        InitializeDamageLayers();
        InitializeDamageOverlay();
        UpdateHitsUI();
    }

    /// <summary>
    /// damageLayersリストに基づき、各レイヤーのIDを取得して初期化
    /// </summary>
    private void InitializeDamageLayers()
    {
        damageLayerIDs.Clear();
        foreach (string layerName in damageLayers)
        {
            int layerID = LayerMask.NameToLayer(layerName);
            if (layerID == -1)
            {
                Debug.LogError($"レイヤー '{layerName}' が見つかりません！");
            }
            else
            {
                damageLayerIDs.Add(layerID);
            }
        }
    }

    /// <summary>
    /// ダメージオーバーレイの初期状態（完全透明）を設定
    /// </summary>
    private void InitializeDamageOverlay()
    {
        if (damageOverlay != null)
        {
            Color color = damageOverlay.color;
            color.a = 0f;
            damageOverlay.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // 即死ダメージ判定
        if (other.CompareTag(instantDeathTag))
        {
            Debug.Log("[GameOverManager] 即死ダメージ発生！");
            currentHits = maxHits;
            PlayInstantDeathSound();
            UpdateHitsUI();
            UpdateDamageOverlay();
            GameOver();
            return;
        }

        // ダメージタグまたはダメージレイヤーに該当する場合の処理
        bool isDamage = damageTags.Contains(other.tag) || damageLayerIDs.Contains(other.gameObject.layer);
        if (isDamage)
        {
            currentHits++;
            Debug.Log($"[GameOverManager] 現在のヒット数: {currentHits}");
            PlayDamageSound();
            UpdateHitsUI();
            UpdateDamageOverlay();

            if (currentHits >= maxHits)
            {
                GameOver();
            }
        }
    }

    /// <summary>
    /// ダメージオーバーレイのアルファ値を即時50%に設定し、フェードアウトを開始
    /// </summary>
    private void UpdateDamageOverlay()
    {
        if (damageOverlay == null)
        {
            Debug.LogWarning("Damage Overlayが設定されていません。");
            return;
        }

        // 既存のフェードコルーチンがあれば停止
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // オーバーレイのアルファを50%に設定
        Color color = damageOverlay.color;
        color.a = 0.5f;
        damageOverlay.color = color;

        // フェードアウトコルーチン開始
        fadeCoroutine = StartCoroutine(FadeOverlay());
    }

    /// <summary>
    /// ダメージオーバーレイを指定時間かけてフェードアウト
    /// </summary>
    private IEnumerator FadeOverlay()
    {
        float timer = 0f;
        Color initialColor = damageOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, timer / fadeDuration);
            Color newColor = initialColor;
            newColor.a = alpha;
            damageOverlay.color = newColor;
            yield return null;
        }

        // 最終的に完全に透明に設定
        Color finalColor = damageOverlay.color;
        finalColor.a = 0f;
        damageOverlay.color = finalColor;
    }

    /// <summary>
    /// ダメージを受けた際のサウンド
    /// </summary>
    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        else
        {
            Debug.LogWarning("AudioSourceまたはダメージ音が設定されていません。");
        }
    }

    /// <summary>
    /// 即死ダメージ時のサウンド
    /// </summary>
    private void PlayInstantDeathSound()
    {
        if (audioSource != null && instantDeathSound != null)
        {
            audioSource.PlayOneShot(instantDeathSound);
        }
        else
        {
            Debug.LogWarning("AudioSourceまたは即死音が設定されていません。");
        }
    }

    /// <summary>
    /// UI上にヒット数の更新を反映
    /// </summary>
    private void UpdateHitsUI()
    {
        if (hitsText != null)
        {
            hitsText.text = $"Hits: {currentHits} / {maxHits}";
        }
        else
        {
            Debug.LogWarning("ヒット数表示用UI Textが設定されていません。");
        }
    }

    /// <summary>
    /// オーバーシーンへ遷移
    /// </summary>
    private void GameOver()
    {
        Debug.Log("[GameOverManager] ゲームオーバー発生");
        SceneManager.LoadScene("Over");
    }
}