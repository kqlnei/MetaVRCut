using UnityEngine;

public class OrbitObject : MonoBehaviour
{
    public Transform player;
    public float radius = 5f; // 半円の半径
    public float speed = 1f; // 回転速度

    private float angle = 0f;

    void Update()
    {
        if (player == null) return;

        angle += speed * Time.deltaTime;

        // 角度を0〜360度の範囲で循環
        angle = Mathf.Repeat(angle, 360f);

        // 半円（0〜180度）または全円（0〜360度）の軌道を計算
        float radian = Mathf.Deg2Rad * angle;

        // 半円の座標を計算
        float x = player.position.x + Mathf.Cos(radian) * radius;
        float z = player.position.z + Mathf.Sin(radian) * radius;

        // オブジェクトの位置を更新 (高さはプレイヤーに合わせる)
        transform.position = new Vector3(x, player.position.y, z);

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // 水平方向のみを向く
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }
}