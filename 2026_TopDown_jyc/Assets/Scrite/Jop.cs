using UnityEngine;

[CreateAssetMenu(fileName = "NewJob", menuName = "GameData/JobData")]
public class JobData : ScriptableObject
{
    public string jobName;
    public int price;
    public float moveSpeed;
    public Sprite jobSprite;

    // ★ 추가: 이 직업이 사용할 애니메이션 교체 세트
    public AnimatorOverrideController jobAnimatorOverride;
}