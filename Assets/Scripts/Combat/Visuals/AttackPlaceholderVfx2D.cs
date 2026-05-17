//using System.Collections.Generic;
//using UnityEngine;

//[DisallowMultipleComponent]
//public class AttackPlaceholderVfx2D : MonoBehaviour
//{
//    [SerializeField] private PlayerController2D player;
//    [SerializeField] private bool showAttackShape = true;
//    [SerializeField] private float durationMultiplier = 1f;
//    [SerializeField] private float sizePadding = 0.15f;
//    [SerializeField] private int sortingOrder = 420;
//    [SerializeField] private Color groundColor = new Color(1f, 0.35f, 0.35f, 0.45f);
//    [SerializeField] private Color airColor = new Color(0.35f, 0.9f, 1f, 0.45f);
//    [SerializeField] private Color downwardColor = new Color(1f, 0.95f, 0.3f, 0.45f);
//    [SerializeField] private Color specialColor = new Color(0.9f, 0.4f, 1f, 0.45f);

//    private class VfxItem
//    {
//        public Transform Transform;
//        public SpriteRenderer Renderer;
//        public float Duration;
//        public float Time;
//        public Color BaseColor;
//        public Vector3 InitialScale;
//    }

//    private static Sprite cachedSquareSprite;
//    private readonly List<VfxItem> activeItems = new List<VfxItem>();

//    private void Awake()
//    {
//        if (player == null)
//        {
//            player = GetComponent<PlayerController2D>();
//        }
//    }

//    private void Update()
//    {
//        for (int i = activeItems.Count - 1; i >= 0; i--)
//        {
//            VfxItem item = activeItems[i];
//            if (item == null || item.Renderer == null || item.Transform == null)
//            {
//                activeItems.RemoveAt(i);
//                continue;
//            }

//            item.Time += Time.deltaTime;
//            float t = item.Duration <= 0f ? 1f : Mathf.Clamp01(item.Time / item.Duration);

//            Color c = item.BaseColor;
//            c.a = item.BaseColor.a * (1f - t);
//            item.Renderer.color = c;

//            float pulse = Mathf.Lerp(1f, 1.08f, t);
//            item.Transform.localScale = item.InitialScale * pulse;

//            if (item.Time >= item.Duration)
//            {
//                Destroy(item.Transform.gameObject);
//                activeItems.RemoveAt(i);
//            }
//        }
//    }

//    public void PlayAttackShape(AttackDefinition2D attack, Vector2 origin, int facing)
//    {
//        if (!showAttackShape || attack == null)
//        {
//            return;
//        }

//        GameObject go = new GameObject("AttackShapeVFX");

//        Vector2 center = origin + new Vector2(attack.hitboxOffset.x * facing, attack.hitboxOffset.y);
//        go.transform.position = new Vector3(center.x, center.y, transform.position.z);

//        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
//        sr.sprite = GetSquareSprite();
//        sr.sortingOrder = sortingOrder;
//        sr.color = GetColor(attack.attackKind);

//        Vector2 size = attack.hitboxSize + Vector2.one * sizePadding;
//        go.transform.localScale = new Vector3(size.x, size.y, 1f);

//        float duration = Mathf.Max(0.05f, (attack.startupTime + attack.activeTime) * Mathf.Max(0.1f, durationMultiplier));
//        activeItems.Add(new VfxItem
//        {
//            Transform = go.transform,
//            Renderer = sr,
//            Duration = duration,
//            Time = 0f,
//            BaseColor = sr.color,
//            InitialScale = go.transform.localScale
//        });
//    }

//    private Color GetColor(AttackKind2D kind)
//    {
//        switch (kind)
//        {
//            case AttackKind2D.Air:
//                return airColor;
//            case AttackKind2D.Downward:
//                return downwardColor;
//            case AttackKind2D.Special:
//                return specialColor;
//            default:
//                return groundColor;
//        }
//    }

//    private static Sprite GetSquareSprite()
//    {
//        if (cachedSquareSprite != null)
//        {
//            return cachedSquareSprite;
//        }

//        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
//        texture.SetPixel(0, 0, Color.white);
//        texture.Apply();

//        cachedSquareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
//        return cachedSquareSprite;
//    }
//}
