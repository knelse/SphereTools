using System;
using System.Collections.Generic;
using System.Linq;
using SpherePacketVisualEditor;

namespace PacketLogViewer.Models.PacketAnalyzeData;

public class ItemPacket : PacketAnalyzeData
{
    public EntityActionType ActionType { get; set; } = EntityActionType.UNDEF;
    public bool HasGameId { get; set; }
    public int GameObjectId { get; set; }
    public int ContainerId { get; set; }
    public int Count { get; set; } = 1;
    public ItemSuffix ItemSuffix { get; set; } = ItemSuffix.None;

    public readonly SphGameObject? GameObject;

    public readonly string OverrideType = string.Empty;

    public override string DisplayValue => GetDisplayValue();

    public ItemPacket (List<PacketPart> parts) : base(parts)
    {
        var actionTypePart = Parts.FirstOrDefault(x => x.Name == PacketPartNames.ActionType);
        if (actionTypePart is not null)
        {
            var actionTypeVal = (int) (actionTypePart.ActualLongValue ?? int.MaxValue);
            ActionType = Enum.IsDefined(typeof (EntityActionType), actionTypeVal)
                ? (EntityActionType) actionTypeVal
                : EntityActionType.UNDEF;
        }

        if (ActionType is EntityActionType.FULL_SPAWN or EntityActionType.FULL_SPAWN_2)
        {
            HasGameId = GetBitValue(PacketPartNames.HasGameId);
            GameObjectId = GetIntValue(PacketPartNames.GameObjectId);
            ContainerId = GetIntValue(PacketPartNames.ContainerId);
            Count = Math.Max(GetIntValue(PacketPartNames.Count), 1);
            if (HasGameId)
            {
                GameObject = SphObjectDb.GameObjectDataDb[GameObjectId];
            }

            var subtypeId = GetIntValue(PacketPartNames.SubtypeId);

            if (ObjectType is ObjectType.ScrollLegend or ObjectType.ScrollRecipe)
            {
                var scrollName = $"scroll{subtypeId:000}";
                if (SphObjectDb.LocalisationContent.ContainsKey(scrollName))
                {
                    var localized = SphObjectDb.LocalisationContent[scrollName][Locale.Russian];
                    if (localized.Length > 0)
                    {
                        OverrideType = localized[0][3..];
                    }
                }
            }
            else if (ObjectType is ObjectType.Key or ObjectType.KeyBarn)
            {
                var keyLocales = SphObjectDb.LocalisationContent["st_key"][Locale.Russian];
                var subtypeStr = $"{subtypeId}";
                var text = keyLocales.FirstOrDefault(x => x.StartsWith(subtypeStr));
                if (!string.IsNullOrEmpty(text))
                {
                    OverrideType = text[(subtypeStr.Length + 1)..];
                }
            }
        }
    }

    private string GetDisplayValue ()
    {
        var questItemPrefix = ObjectPacketTools.IsQuestItem(ObjectType) ? "Квест " : string.Empty;

        var typeName = $"({Enum.GetName(ObjectType)!})";
        string tier;
        var displayName =
            HasGameId
                ? SphObjectDb.GameObjectDataDb[GameObjectId].Localisation[Locale.Russian]
                : string.IsNullOrEmpty(OverrideType)
                    ? ObjectPacketTools.GetFriendlyNameByObjectType(ObjectType)
                    : OverrideType;

        if (GameObject?.ObjectType == GameObjectType.Ring)
        {
            tier = GameObject.TitleMinusOne > 0
                ? $"{GameObject.TitleMinusOne + 1}т"
                : GameObject.DegreeMinusOne > 0
                    ? $"{GameObject.DegreeMinusOne + 1}с"
                    : GameObject.ToRomanTierLiteral();
        }
        else
        {
            tier = GameObject?.ToRomanTierLiteral() ?? string.Empty;
        }

        var suffix = (GameObject?.Suffix ?? ItemSuffix.None) == ItemSuffix.None
            ? string.Empty
            : $" {GameObjectDataHelper.ObjectTypeToSuffixLocaleMap[GameObject!.ObjectType][GameObject!.Suffix]
                .localization[Locale.Russian]}";

        var count = Count > 1 ? $" ({Count})" : string.Empty;
        var name = $"{questItemPrefix}{displayName}" + suffix +
                   (string.IsNullOrEmpty(tier) ? tier : $" {tier}") + count;
        return $"{name,-44}ID: {Id:X4}  GMID: {GameObjectId.ToString(),5}  " +
               $"Type: {(int) ObjectType,4} {typeName,-24} Suff: N/A  Bag: {ContainerId:X4}";
    }
}