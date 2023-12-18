var sampleOutput = @"
equip_item_id: 64336
header:
  client_id: 21112
  length:
    length: 30
  ok_marker:
    ok_01: 1
    ok_2c: 44
  packet_type: 4407304
  sync_1: 935398249
  sync_2: 163328
mainhand_state: 7
skip_1: 2571542916
skip_2: 124654742
";

var sampleInputBytes =
    Convert.FromHexString(
        "1E00B80B972C2C01008A27785208404384A1469996146E07B57F00000000A697A91F8A99913B0DAFCAF11698794503D85F4A91B1A9AFF26B9E8B90E8E2D920D53F99428B00");

var sampleInput = @"
meta:
  id: mainhand_reequip_powder_powder
  file-extension: mainhand_reequip_powder_powder
  endian: le
  bit-endian: le
seq:
  - id: header
    type: header_type
  - id: skip_1
    type: b32
  - id: skip_2
    type: b28
  - id: equip_item_id
    type: b16
  - id: mainhand_state
    type: b12
    enum: slot_state
types:
  packet_length:
    seq:
      - id: length
        type: u2
  header_type:
    seq:
      - id: length
        type: packet_length
      - id: sync_1
        type: u4
      - id: ok_marker
        type: ok_marker
      - id: sync_2
        type: b24
      - id: client_id
        type: u2
      - id: packet_type
        type: b24
  ok_marker:
    seq:
      - id: ok_2c
        type: u1
      - id: ok_01
        type: u1
enums:
  slot_state:
    0: full
    255: empty
    22: fists
";
var parser = new SimpleKaitaiParser.SimpleKaitaiParser();
var script = parser.ParseKaitaiScript(sampleInput);
foreach (var entry in script)
{
    Console.WriteLine(entry);
}

var result = parser.ParseByteArray(sampleInput, sampleInputBytes);
foreach (var entry in result)
{
    Console.WriteLine(entry);
}