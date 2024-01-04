using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using BitStreams;

namespace SpherePacketVisualEditor;

public class PacketDefinition
{
    public List<PacketPart> PacketParts = new ();
    public string Name { get; set; }
    public string FilePath { get; set; }

    public void LoadFromFile ()
    {
        var contents = File.ReadAllLines(FilePath);
        var parts = new List<PacketPart>();

        foreach (var line in contents)
        {
            var fieldValues = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);

            if (fieldValues.Length < 8)
            {
                Console.WriteLine($"Missing fields in packet definition {Name}, line: {line}");
            }

            var partName = fieldValues[0];
            if (partName == PacketPart.UndefinedPacketPartName)
            {
                // skip undef
                continue;
            }

            var packetPartType = Enum.TryParse(fieldValues[1], out PacketPartType partType)
                ? partType
                : PacketPartType.BITS;
            var start = int.Parse(fieldValues[2]);
            var length = int.Parse(fieldValues[3]);
            var r = byte.Parse(fieldValues[4]);
            var g = byte.Parse(fieldValues[5]);
            var b = byte.Parse(fieldValues[6]);
            var a = byte.Parse(fieldValues[7]);
            var color = new Color
            {
                A = a,
                R = r,
                G = g,
                B = b
            };

            var startPosition = new StreamPosition(start / 8, start % 8);
            var end = start + length;
            var endPosition = new StreamPosition(end / 8, end % 8);

            var highlightColor = new SolidColorBrush
            {
                Color = color
            };

            var part = new PacketPart(length, highlightColor, partName, packetPartType, startPosition, endPosition,
                new List<Bit>());
            parts.Add(part);
        }

        PacketParts = parts;
    }
}