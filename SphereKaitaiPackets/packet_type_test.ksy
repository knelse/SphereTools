meta:
  id: packet_type_test
  file-extension: packet_type_test
  endian: le
  bit-endian: le
seq:
  - id: header
    type: server_header
  - id: skip
    size: 19
  - id: skip_1
    type: b6
  - id: target_id
    type: b16
  - id: skip_3
    type: b2
  - id: skip_4
    size: 22
  - id: mutator_id
    type: u2
  - id: skip_2
    size: 3
  - id: from_id
    type: u2
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
      - id: packet_type_1
        type: u1
      - id: packet_type_2
        type: b6
      - id: packet_type_3
        type: u1
      - id: packet_type_4
        type: b2