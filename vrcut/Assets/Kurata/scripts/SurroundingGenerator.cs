using UnityEngine;
using System.Collections.Generic;

public class SurroundingGenerator : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPattern
    {
        public RotationType rotationType;
        public GameObject[] prefabs;
        public Vector3 basePosition;
        public Vector3 interval;
    }

    public enum RotationType
    {
        RightWall,
        LeftWall,
        Ceiling,
        Floor
    }

    [Header("生成設定")]
    [SerializeField] private SpawnPattern[] spawnPatterns;
    [SerializeField] private int visibleSectionCount = 5;

    private Transform player;
    private Queue<GameObject>[] generatedSections;
    private float nextSpawnZ = 0f;
    private Transform sectionsParent;

    [Header("削除設定")]
    [SerializeField] private float removalDelayMultiplier = 1.2f;
    [SerializeField] private int bufferSections = 2;

    private float sectionIntervalZ = 0f;

    private void Start()
    {
        if (spawnPatterns == null || spawnPatterns.Length == 0)
        {
            Debug.LogError("SpawnPatternsが設定されていません。");
            enabled = false;
            return;
        }
        // patterns[0]のZ方向間隔をキャッシュ
        sectionIntervalZ = spawnPatterns[0].interval.z;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Playerタグが付いたオブジェクトが見つかりません。");
            enabled = false;
            return;
        }
        player = playerObj.transform;

        CreateParentObject();
        InitializeSections();
        PrewarmSections();
    }

    /// <summary>
    /// 生成したセクションを格納する親オブジェクトを生成
    /// </summary>
    void CreateParentObject()
    {
        sectionsParent = new GameObject("GeneratedSections").transform;
    }

    /// <summary>
    /// 各SpawnPatternごとにセクションを管理するQueueを初期化
    /// </summary>
    void InitializeSections()
    {
        generatedSections = new Queue<GameObject>[spawnPatterns.Length];
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            generatedSections[i] = new Queue<GameObject>();
        }
    }

    /// <summary>
    /// 初期状態で見える範囲のセクションを先に生成
    /// </summary>
    void PrewarmSections()
    {
        for (int i = 0; i < visibleSectionCount; i++)
        {
            GenerateSections(nextSpawnZ);
            nextSpawnZ += sectionIntervalZ;
        }
    }

    private void Update()
    {
        float removalThreshold = visibleSectionCount * sectionIntervalZ * removalDelayMultiplier;
        if (player.position.z > nextSpawnZ - removalThreshold)
        {
            GenerateSections(nextSpawnZ);
            RemoveOldSections();
            nextSpawnZ += sectionIntervalZ;
        }
    }

    /// <summary>
    /// 指定位置に各パターンのセクションを生成
    /// </summary>
    /// <param name="zPosition">生成位置のZ座標</param>
    void GenerateSections(float zPosition)
    {
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            SpawnPattern pattern = spawnPatterns[i];
            if (pattern.prefabs == null || pattern.prefabs.Length == 0)
                continue;

            GameObject selectedPrefab = pattern.prefabs[Random.Range(0, pattern.prefabs.Length)];
            Vector3 spawnPos = CalculatePosition(pattern, zPosition);
            Quaternion rotation = GetPatternRotation(pattern.rotationType);

            GameObject section = Instantiate(selectedPrefab, spawnPos, rotation, sectionsParent);
            generatedSections[i].Enqueue(section);
        }
    }

    /// <summary>
    /// 指定のパターンに基づき、生成位置を計算
    /// </summary>
    Vector3 CalculatePosition(SpawnPattern pattern, float z)
    {
        return new Vector3(
            pattern.basePosition.x,
            pattern.basePosition.y,
            z + pattern.basePosition.z
        );
    }

    /// <summary>
    /// 指定されたRotationTypeに合わせた回転を返す
    /// </summary>
    Quaternion GetPatternRotation(RotationType type)
    {
        // ランダムな回転のオフセットを付与（0または180度）
        int random180 = Random.Range(0, 2) * 180;
        return type switch
        {
            RotationType.RightWall => Quaternion.Euler(random180, 0, 0),
            RotationType.LeftWall => Quaternion.Euler(random180, 180, 0),
            RotationType.Ceiling => Quaternion.Euler(0, random180, 90),
            RotationType.Floor => Quaternion.Euler(0, random180, -90),
            _ => Quaternion.identity,
        };
    }

    /// <summary>
    /// プレイヤーから離れた古いセクションを削除
    /// </summary>
    void RemoveOldSections()
    {
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            int keepCount = visibleSectionCount + bufferSections;
            while (generatedSections[i].Count > keepCount)
            {
                GameObject oldSection = generatedSections[i].Dequeue();
                if (oldSection != null)
                {
                    Destroy(oldSection);
                }
            }
        }
    }
}