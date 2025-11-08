// SpellCard.cs
using UnityEngine;

/// <summary>
/// 符卡資料類別。定義一輪攻擊的基本參數。
/// </summary>
[System.Serializable]
public class SpellCard
{
    [Tooltip("符卡名稱")]
    public string cardName = "Spell";
    
    [Tooltip("符卡總持續時間（秒）")]
    public float duration = 5f;
}