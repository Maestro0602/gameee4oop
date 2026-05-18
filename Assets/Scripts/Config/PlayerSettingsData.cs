using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Settings Data", fileName = "PlayerSettingsData")]
public class PlayerSettingsData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 100f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.8f, 0.2f);
    [SerializeField] private Vector2 attackSize = new Vector2(1.1f, 0.7f);

    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float AttackDamage => attackDamage;
    public Vector2 AttackOffset => attackOffset;
    public Vector2 AttackSize => attackSize;
}
