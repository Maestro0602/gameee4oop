using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public class CombatHitboxViewer2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController2D player;

    [Header("Toggle")]
    [SerializeField] private bool showHitboxes = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.H;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key toggleKeyInputSystem = Key.H;
#endif

    [Header("Visual")]
    [SerializeField] private bool disablePlayerRuntimeHitbox = true;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private Color groundColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color airColor = new Color(0.35f, 0.9f, 1f, 1f);
    [SerializeField] private Color downwardColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] private Color specialColor = new Color(0.9f, 0.4f, 1f, 1f);

    private readonly List<LineRenderer> renderers = new List<LineRenderer>();
    private bool cachedPlayerRuntimeHitbox;

    public bool ShowHitboxes
    {
        get => showHitboxes;
        set
        {
            showHitboxes = value;
            ApplyVisibility();
        }
    }

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController2D>();
        }

        if (player != null && disablePlayerRuntimeHitbox)
        {
            cachedPlayerRuntimeHitbox = player.ShowRuntimeHitbox;
            player.ShowRuntimeHitbox = false;
        }
    }

    private void OnDestroy()
    {
        if (player != null && disablePlayerRuntimeHitbox)
        {
            player.ShowRuntimeHitbox = cachedPlayerRuntimeHitbox;
        }
    }

    private void Update()
    {
        if (ReadTogglePressedThisFrame())
        {
            showHitboxes = !showHitboxes;
            ApplyVisibility();
        }
    }

    private bool ReadTogglePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current[toggleKeyInputSystem].wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(toggleKey);
#else
        return false;
#endif
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        List<AttackDefinition2D> attacks = CollectAttacks();
        EnsureRendererCount(attacks.Count);

        if (!showHitboxes)
        {
            DisableAll();
            return;
        }

        int facing = player.FacingDirection;
        Vector2 origin = player.AttackOriginPosition;

        for (int i = 0; i < attacks.Count; i++)
        {
            AttackDefinition2D attack = attacks[i];
            if (attack == null)
            {
                renderers[i].enabled = false;
                continue;
            }

            LineRenderer lr = renderers[i];
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            Color c = GetColorForAttack(attack.attackKind);
            lr.startColor = c;
            lr.endColor = c;

            Vector2 center = origin + new Vector2(attack.hitboxOffset.x * facing, attack.hitboxOffset.y);
            Vector2 half = attack.hitboxSize * 0.5f;

            Vector3 bottomLeft = new Vector3(center.x - half.x, center.y - half.y, transform.position.z);
            Vector3 topLeft = new Vector3(center.x - half.x, center.y + half.y, transform.position.z);
            Vector3 topRight = new Vector3(center.x + half.x, center.y + half.y, transform.position.z);
            Vector3 bottomRight = new Vector3(center.x + half.x, center.y - half.y, transform.position.z);

            lr.SetPosition(0, bottomLeft);
            lr.SetPosition(1, topLeft);
            lr.SetPosition(2, topRight);
            lr.SetPosition(3, bottomRight);
            lr.SetPosition(4, bottomLeft);
            lr.enabled = true;
        }

        for (int i = attacks.Count; i < renderers.Count; i++)
        {
            renderers[i].enabled = false;
        }
    }

    private List<AttackDefinition2D> CollectAttacks()
    {
        List<AttackDefinition2D> attacks = new List<AttackDefinition2D>();

        AddRangeUnique(attacks, player.GroundCombo);
        AddRangeUnique(attacks, player.AirCombo);

        if (player.SpecialAttack != null && !attacks.Contains(player.SpecialAttack))
        {
            attacks.Add(player.SpecialAttack);
        }

        return attacks;
    }

    private static void AddRangeUnique(List<AttackDefinition2D> list, AttackDefinition2D[] values)
    {
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            AttackDefinition2D value = values[i];
            if (value == null || value.attackKind == AttackKind2D.Downward || list.Contains(value))
            {
                continue;
            }

            list.Add(value);
        }
    }

    private void EnsureRendererCount(int count)
    {
        while (renderers.Count < count)
        {
            GameObject go = new GameObject($"HitboxViewer_{renderers.Count}");
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.positionCount = 5;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lr.material = new Material(shader);
            }
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.sortingOrder = 450;
            lr.enabled = false;
            renderers.Add(lr);
        }
    }

    private void DisableAll()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].enabled = false;
        }
    }

    private void ApplyVisibility()
    {
        if (showHitboxes)
        {
            return;
        }

        DisableAll();
    }

    private Color GetColorForAttack(AttackKind2D kind)
    {
        switch (kind)
        {
            case AttackKind2D.Air:
                return airColor;
            case AttackKind2D.Downward:
                return downwardColor;
            case AttackKind2D.Special:
                return specialColor;
            default:
                return groundColor;
        }
    }
}
