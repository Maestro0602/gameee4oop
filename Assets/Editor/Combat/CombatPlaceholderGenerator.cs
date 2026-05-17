//using UnityEditor;
//using UnityEditor;
//using UnityEngine;

//public static class CombatPlaceholderGenerator
//{
//    private const string FolderPath = "Assets/Data/Combat/Attacks";

//    [MenuItem("Tools/Combat/Generate Placeholder Attack Set")]
//    public static void GeneratePlaceholderAttackSet()
//    {
//        EnsureFolder("Assets/Data");
//        EnsureFolder("Assets/Data/Combat");
//        EnsureFolder(FolderPath);

//        AttackDefinition2D basicSlash = CreateOrUpdateAttack(
//            "BasicSlash",
//            "basic_slash",
//            AttackKind2D.Ground,
//            0.04f, 0.08f, 0.10f,
//            0.02f, 0.08f, 0.20f,
//            new Vector2(1.4f, 0f),
//            new Vector2(0.9f, 0.55f), new Vector2(1.0f, 0.8f),
//            1f,
//            false, 0f,
//            0, 10f
//        );

//        AttackDefinition2D thrust = CreateOrUpdateAttack(
//            "Thrust",
//            "thrust",
//            AttackKind2D.Ground,
//            0.08f, 0.10f, 0.16f,
//            0.06f, 0.12f, 0.20f,
//            new Vector2(2.6f, 0f),
//            new Vector2(1.2f, 0.55f), new Vector2(1.2f, 0.8f),
//            2f,
//            false, 0f,
//            0, 12f
//        );

//        AttackDefinition2D downwardStrike = CreateOrUpdateAttack(
//            "DownwardStrike",
//            "downward_strike",
//            AttackKind2D.Downward,
//            0.05f, 0.10f, 0.12f,
//            0.04f, 0.08f, 0.24f,
//            new Vector2(0.4f, -1.8f),
//            new Vector2(0f, -0.9f), new Vector2(0.9f, 1.0f),
//            1.2f,
//            true, 12f,
//            0, 12f
//        );

//        AttackDefinition2D spin = CreateOrUpdateAttack(
//            "SpinAttack",
//            "spin_attack",
//            AttackKind2D.Special,
//            0.10f, 0.14f, 0.20f,
//            0.08f, 0.16f, 0.25f,
//            Vector2.zero,
//            Vector2.zero, new Vector2(1.8f, 1.8f),
//            2.5f,
//            false, 0f,
//            40, 0f
//        );

//        PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
//        if (player != null)
//        {
//            SerializedObject so = new SerializedObject(player);
//            so.FindProperty("groundCombo").arraySize = 2;
//            so.FindProperty("groundCombo").GetArrayElementAtIndex(0).objectReferenceValue = basicSlash;
//            so.FindProperty("groundCombo").GetArrayElementAtIndex(1).objectReferenceValue = thrust;

//            so.FindProperty("airCombo").arraySize = 1;
//            so.FindProperty("airCombo").GetArrayElementAtIndex(0).objectReferenceValue = downwardStrike;

//            so.FindProperty("specialAttack").objectReferenceValue = spin;
//            so.ApplyModifiedPropertiesWithoutUndo();
//            EditorUtility.SetDirty(player);
//        }

//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//        Debug.Log("Placeholder attacks generated in Assets/Data/Combat/Attacks and assigned to PlayerController2D if found.");
//    }

//    private static AttackDefinition2D CreateOrUpdateAttack(
//        string assetName,
//        string attackId,
//        AttackKind2D kind,
//        float startup,
//        float active,
//        float recovery,
//        float cancelStart,
//        float cancelEnd,
//        float comboWindow,
//        Vector2 velocity,
//        Vector2 hitboxOffset,
//        Vector2 hitboxSize,
//        float damage,
//        bool bounceOnHit,
//        float bounceVelocity,
//        int resourceCost,
//        float resourceGain)
//    {
//        string path = $"{FolderPath}/{assetName}.asset";
//        AttackDefinition2D attack = AssetDatabase.LoadAssetAtPath<AttackDefinition2D>(path);

//        if (attack == null)
//        {
//            attack = ScriptableObject.CreateInstance<AttackDefinition2D>();
//            AssetDatabase.CreateAsset(attack, path);
//        }

//        attack.attackId = attackId;
//        attack.attackKind = kind;
//        attack.startupTime = startup;
//        attack.activeTime = active;
//        attack.recoveryTime = recovery;
//        attack.cancelWindowStart = cancelStart;
//        attack.cancelWindowEnd = cancelEnd;
//        attack.comboWindow = comboWindow;
//        attack.velocityEffect = velocity;
//        attack.hitboxOffset = hitboxOffset;
//        attack.hitboxSize = hitboxSize;
//        attack.damage = damage;
//        attack.bounceOnHit = bounceOnHit;
//        attack.bounceVelocity = bounceVelocity;
//        attack.resourceCost = resourceCost;
//        attack.resourceGainOnHit = resourceGain;

//        EditorUtility.SetDirty(attack);
//        return attack;
//    }

//    private static void EnsureFolder(string path)
//    {
//        if (AssetDatabase.IsValidFolder(path))
//        {
//            return;
//        }

//        int slash = path.LastIndexOf('/');
//        string parent = path.Substring(0, slash);
//        string leaf = path.Substring(slash + 1);

//        if (!AssetDatabase.IsValidFolder(parent))
//        {
//            EnsureFolder(parent);
//        }

//        AssetDatabase.CreateFolder(parent, leaf);
//    }
//}
