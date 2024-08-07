meta:
  id: mainhand_equip_powder
  file-extension: mainhand_equip_powder
  endian: le
  bit-endian: le
seq:
  - id: header
    type: client_header
  - id: skip_13_bit
    type: b12
  - id: equip_item_id
    type: b16
  - id: mainhand_state
    type: b11
    enum: slot_state
types:
  packet_length:
    seq:
      - id: length
        type: u2
  client_header:
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