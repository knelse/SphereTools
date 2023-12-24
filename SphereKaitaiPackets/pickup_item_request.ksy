meta:
  id: pickup_item_request_0x16
  file-extension: pickup_item_request_0x16
  endian: be
  bit-endian: le
seq:
  - id: header
    type: client_header
  - id: skip_13_bit
    type: b13
  - id: item_id
    type: b16
  - id: zeroes
    type: b19
types:
  client_header:
    seq:
      - id: length
        type: u2
      - id: sync_1
        type: u4
      - id: ok_marker
        type: u2
      - id: sync_2
        type: b24
      - id: client_id
        type: u2
      - id: packet_type
        type: b24
