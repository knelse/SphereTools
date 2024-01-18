using System;
using System.Collections.Generic;
using System.Linq;
using BitStreams;
using LiteDB;
using SphereHelpers.Extensions;
using SpherePacketVisualEditor;
using SphServer.Helpers;

namespace PacketLogViewer.Models.PacketAnalyzeData;

public class PacketAnalyzeData
{
    public int Id { get; set; }
    public ObjectType ObjectType { get; set; }
    [BsonIgnore] protected readonly List<PacketPart> Parts;
    public string DisplayValue => $"{Id:X4} ({Enum.GetName(ObjectType) ?? string.Empty})";

    public PacketAnalyzeData ()
    {
    }

    public PacketAnalyzeData (List<PacketPart> parts)
    {
        Parts = parts;
        Id = GetIntValue(PacketPartNames.ID);

        var objectTypePart = Parts.FirstOrDefault(x =>
            x.Name == PacketPartNames.EntityType || x.Name == PacketPartNames.ObjectType);
        if (objectTypePart is not null)
        {
            var objTypeVal = (ushort) BitStreamExtensions.BitsToInt(objectTypePart.Value);
            ObjectType = Enum.IsDefined(typeof (ObjectType), objTypeVal) ? (ObjectType) objTypeVal : ObjectType.Unknown;
        }
    }

    public int GetIntValue (string name)
    {
        var part = Parts.FirstOrDefault(x => x.Name == name);
        return part is not null ? BitStreamExtensions.BitsToInt(part.Value) : 0;
    }

    public double GetClientCoordValue (string name)
    {
        var part = Parts.FirstOrDefault(x => x.Name == name);
        if (part is not null)
        {
            return CoordsHelper.DecodeClientCoordinateWithoutShift(BitStream.BitArrayToBytes(part.Value.ToArray()),
                false);
        }

        return 0;
    }

    public string GetStringValue (string name)
    {
        var part = Parts.FirstOrDefault(x => x.Name == name);
        return part is not null
            ? PacketLogViewerMainWindow.Win1251.GetString(BitStream.BitArrayToBytes(part.Value.ToArray()))
            : string.Empty;
    }
}