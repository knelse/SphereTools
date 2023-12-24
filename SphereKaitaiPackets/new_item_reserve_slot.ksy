meta:
  id: new_item_reserve_slot
  file-extension: new_item_reserve_slot
  endian: le
  bit-endian: le
seq:
  - id: header
    type: server_header
  - id: skip_1
    type: b9
  - id: slot_id
    type: u1
    enum: item_slot
  - id: new_item_id
    type: u2
  - id: zero_byte
    type: u1
  - id: count
    type: u1
types:
  server_header:
    seq:
      - id: length
        type: u2
      - id: ok_marker
        type: u2
      - id: sync_1
        type: b24
      - id: entity_id
        type: u2
      - id: packet_type
        type: b24
enums:
  item_slot:
    0: helmet
    1: amulet
    2: shield
    3: chest
    4: gloves
    5: belt
    6: bracelet_left
    7: bracelet_right
    8: ring_1
    9: ring_2
    10: ring_3
    11: ring_4
    12: pants
    13: boots
    14: guild
    15: map_book
    16: recipe_book
    17: mantra_book
    18: unknown
    19: unknown
    20: inkpot
    21: unknown
    22: unknown
    23: unknown
    24: unknown
    25: unknown
    26: inventory_1
    27: inventory_2
    28: inventory_3
    29: inventory_4
    30: inventory_5
    31: inventory_6
    32: inventory_7
    33: inventory_8
    34: inventory_9
    35: inventory_10
    36: mutator_1
    37: mutator_2
    38: mutator_3
    39: mutator_4
    40: mutator_5
    41: mutator_6
    42: mutator_7
    43: mutator_8
    44: mutator_9
    45: mutator_10