meta:
  id: new_entity_inkpot_with_header
  file-extension: new_entity_inkpot_with_header
  endian: le
  bit-endian: le
seq:
# inkpot
  - id: length
    type: u2
    comment: --- INKPOT ---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
# next, small backpack
  - id: entity_id
    type: u2
    comment: --- SMALL BACKPACK ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b61
    type: b61
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
# next, sack 1
  - id: entity_id
    type: u2
    comment: --- SACK 1 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b7
# next, mapbook
  - id: entity_id
    type: u2
    comment: --- MAPBOOK ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b61
    type: b61
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b6 #might be 7 or 8, we'll see
#next, token 1 (has packet header here)
  - id: length
    type: u2
    comment: --- TOKEN 1 ---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b34
    type: b38
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, token 2
  - id: entity_id
    type: u2
    comment: --- TOKEN 2 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b34
    type: b38
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, token 3
  - id: entity_id
    type: u2
    comment: --- TOKEN 3 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b34
    type: b38
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, token 4
  - id: entity_id
    type: u2
    comment: --- TOKEN 4 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b34
    type: b38
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, map Hyperion (has packet header here)
  - id: length
    type: u2
    comment: --- MAP HYPERION ---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Shipstone
  - id: entity_id
    type: u2
    comment: --- MAP SHIPSTONE ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Hyperion NW
  - id: entity_id
    type: u2
    comment: --- MAP HYPERION NW ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Hyperion W
  - id: entity_id
    type: u2
    comment: --- MAP HYPERION W ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Hyperion SW
  - id: entity_id
    type: u2
    comment: --- MAP HYPERION SW ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b5 #new packet here
#next, map Hyperion SE(has packet header here)
  - id: length
    type: u2
    comment: --- MAP HYPERION SE---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Hyperion E
  - id: entity_id
    type: u2
    comment: --- MAP HYPERION E ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Shipstone
  - id: entity_id
    type: u2
    comment: --- MAP SHIPSTONE ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Torweal
  - id: entity_id
    type: u2
    comment: --- MAP TORWEAL ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, map Bangville
  - id: entity_id
    type: u2
    comment: --- MAP BANGVILLE ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b5 #new packet here
#next, map Sunpool SE(has packet header here)
  - id: length
    type: u2
    comment: --- MAP SUNPOOL---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b137
  - id: has_game_id
    type: b1
  - id: game_id
    type: b14
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
#next, earstring 1
  - id: entity_id
    type: u2
    comment: --- EARSTRING 1 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types    
  - id: unknown
    type: b136
  - id: has_game_id
    type: b2
  - id: game_id
    type: b7
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b7
    type: b7
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, earstring 2
  - id: entity_id
    type: u2
    comment: --- EARSTRING 2 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types    
  - id: unknown
    type: b136
  - id: has_game_id
    type: b2
  - id: game_id
    type: b7
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b7
    type: b7
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, earstring 3
  - id: entity_id
    type: u2
    comment: --- EARSTRING 3 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types    
  - id: unknown
    type: b136
  - id: has_game_id
    type: b2
  - id: game_id
    type: b7
    enum: localizable
  - id: skip_b29
    type: b29
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b7
    type: b7
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, earstring 4
  - id: entity_id
    type: u2
    comment: --- EARSTRING 4 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types    
  - id: unknown
    type: b136
  - id: has_game_id
    type: b2
  - id: game_id
    type: b7
    enum: localizable
  - id: skip_b7
    type: b7
  # packet split happened here
  - id: length
    type: u2
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: skip_b25
    type: b25
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b30
    type: b30
  - id: unknown_FFFFFFFE
    type: u4
  - id: skip2
    type: u3
  - id: count
    type: u2
  - id: skip_b7
    type: b7
  - id: skip_00_00
    type: u2
  - id: split
    type: b8
#next, large backpack
  - id: entity_id
    type: u2
    comment: --- LARGE BACKPACK ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types    
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b61
    type: b61
  - id: unknown_FFFFFFFE
    type: u4
  - id: split
    type: b8
# next, sack 2
  - id: entity_id
    type: u2
    comment: --- SACK 2 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b7
# next, sack 3
  - id: entity_id
    type: u2
    comment: --- SACK 3 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b7
# next, sack 4(has packet header here)
  - id: length
    type: u2
    comment: --- SACK 4 ---
  - id: skip
    type: u5
  - id: entity_id
    type: u2
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b7
# next, sack 5
  - id: entity_id
    type: u2
    comment: --- SACK 5 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b7
# next, sack 6
  - id: entity_id
    type: u2
    comment: --- SACK 6 ---
  - id: skip_b2
    type: b2
  - id: object_type
    type: b10
    enum: object_types
  - id: unknown
    type: b192
  - id: has_game_id
    type: b1
  - id: skip_4
    type: u4
  - id: container_id
    type: u2
  - id: skip_1
    type: u1
  - id: skip_b46
    type: b46
  - id: icon_name_length
    type: u1
  - id: icon_name
    type: str
    size: icon_name_length
  - id: split
    type: b1 # new packet here

    
enums:
  object_types:
    4: player
    8: token
    30: mutator
    40: seed_castle
    47: xp_pill_degree
    66: token_multiuse
    68: trade_license
    90: scroll_legend
    91: scroll_recipe
    95: mission
    104: token_island
    105: token_island_guest
    205: npc_quest_title
    209: npc_quest_degree
    210: monster
    213: npc_trade
    225: npc_banker
    236: bead
    400: backpack_large
    401: backpack_small
    405: sack
    406: chest
    407: sack_mob_loot
    409: mantrabook_small
    410: recipe_book
    411: mantrabook_large
    412: mantrabook_great
    413: mapbook
    418: key_barn
    451: powder_finale
    453: powder_single_target
    454: powder_amilus
    455: powder_aoe
    471: elixir_casle
    472: elixir_trap
    500: weapon_sword
    501: weapon_axe
    502: weapon_crossbow
    503: arrows
    551: ring_diamond
    552: ring_ruby
    553: ruby
    555: ring_gold
    600: alchemy_mineral
    601: alchemy_plant
    602: alchemy_metal
    650: food_apple
    651: food_pear
    652: food_meat
    653: food_bread
    655: food_fish
    700: alchemy_brushwood
    701: key
    703: map
    704: inkpot
    705: firecracker
    706: ear
    708: ear_string
    709: monster_part
    712: firework
    715: inkpot_broken
    750: armor_chest
    751: armor_amulet
    752: armor_boots
    754: armor_gloves
    755: armor_belt
    756: armor_shield
    757: armor_helmet
    758: armor_pants
    759: armor_bracelet
    760: ring
    761: armor_robe
    762: ring_golem
    800: alchemy_pot
    804: blueprint
    949: quest_armor_chest
    950: quest_armor_amulet
    952: quest_armor_boots
    953: quest_armor_gloves
    954: quest_armor_belt
    955: quest_armor_shield
    956: quest_armor_helmet
    957: quest_armor_pants
    958: quest_armor_bracelet
    959: quest_armor_ring
    960: quest_armor_robe
    961: quest_weapon_sword
    962: quest_weapon_axe
    963: quest_weapon_crossbow
    976: guild
    977: guild_ability
    979: guild_ability_steal
    990: armor_helmet_premium
    1000: mantra_white
    1001: mantra_black
  localizable:
    1: Кривой меч
    2: Сабля
    3: Палаш
    4: Скимитар
    5: Ятаган
    6: Катана
    7: Шамшер
    8: Элитная сабля
    9: Элитный палаш
    10: Элитный ятаган
    11: Элитная катана
    12: Руническая сабля
    13: Рунический палаш
    14: Рунический ятаган
    15: Руническая катана
    16: Полуторный меч
    17: Двуручный меч
    18: Клеймор
    19: Эспадон
    20: Фламберг
    21: Великий меч
    22: Гигантский меч
    23: Элитный двуручный меч
    24: Элитный клеймор
    25: Элитный фламберг
    26: Элитный великий меч
    27: Рунический двуручный меч
    28: Рунический клеймор
    29: Рунический фламберг
    30: Рунический великий меч
    31: Короткий меч
    32: Широкий меч
    33: Гладиус
    34: Пехотный меч
    35: Длинный меч
    36: Кристальный меч
    37: Золотой меч
    38: Элитный короткий меч
    39: Элитный гладиус
    40: Элитный длинный меч
    41: Элитный кристальный меч
    42: Рунический короткий меч
    43: Рунический гладиус
    44: Рунический длинный меч
    45: Рунический кристальный меч
    50: Медаль
    60: Лотерейный билет (класс 1)
    61: Лотерейный билет (класс 2)
    62: Лотерейный билет (класс 3)
    63: Лотерейный билет (класс 4)
    64: Лотерейный билет (класс 5)
    90: Красная нитка для ушей
    91: Зелёная нитка для ушей
    92: Синяя нитка для ушей
    93: Желтая нитка для ушей
    101: Топор
    102: Большой топор
    103: Боевой топор
    104: Пехотный топор
    105: Франциска
    106: Секира
    107: Табар
    108: Элитный большой топор
    109: Элитный боевой топор
    110: Элитная франциска
    111: Элитная секира
    112: Рунический большой топор
    113: Рунический боевой топор
    114: Руническая франциска
    115: Руническая секира
    116: Дубинка
    117: Булава
    118: Палица
    119: Молот
    120: Боевой молот
    121: Моргенштерн
    122: Кузнечный молот
    123: Элитная булава
    124: Элитная палица
    125: Элитный боевой молот
    126: Элитный моргенштерн
    127: Руническая булава
    128: Руническая палица
    129: Рунический боевой молот
    130: Меч Дракона
    140: Ухо псоглавца
    141: Кость летуна
    142: Усики сколопендры
    143: Череп лича
    144: Клык вепря
    145: Коготь цианоса
    146: Хвост волка
    147: Лапы паука
    148: Хвост саламандры
    149: Жало скорпиона
    150: Чешуя тифона
    151: Зуб дракона
    152: Бивень мамонта
    153: Ухо нетопыря
    154: Бусинка
    155: Вода
    160: Посылка
    161: Письмо
    162: Записка
    163: Конверт
    164: Документы
    165: Меч
    166: Отравленная рыба
    167: Рисунок
    168: Рецепт
    169: Кровь монстра
    170: Свиток
    171: Голова
    172: Голова монстра
    173: Книга
    174: Книга
    175: Череп
    176: Глаз
    177: Архивы
    178: Формула
    179: UC
    180: Гриб
    181: Эликсир
    184: Перо
    186: Алмаз
    187: Рубин
    188: Изумруд
    189: Камень
    190: Формула
    191: Вино
    192: Яд
    193: Кольцо
    195: Старый ключ
    196: Сердце дракона
    197: Кровь дракона
    201: Ручной арбалет
    202: Медный арбалет
    203: Легкий арбалет
    204: Серебрянный арбалет
    205: Золотой арбалет
    206: Охотничий арбалет
    207: Дуэльный арбалет
    208: Элитный ручной арбалет
    209: Элитный легкий арбалет
    210: Элитный золотой арбалет
    211: Элитный дуэльный арбалет
    212: Рунический ручной арбалет
    213: Рунический легкий арбалет
    214: Рунический золотой арбалет
    215: Рунический дуэльный арбалет
    216: Арбалет
    217: Большой арбалет
    218: Тяжелый арбалет
    219: Железный арбалет
    220: Стальной арбалет
    221: Боевой арбалет
    222: Осадный арбалет
    223: Элитный арбалет
    224: Элитный тяжелый арбалет
    225: Элитный стальной арбалет
    226: Элитный осадный арбалет
    227: Рунический арбалет
    228: Рунический тяжелый арбалет
    229: Рунический стальной арбалет
    230: Рунический осадный арбалет
    231: Кристалл вызова демона воды
    232: Кристалл вызова демона огня
    233: Кристалл вызова демона земли
    234: Кристалл вызова демона воздуха
    235: Водный кристалл Вызова
    236: Огненный кристалл Вызова
    237: Земляной кристалл Вызова
    238: Воздушный кристалл Вызова
    240: Ключ от чёрной комнаты воды
    241: Ключ от чёрной комнаты огня
    242: Ключ от чёрной комнаты земли
    243: Ключ от чёрной комнаты воздуха
    244: Ключ от белой комнаты воды
    245: Ключ от белой комнаты огня
    246: Ключ от белой комнаты земли
    247: Ключ от белой комнаты воздуха
    262: Новогодний Колпак
    274: Буква З
    275: Буква А
    276: Буква Щ
    277: Буква И
    278: Буква Т
    279: Буква Н
    280: Буква К
    301: Куртка
    302: Кожаная броня
    303: Клёпанная кожаная броня
    305: Кольчуга
    307: Кираса
    310: Древняя броня
    313: Деревянный щит
    314: Большой щит
    317: Обитый деревянный щит
    320: Обитый большой щит
    323: Железный щит
    337: Шапка
    338: Кожаный шлем
    339: Клёпанный кожаный шлем
    341: Кольчужный шлем
    343: Железный шлем
    346: Древний железный шлем
    349: Кушак
    350: Кожаный пояс
    351: Клёпанный кожаный пояс
    353: Кольчужный пояс
    355: Железный пояс
    358: Древний железный пояс
    361: Сапоги
    362: Кожаные ботинки
    363: Клёпанные кожаные ботинки
    365: Кольчужные ботинки
    367: Латные сапоги
    370: Древние латные сапоги
    373: Штаны
    374: Кожаные штаны
    375: Клёпанные кожаные штаны
    377: Кольчужные штаны
    379: Латные поножи
    382: Древние латные поножи
    401: Стеклянный амулет
    413: Бронзовый защитный браслет
    425: Кольцо
    501: Порошок физической защиты
    509: Порошок атаки Земли
    521: Порошок магической защиты
    529: Порошок камнепада
    537: Порошок здоровья
    543: Порошок слабости
    547: Порошок сверхзащиты
    550: Порошок ослепления
    551: Порошок физической защиты
    554: Порошок атаки Земли
    557: Порошок магической защиты
    560: Порошок камнепада
    563: Порошок сверхзащиты
    570: Эликсир обезвреживания ловушек
    571: Эликсир обезвреживания ловушек
    572: Эликсир обезвреживания ловушек
    573: Эликсир обезвреживания ловушек
    574: Эликсир обезвреживания ловушек
    575: Эликсир обезвреживания ловушек
    576: Эликсир обезвреживания ловушек
    577: Эликсир обезвреживания ловушек
    578: Эликсир обезвреживания ловушек
    579: Эликсир обезвреживания ловушек
    580: Эликсир обезвреживания ловушек
    581: Эликсир обезвреживания ловушек
    601: Порошок здоровья
    607: Порошок ледяной атаки
    619: Порошок кислотной атаки
    626: Порошок болезни
    632: Порошок ядовитого удара
    635: Порошок очищения
    636: Порошок яда
    641: Порошок регенерации
    645: Порошок полного очищения
    646: Порошок здоровья
    655: Порошок ледяной атаки
    658: Порошок кислотной атаки
    661: Порошок болезни
    664: Порошок ядовитого удара
    670: Эликсир Узла 
    671: Эликсир Узла 
    672: Эликсир Узла 
    673: Эликсир Узла 
    674: Эликсир Узла 
    675: Эликсир Узла 
    676: Эликсир Узла 
    680: Порошок защиты Сестёр
    701: Порошок шока
    713: Порошок молний
    722: Порошок усиления
    727: Порошок цепной молнии
    734: Порошок ослабления
    738: Порошок воздушного проклятия
    740: Порошок замедления
    741: Порошок шока
    744: Порошок молний
    747: Порошок цепной молнии
    750: Порошок ослабления
    753: Порошок усиления
    801: Порошок пламенных стрел
    807: Порошок огненных шаров
    812: Порошок усиления
    818: Порошок серного дождя
    822: Порошок огненного проклятия
    827: Порошок испепеления
    832: Порошок большого огня
    835: Порошок великого огня
    836: Порошок ядовитого пламени
    841: Порошок огненного ослепления
    850: Порошок атаки Культа
    851: Саидакра
    852: Порошок лечения Культа
    853: Порошок защиты Культа
    854: Порошок пламенных стрел
    857: Порошок огненных шаров
    860: Порошок усиления
    863: Порошок серного дождя
    866: Порошок огненного проклятия
    869: Порошок испепеления
    872: Порошок большого огня
    875: Порошок справедливости
    876: Порошок нечисти
    900: Герудит
    901: Бирюза
    902: Сердолик
    903: Агат
    904: Сферит
    905: Терратон
    906: Астрамат
    907: Пармелит
    908: Малахит
    909: Аквамарин
    910: Содалит
    911: Эритрит
    912: Маринит
    913: Гиперит
    914: Марсолик
    915: Фитолит
    916: Камелик
    917: Кремень
    918: Уголь
    919: Сапронак
    920: Форвес
    921: Флема
    922: Веорант
    923: Патронит
    924: Пирит
    925: Ардамат
    926: Мелеозит
    927: Фейрик
    928: Чатлит
    929: Берманат
    930: Гранит
    931: Галеонат
    932: Александрит
    933: Хорт
    934: Чароит
    940: Цветок папоротника
    941: Цветок ференгона
    942: Цветок целесты
    943: Кледер
    944: Флорус
    945: Парис
    946: Цветы креатолуса
    947: Корень мандрагоры
    948: Пишаль
    949: Лист легры
    950: Ягоды серепея
    951: Цветок самальи
    952: Цветок желены
    953: Цветок циркулония
    954: Цветок мельверы
    955: Сердень
    956: Горицвет
    957: Пальмирус
    958: Фесень
    959: Драконий глаз
    960: Лист виолуса
    961: Казадур
    962: Невежень
    963: Цак
    964: Кавайя
    965: Эдельвейс
    966: Шалфей
    967: Катарантус
    970: Золото
    971: Серебро
    972: Медь
    973: Железо
    974: Свинец
    975: Олово
    976: Платина
    977: Мифрил
    978: Алюминий
    979: Вольфрам
    980: Иттрий
    1000: Палочник
    1001: Палочник степной
    1002: Палочник лесной
    1003: Палочник пещерный
    1010: Псоглавец
    1011: Псоглавец степной
    1012: Псоглавец лесной
    1013: Псоглавец пещерный
    1020: Трухлявый рыцарь
    1021: Ржавый рыцарь
    1022: Неприкаянный рыцарь
    1023: Золотой рыцарь
    1024: Гигантский рыцарь
    1025: Воздушный рыцарь
    1030: Летун
    1031: Штормовой летун
    1032: Огненный летун
    1033: Янтарный летун
    1034: Гигантский летун
    1040: Земляной голем
    1041: Песочный голем
    1042: Глиняный голем
    1043: Свободный голем
    1044: Гигантский голем
    1050: Бангвильская сколопендра
    1051: Царская сколопендра
    1060: Скелет
    1061: Скелет лесной
    1062: Скелет Умрадский
    1063: Скелет воздушный
    1064: Скелет пещерный
    1065: Лич
    1066: Гигантский скелет
    1067: Воздушный лич
    1069: Ассасин
    1070: Вепрь
    1071: Бешеный вепрь
    1072: Сухой вепрь
    1073: Воздушный вепрь
    1074: Железный вепрь
    1075: Огненный вепрь
    1076: Гигантский вепрь
    1080: Цианос
    1081: Цианос тёмный
    1082: Цианос сухой
    1083: Цианос железный
    1084: Цианос воздушный
    1085: Цианос гигантский
    1090: Волк
    1091: Степной волк
    1092: Ледяной волк
    1093: Сухой волк
    1094: Железный волк
    1095: Адский волк
    1100: Паук
    1101: Серный паук
    1102: Сухой паук
    1103: Железный паук
    1104: Огненный паук
    1105: Чёрная вдова
    1106: Гигантский паук
    1107: Воздушный паук
    1110: Саламандра
    1111: Номрадская саламандра
    1112: Земляная саламандра
    1113: Железная саламандра
    1114: Саламандига
    1120: Красный скорпион
    1121: Скорпион
    1122: Синий скорпион
    1123: Огненный скорпион
    1130: Тифон
    1131: Дымный тифон
    1132: Визжащий тифон
    1133: Железный тифон
    1134: Огненный тифон
    1135: Гигантский тифон
    1140: Людоед
    1141: Серный людоед
    1142: Пещерный людоед
    1143: Гигантский людоед
    1150: Нифон
    1160: Зелёный дракон
    1161: Синий дракон
    1162: Красный дракон
    1163: Стальной дракон
    1164: Гигантский дракон
    1170: Кошка
    1180: Бык
    1190: Тропос
    1191: Бешеный тропос
    1192: Торфяной тропос
    1193: Королевский тропос
    1194: Воздушный тропос
    1200: Мамонт
    1201: Железный мамонт
    1202: Снежный мамонт
    1203: Угольный мамонт
    1210: Зомби
    1211: Серый зомби
    1212: Зомбадер
    1213: Гигантский зомби
    1220: Зачарованное дерево
    1221: Красное дерево
    1222: Мёртвое дерево
    1230: Гранитный камнеед
    1231: Изумрудный камнеед
    1232: Сапфировый камнеед
    1233: Хозяин скал
    1240: Ходячий труп
    1241: Кадавр
    1242: Кадавр-паук
    1243: Костяной кадавр
    1244: Гигантский кадавр
    1250: Нетопырь
    1251: Серый нетопырь
    1252: Огненный нетопырь
    1253: Гигантский нетопырь
    1260: Лесная менада
    1261: Степная менада
    1262: Пещерная менада
    1263: Магическая менада
    1264: Воздушная менада
    1270: Карлик
    1271: Лесной Карлик
    1272: Кратерный Карлик
    1273: Воздушный карлик
    1280: Замковый камень
    1281: Защитный камень
    1290: Ассасин
    1291: Свободный ассасин
    1292: Призрачный ассасин
    1300: Дракост
    1301: Золотой дракост
    1302: Ледяной дракост
    1310: Глот
    1311: Болотный глот
    1320: Вестник смерти
    1330: Курганник
    1340: Разбойник
    1350: Дух башни
    1360: Чёрный дракон
    1361: Белый дракон
    1362: Чёрный дракон
    1363: Белый дракон
    1364: Чёрный дракон
    1365: Белый дракон
    1366: Великий чёрный дракон
    1367: Великий белый дракон
    1368: Чёрный раненый дракон
    1370: Чёрный наездник
    1371: Белая наездница
    1380: Плывунец
    1400: Огненный змей
    1401: Костяной змей
    1402: Железный змей
    1410: Подземный червяк
    1420: Русалка
    1430: Мясной щупальник
    1431: Железный щупальник
    1440: Костяная собака
    1450: Харонский охотник
    1451: Огненный охотник
    1452: Железный охотник
    1453: Мёртвый охотник
    1460: Огненная охотница
    1461: Железная охотница
    1462: Харонская охотница
    1463: Мёртвая охотница
    1470: Страж бездны
    1480: Химера
    1850: Летун некроманта
    1870: Демон воды
    1871: Демон огня
    1872: Демон воздуха
    1873: Демон земли
    1874: Летающий демон
    1875: Сверх-Демон
    1880: Бангвильская сколопендра
    1881: Палочник
    1882: Тропос
    1883: Ржавый рыцарь
    1900: Дух замка Льеж
    1901: Дух замка Фьеф
    1902: Дух замка Арис
    1903: Дух замка Латор
    1904: Дух замка Эйкум-кас
    1905: Дух замка Гедеон
    1906: Дух замка Шателье
    1907: Дух замка Туанод
    1908: Дух замка Пельтье
    1909: Дух замка Каре-Рояль
    1910: Дух замка Блессендор
    1911: Дух замка Терноваль
    1912: Дух замка Аммалаэль
    1913: Дух замка Каблак
    1914: Дух замка Дэванагари
    1915: Дух замка Сабулат
    1916: Дух замка Деффенсат
    1917: Дух замка Айонат
    1918: Дух замка Триумфалер
    1919: Дух замка Хангаар
    1920: Дух замка Дабрад
    1921: Дух замка Сед
    1922: Дух замка Лендер
    1923: Дух замка Келлос
    1924: Дух замка Шиброн
    1925: Дух замка Нимед
    1926: Дух замка Канакун
    1927: Дух замка Элдук
    1928: Дух замка Янг
    1929: Дух замка Элек
    1930: Дух замка Гавот
    1931: Дух замка Кандур
    1932: Дух замка Иммертель
    1933: Дух замка Нарцисс
    1934: Дух замка Ранден
    1935: Дух замка Ниргун
    1936: Дух замка Гелгивинн
    1937: Дух замка Иль-Суильи-Руа
    2000: Карта Гипериона
    2001: Гиперион, С-В
    2002: Гиперион, С-З
    2003: Гиперион, З
    2004: Гиперион, Ю-З
    2005: Гиперион, Ю-В
    2006: Гиперион, В
    2007: Карта Шипстоуна
    2008: Карта Торвила
    2009: Карта Бангвиля
    2010: Карта Санпула
    2011: Карта Харона
    2012: Карта Феба
    2013: Карта Родоса
    2300: Мантра
    2301: Мантра
    2302: Мантра
    2303: Мантра
    2304: Мантра
    2305: Мантра
    2306: Мантра
    2307: Мантра
    2308: Мантра
    2309: Мантра
    2310: Мантра
    2311: Мантра
    2312: Мантра
    2313: Мантра
    2314: Мантра
    2315: Мантра
    2316: Мантра
    2317: Мантра
    2318: Мантра
    2319: Мантра
    2320: Мантра
    2321: Мантра
    2322: Мантра
    2323: Мантра
    2324: Мантра
    2325: Мантра
    2326: Мантра
    2327: Мантра
    2328: Мантра
    2329: Мантра
    2330: Мантра
    2331: Мантра
    2332: Мантра
    2333: Мантра
    2334: Мантра
    2335: Мантра
    2336: Мантра
    2337: Мантра
    2338: Мантра
    2339: Мантра
    2340: Мантра
    2341: Мантра
    2342: Мантра
    2343: Мантра
    2344: Мантра
    2345: Мантра
    2346: Мантра
    2347: Мантра
    2348: Мантра
    2349: Мантра
    2350: Мантра
    2351: Мантра
    2352: Мантра
    2353: Мантра
    2354: Мантра
    2355: Мантра
    2356: Мантра
    2357: Мантра
    2358: Мантра
    2359: Мантра
    2360: Мантра
    2361: Мантра
    2362: Мантра
    2363: Мантра
    2364: Мантра
    2365: Мантра
    2366: Мантра
    2367: Мантра
    2368: Мантра
    2369: Мантра
    2370: Мантра
    2371: Мантра
    2372: Мантра
    2374: Мантра
    2375: Мантра
    2376: Мантра
    2377: Мантра
    2378: Мантра
    2379: Мантра
    2380: Мантра
    2381: Мантра
    2382: Мантра
    2383: Мантра
    2384: Мантра
    2385: Мантра
    2386: Мантра
    2387: Мантра
    2388: Мантра
    2389: Мантра
    2390: Мантра
    2391: Мантра
    2392: Мантра
    2393: Мантра
    2394: Мантра
    2395: Мантра
    2396: Мантра
    2397: Мантра
    2398: Мантра
    2399: Мантра
    2401: Мантра
    2402: Мантра
    2403: Мантра
    2404: Мантра
    2405: Мантра
    2406: Мантра
    2407: Мантра
    2410: Мантра
    2411: Мантра
    2412: Мантра
    2413: Мантра
    2414: Мантра
    2415: Мантра
    2416: Мантра
    2417: Мантра
    2418: Мантра
    2419: Мантра
    2420: Мантра
    2421: Мантра
    2422: Мантра
    2423: Мантра
    2424: Мантра
    2425: Мантра
    2426: Мантра
    2427: Мантра
    2428: Мантра
    2429: Мантра
    2430: Мантра
    2431: Мантра
    2432: Мантра
    2433: Мантра
    2434: Мантра
    2435: Мантра
    2436: Мантра
    2437: Мантра
    2438: Мантра
    2439: Мантра
    2440: Мантра
    2441: Мантра
    2442: Мантра
    2443: Мантра
    2444: Мантра
    2445: Мантра
    2446: Мантра
    2447: Мантра
    2448: Мантра
    2449: Мантра
    2450: Мантра
    2451: Мантра
    2452: Мантра
    2453: Мантра
    2454: Мантра
    2455: Мантра
    2456: Мантра
    2457: Мантра
    2459: Мантра
    2460: Мантра
    2461: Мантра
    2462: Мантра
    2463: Мантра
    2464: Мантра
    2465: Мантра
    2466: Мантра
    2467: Мантра
    2468: Мантра
    2469: Мантра
    2470: Мантра
    2471: Мантра
    2472: Мантра
    2473: Мантра
    2474: Мантра
    2475: Мантра Атишула сфурьяна
    2700: Сабля
    2702: Великий меч
    2704: Элитная катана
    2706: Рунический гладиус
    2708: Легендарный кристальный меч
    2710: Драконий меч
    2715: Франциска
    2717: Табар
    2719: Элитная секира
    2721: Руническая булава
    2723: Призрачный топор
    2726: Драконий молот
    2730: Ручной арбалет
    2732: Тяжелый арбалет
    2734: Элитный ручной арбалет
    2736: Рунический легкий арбалет
    2738: Призрачный стальной арбалет
    2741: Драконий осадный арбалет
    2745: Куртка
    2747: Кожаная броня
    2749: Клёпанная кожаная броня
    2751: Кольчуга
    2753: Кираса
    2756: Древняя броня
    2760: Деревянный щит
    2762: Большой щит
    2764: Обитый деревянный щит
    2766: Обитый большой щит
    2769: Железный щит
    2775: Штаны
    2777: Кожаные штаны
    2779: Клепанные кожаные штаны
    2781: Кольчужные штаны
    2783: Латные поножи
    2786: Древние латные поножи
    2790: Шапка
    2792: Кожаный шлем
    2794: Клёпанный кожаный шлем
    2796: Кольчужный шлем
    2798: Железный шлем
    2801: Древний железный шлем
    2805: Кушак
    2807: Кожаный пояс
    2809: Клёпанный кожаный пояс
    2811: Кольчужный пояс
    2813: Железный пояс
    2816: Древний железный пояс
    2820: Сапоги
    2822: Кожаные ботинки
    2824: Клёпанные кожаные ботинки
    2826: Кольчужные ботинки
    2828: Латные сапоги
    2831: Древние латные сапоги
    2835: Перчатки
    2837: Кожаные перчатки
    2839: Клёпанные кожаные перчатки
    2841: Кольчужные перчатки
    2843: Латные перчатки
    2846: Древние латные перчатки
    2850: Коричневая роба
    2858: Синяя роба
    2950: Артефакт. Красная роба
    2956: Артефакт. Белая роба
    2962: Артефакт. Синяя роба
    3001: Куртка
    3005: Кожаная броня
    3010: Клёпанная кожаная броня
    3020: Кольчуга
    3030: Кираса
    3045: Древняя броня
    3061: Деревянный щит
    3065: Большой щит
    3080: Обитый деревянный щит
    3095: Обитый большой щит
    3110: Железный щит
    3121: Штаны
    3123: Кожаные штаны
    3125: Клепанные кожаные штаны
    3129: Кольчужные штаны
    3133: Латные поножи
    3139: Древние латные поножи
    3146: Шапка
    3148: Кожаный шлем
    3150: Клёпанный кожаный шлем
    3154: Кольчужный шлем
    3158: Железный шлем
    3164: Древний железный шлем
    3171: Кушак
    3172: Кожаный пояс
    3173: Клёпанный кожаный пояс
    3175: Кольчужный пояс
    3177: Железный пояс
    3180: Древний железный пояс
    3183: Сапоги
    3184: Кожаные ботинки
    3185: Клёпанные кожаные ботинки
    3187: Кольчужные ботинки
    3189: Латные сапоги
    3192: Древние латные сапоги
    3251: Кривой меч
    3253: Сабля
    3255: Палаш
    3258: Скимитар
    3260: Ятаган
    3263: Катана
    3265: Шамшер
    3270: Элитная сабля
    3275: Элитный палаш
    3280: Элитный ятаган
    3285: Элитная катана
    3290: Руническая сабля
    3295: Рунический палаш
    3300: Рунический ятаган
    3305: Руническая катана
    3311: Полуторный меч
    3313: Двуручный меч
    3315: Клеймор
    3318: Эспадон
    3320: Фламберг
    3323: Великий меч
    3325: Гигантский меч
    3330: Элитный двуручный меч
    3335: Элитный клеймор
    3340: Элитный фламберг
    3345: Элитный великий меч
    3350: Рунический двуручный меч
    3355: Рунический клеймор
    3360: Рунический фламберг
    3365: Рунический великий меч
    3371: Короткий меч
    3373: Широкий меч
    3375: Гладиус
    3378: Пехотный меч
    3380: Длинный меч
    3383: Кристальный меч
    3385: Золотой меч
    3390: Элитный короткий меч
    3395: Элитный гладиус
    3400: Элитный длинный меч
    3405: Элитный кристальный меч
    3410: Рунический короткий меч
    3415: Рунический гладиус
    3420: Рунический длинный меч
    3425: Рунический кристальный меч
    3431: Легендарная катана
    3434: Призрачная катана
    3437: Драконья катана
    3440: Легендарный великий меч
    3443: Призрачный великий меч
    3446: Драконий великий меч
    3449: Легендарный кристальный меч
    3452: Призрачный кристальный меч
    3455: Драконий кристальный меч
    3501: Топор
    3503: Большой топор
    3505: Боевой топор
    3508: Пехотный топор
    3510: Франциска
    3513: Секира
    3515: Табар
    3520: Элитный большой топор
    3525: Элитный боевой топор
    3530: Элитная франциска
    3535: Элитная секира
    3540: Рунический большой топор
    3545: Рунический боевой топор
    3550: Руническая франциска
    3555: Руническая секира
    3561: Дубинка
    3563: Булава
    3565: Палица
    3568: Молот
    3570: Боевой молот
    3573: Моргенштерн
    3575: Кузнечный молот
    3580: Элитная булава
    3585: Элитная палица
    3590: Элитный боевой молот
    3595: Элитный моргенштерн
    3600: Руническая булава
    3605: Руническая палица
    3610: Рунический боевой молот
    3615: Рунический моргенштерн
    3621: Посох мастера игры
    3622: Легендарный топор
    3625: Призрачный топор
    3628: Драконий топор
    3631: Легендарный молот
    3634: Призрачный молот
    3637: Драконий молот
    3644: Посох разработчика игры
    3751: Ручной арбалет
    3753: Медный арбалет
    3755: Легкий арбалет
    3758: Серебрянный арбалет
    3760: Золотой арбалет
    3763: Охотничий арбалет
    3765: Дуэльный арбалет
    3770: Элитный ручной арбалет
    3775: Элитный легкий арбалет
    3780: Элитный золотой арбалет
    3785: Элитный дуэльный арбалет
    3790: Рунический ручной арбалет
    3795: Рунический легкий арбалет
    3800: Рунический золотой арбалет
    3805: Рунический дуэльный арбалет
    3811: Арбалет
    3813: Большой арбалет
    3815: Тяжелый арбалет
    3818: Железный арбалет
    3820: Стальной арбалет
    3823: Боевой арбалет
    3825: Осадный арбалет
    3830: Элитный арбалет
    3835: Элитный тяжелый арбалет
    3840: Элитный стальной арбалет
    3845: Элитный осадный арбалет
    3850: Рунический арбалет
    3855: Рунический тяжелый арбалет
    3860: Рунический стальной арбалет
    3865: Рунический осадный арбалет
    3871: Легендарный дуэльный арбалет
    3874: Призрачный дуэльный арбалет
    3877: Драконий дуэльный арбалет
    3880: Легендарный осадный арбалет
    3883: Призрачный осадный арбалет
    3886: Драконий осадный арбалет
    4001: Стеклянный амулет
    4024: Бронзовый защитный браслет
    4047: Кольцо
    4167: Стеклянный амулет
    4176: Бронзовый защитный браслет
    4185: Амулет Празднования
    4188: Освежающий амулет
    4190: Стеклянный амулет радости
    4191: Стеклянный амулет счастья
    4192: Стеклянный амулет добродетели
    4193: Хрустальный амулет утешения
    4194: Хрустальный амулет ликования
    4195: Кольцо волка
    4196: Кольцо голема
    4197: Кольцо цианоса
    4199: Амулет life
    4200: Кольцо невидимости
    4242: Хризантема
    4243: Роза
    4245: Ирис
    4247: Тюльпан
    4249: Гербера
    4251: Орхидея
    4302: Большой новогодний амулет
    4304: Малый новогодний амулет
    4306: Недолговечный амулет ликования
    4308: Амулет Возрождения
    4310: Подарочный амулет
    4312: Кольцо скелета
    4314: Кольцо карлика
    4316: Кольцо старца
    4318: Кольцо голема
    4320: Кольцо лесной менады
    4322: Кольцо пещерной менады
    4324: Кольцо цианоса
    4326: Кольцо вампирши
    4328: Кольцо тропоса
    4330: Кольцо зомби
    4332: Пилюля титула Х2 (должна лежать в инвентаре, на 3 часа)
    4334: Пилюля степени Х2 (должна лежать в инвентаре, на 3 часа)
    4336: Пилюля могущества Х2 (должна лежать в инвентаре, на 3 часа)
    4338: Пилюля титула Х2 (должна лежать в инвентаре, на 6 часов)
    4340: Пилюля степени Х2 (должна лежать в инвентаре, на 6 часов)
    4342: Пилюля могущества Х2 (должна лежать в инвентаре, на 6 часов)
    4344: Пилюля титула Х2 (должна лежать в инвентаре, на 12 часов)
    4346: Пилюля степени Х2 (должна лежать в инвентаре, на 12 часов)
    4348: Пилюля могущества Х2 (должна лежать в инвентаре, на 12 часов)
    4453: Кираса
    4462: Железный щит
    4471: Древние латные поножи
    4480: Древний железный шлем
    4489: Древний железный пояс
    4498: Древние латные сапоги
    4740: Великая Чёрная Кираса
    4741: Великая Белая Кираса
    4742: Чёрные латные поножи
    4743: Белые латные поножи
    4744: Чёрный шлем
    4745: Белый шлем
    4746: Чёрные сапоги
    4747: Белые сапоги
    4750: Великая Чёрная Кираса
    4751: Великая Белая Кираса
    5001: Клепанная кожаная броня
    5002: Большой щит
    5003: Клёпанные кожаные штаны
    5004: Клёпанный кожаный шлем
    5005: Клёпанный кожаный пояс
    5006: Клёпанные кожаные ботинки
    5007: Клёпанные кожаные перчатки
    5008: Коричневая роба
    5009: Синяя роба
    5010: Белая роба
    5011: Красная роба
    5021: Кираса
    5022: Обитый деревянный щит
    5023: Латные поножи
    5024: Железный шлем
    5025: Железный пояс
    5026: Латные сапоги
    5027: Латные перчатки
    5028: Коричневая роба
    5029: Синяя роба
    5030: Белая роба
    5031: Красная роба
    5041: Древняя броня
    5042: Обитый большой щит
    5043: Древние латные поножи
    5044: Древний железный шлем
    5045: Древний железный пояс
    5046: Древние латные сапоги
    5047: Древние латные перчатки
    5048: Коричневая роба
    5049: Синяя роба
    5050: Белая роба
    5051: Красная роба
    5061: Древняя броня
    5062: Железный щит
    5063: Древние латные поножи
    5064: Древний железный шлем
    5065: Древний железный пояс
    5066: Древние латные сапоги
    5067: Древние латные перчатки
    5068: Коричневая роба
    5069: Синяя роба
    5070: Белая роба
    5071: Красная роба
    5081: Малый кристалл силы
    5082: Малый кристалл энергии
    5083: Малый кристалл стойкости
    5084: Малый кристалл отражения
    5085: Средний кристалл силы
    5086: Средний кристалл энергии
    5087: Средний кристалл стойкости
    5088: Средний кристалл отражения
    5089: Большой кристалл силы
    5090: Большой кристалл энергии
    5091: Большой кристалл стойкости
    5092: Большой кристалл отражения
    5093: Малый темный кристалл силы
    5094: Малый темный кристалл энергии
    5095: Малый темный кристалл стойкости
    5096: Малый темный кристалл отражения
    5097: Средний темный кристалл силы
    5098: Средний темный кристалл энергии
    5099: Средний темный кристалл стойкости
    5100: Средний темный кристалл отражения
    5101: Большой темный кристалл силы
    5102: Большой темный кристалл энергии
    5103: Большой темный кристалл стойкости
    5104: Великий кристалл силы
    5105: Великий кристалл энергии
    5106: Великий кристалл кристалл стойкости
    5107: Великий кристалл отражения
    5108: Большой темный кристалл отражения
    5111: Древняя броня
    5112: Железный щит
    5113: Древние латные поножи
    5114: Древний железный шлем
    5115: Древний железный пояс
    5116: Древние латные сапоги
    5117: Древние латные перчатки
    5118: Коричневая роба
    5119: Синяя роба
    5120: Белая роба
    5121: Красная роба
    5131: Древняя броня
    5132: Железный щит
    5133: Древние латные поножи
    5134: Древний железный шлем
    5135: Древний железный пояс
    5136: Древние латные сапоги
    5137: Древние латные перчатки
    5138: Коричневая роба
    5139: Синяя роба
    5140: Белая роба
    5141: Красная роба
    5150: Шамшер
    5151: Элитный ятаган
    5152: Рунический палаш
    5153: Руническая катана
    5154: Легендарная катана
    5155: Призрачная катана
    5156: Драконья катана
    5160: Древняя броня
    5161: Железный щит
    5162: Древние латные поножи
    5163: Древний железный шлем
    5164: Древний железный пояс
    5165: Древние латные сапоги
    5166: Древние латные перчатки
    5167: Коричневая роба
    5168: Синяя роба
    5169: Белая роба
    5170: Красная роба
    5181: Малый кристалл силы
    5182: Малый кристалл энергии
    5183: Малый кристалл стойкости
    5184: Малый кристалл отражения
    5185: Средний кристалл силы
    5186: Средний кристалл энергии
    5187: Средний кристалл стойкости
    5188: Средний кристалл отражения
    5189: Большой кристалл силы
    5190: Большой кристалл энергии
    5191: Большой кристалл стойкости
    5192: Большой кристалл отражения
    5193: Малый темный кристалл силы
    5194: Малый темный кристалл энергии
    5195: Малый темный кристалл стойкости
    5196: Малый темный кристалл отражения
    5197: Средний темный кристалл силы
    5198: Средний темный кристалл энергии
    5199: Средний темный кристалл стойкости
    5200: Средний темный кристалл отражения
    5201: Большой темный кристалл силы
    5202: Большой темный кристалл энергии
    5203: Большой темный кристалл стойкости
    5204: Великий кристалл силы
    5205: Великий кристалл энергии
    5206: Великий кристалл кристалл стойкости
    5207: Великий кристалл отражения
    5208: Большой темный кристалл отражения
    5281: Малый кристалл силы
    5282: Малый кристалл энергии
    5283: Малый кристалл стойкости
    5284: Малый кристалл отражения
    5285: Средний кристалл силы
    5286: Средний кристалл энергии
    5287: Средний кристалл стойкости
    5288: Средний кристалл отражения
    5289: Большой кристалл силы
    5290: Большой кристалл энергии
    5291: Большой кристалл стойкости
    5292: Большой кристалл отражения
    5293: Малый темный кристалл силы
    5294: Малый темный кристалл энергии
    5295: Малый темный кристалл стойкости
    5296: Малый темный кристалл отражения
    5297: Средний темный кристалл силы
    5298: Средний темный кристалл энергии
    5299: Средний темный кристалл стойкости
    5300: Средний темный кристалл отражения
    5301: Большой темный кристалл силы
    5302: Большой темный кристалл энергии
    5303: Большой темный кристалл стойкости
    5304: Великий кристалл силы
    5305: Великий кристалл энергии
    5306: Великий кристалл кристалл стойкости
    5307: Великий кристалл отражения
    5308: Большой темный кристалл отражения
    5500: Гилд ассасина (1 ступень)
    5501: Гилд ассасина (2 ступень)
    5502: Гилд ассасина (3 ступень)
    5503: Гилд ассасина (4 ступень)
    5504: Гилд ассасина (5 ступень)
    5505: Гилд ассасина (6 ступень)
    5510: Невидимость
    5520: Критический удар
    5530: Яд
    5565: Кинжал убийства
    5571: Ботинки убийцы
    5572: Кожаные ботинки убийцы
    5573: Клепанные ботинки убийцы
    5574: Кольчужные ботинки убийцы
    5575: Латные ботинки убийцы
    5576: Древние ботинки убийцы
    5580: Браслет шторма
    5586: Куртка злодея
    5587: Кожаная куртка злодея
    5588: Клепанная куртка злодея
    5589: Кольчуга злодея
    5590: Кираса злодея
    5591: Броня злодея
    5600: Гилд крестоносца (1 ступень)
    5601: Гилд крестоносца (2 ступень)
    5602: Гилд крестоносца (3 ступень)
    5603: Гилд крестоносца (4 ступень)
    5604: Гилд крестоносца (5 ступень)
    5605: Гилд крестоносца (6 ступень)
    5610: Божественный перенос
    5611: Божественный  перенос
    5612: Божественный перенос
    5620: Щит праны
    5630: Бег времени
    5665: Пояс отваги
    5666: Кожаный пояс отваги
    5667: Клепанный пояс отваги
    5668: Кольчужный пояс отваги
    5669: Железный пояс отваги
    5670: Древний пояс отваги
    5671: Благой амулет
    5677: Куртка сияния
    5678: Кожаная куртка сияния
    5679: Клепанная куртка сияния
    5680: Кольчуга сияния
    5681: Кираса сияния
    5682: Броня сияния
    5683: Длинный меч справедливости
    5684: Элитный меч справедливости
    5687: Большой меч справедливости
    5688: Великий меч справедливости
    5700: Гилд инквизитора (1 ступень)
    5701: Гилд инквизитора (2 ступень)
    5702: Гилд инквизитора (3 ступень)
    5703: Гилд инквизитора (4 ступень)
    5704: Гилд инквизитора (5 ступень)
    5705: Гилд инквизитора (6 ступень)
    5710: Возрождение
    5711: Возрождение 
    5712: Возрождение
    5720: Оковы
    5730: Очищение
    5750: Посох Инквизитора
    5751: Посох Великого Инквизитора
    5759: Порошок Ткача
    5771: Шапка Инквизитора
    5772: Кожаный шлем Инквизитора
    5773: Клепанный шлем Инквизитора
    5774: Кольчужный шлем Инквизитора
    5775: Железный шлем Инквизитора
    5776: Древний шлем Инквизитора
    5777: Пояс Трибунала
    5778: Кожаный пояс Трибунала
    5779: Клепанный пояс Трибунала
    5780: Кольчужный пояс Трибунала
    5781: Железный пояс Трибунала
    5782: Древний пояс Трибунала
    5800: Гилд охотника (1 ступень)
    5801: Гилд охотника (2 ступень)
    5802: Гилд охотника (3 ступень)
    5803: Гилд охотника (4 ступень)
    5804: Гилд охотника (5 ступень)
    5805: Гилд охотника (6 ступень)
    5810: Белкин глаз
    5820: Двойная стрела
    5830: Странник
    5865: Куртка скрытности
    5866: Кожаная куртка скрытности
    5867: Клепанная куртка скрытности
    5868: Кольчуга скрытности
    5869: Кираса скрытности
    5870: Броня скрытности
    5871: Щит охотника
    5877: Оберег зверя
    5883: Охотничий арбалет
    5884: Большой охотничий арбалет
    5885: Тяжелый охотничий арбалет
    5886: Железный охотничий арбалет
    5887: Стальной охотничий арбалет
    5888: Великий охотничий арбалет
    5900: Гилд архимага (1 ступень)
    5901: Гилд архимага (2 ступень)
    5902: Гилд архимага (3 ступень)
    5903: Гилд архимага (4 ступень)
    5904: Гилд архимага (5 ступень)
    5905: Гилд архимага (6 ступень)
    5910: Обитель
    5911: Обитель 
    5912: Обитель
    5920: Близорукость
    5930: Проклятье
    5950: Посох де Орко
    5951: Посох Архимага
    5952: Посох Мага
    5953: Посох Элементов
    5954: Кольцо Мага
    5955: Кольцо Архимага
    5956: Кольцо Великого Архимага
    5957: Амулет Мага
    5958: Амулет Архимага
    5959: Амулет Великого Архимага
    5961: Сапоги
    5962: Пояс
    5963: Браслет
    5965: Малый посох Архимага
    5966: Посох Архимага
    5969: Большой посох Архимага
    5970: Великий посох Архимага
    5971: Амулет Архимага
    5977: Кольцо Мага
    5978: Кольцо Архимага
    5982: Кольцо Великого Архимага
    5983: Сапоги Архимага
    5984: Кожаные Сапоги Архимага
    5985: Клепанные Сапоги Архимага
    5986: Кольчужные Сапоги Архимага
    5987: Латные Сапоги Архимага
    5988: Древние Сапоги Архимага
    6000: Гилд варвара (1 ступень)
    6001: Гилд варвара (2 ступень)
    6002: Гилд варвара (3 ступень)
    6003: Гилд варвара (4 ступень)
    6004: Гилд варвара (5 ступень)
    6005: Гилд варвара (6 ступень)
    6010: Дальнозоркость
    6020: Берсерк
    6030: Оглушение
    6065: Молот грома
    6071: Меч вражды
    6077: Пояс Титана
    6078: Кожаный пояс Титана
    6079: Клепанный пояс Титана
    6080: Кольчужный пояс Титана
    6081: Железный пояс Титана
    6082: Древний пояс Титана
    6083: Куртка Титана
    6084: Кожаная куртка Титана
    6085: Клепанная куртка Титана
    6086: Кольчуга Титана
    6087: Кираса Титана
    6088: Броня Титана
    6100: Гилд друида (1 ступень)
    6101: Гилд друида (2 ступень)
    6102: Гилд друида (3 ступень)
    6103: Гилд друида (4 ступень)
    6104: Гилд друида (5 ступень)
    6105: Гилд друида (6 ступень)
    6110: Жизнь природы
    6120: Зверь ночи
    6121: Зверь  ночи
    6122: Зверь ночи
    6130: Метаморфоза
    6165: Порошок группового лечения
    6171: Порошок групповой регенерации
    6183: Малый посох востановления
    6184: Средний посох восстановления
    6186: Большой посох восстановления
    6188: Великий посох восстановления
    6200: Гилд вора (1 ступень)
    6201: Гилд вора (2 ступень)
    6202: Гилд вора (3 ступень)
    6203: Гилд вора (4 ступень)
    6204: Гилд вора (5 ступень)
    6205: Гилд вора (6 ступень)
    6220: Бегство
    6230: Ночная тень
    6249: Кольцо умений
    6255: Браслет умения
    6265: Метательный нож
    6300: Гилд мастера стали (1 ступень)
    6301: Гилд мастера стали (2 ступень)
    6302: Гилд мастера стали (3 ступень)
    6303: Гилд мастера стали (4 ступень)
    6304: Гилд мастера стали (5 ступень)
    6305: Гилд мастера стали (6 ступень)
    6310: Вихрь стали
    6311: Вихрь  стали
    6312: Вихрь стали
    6320: Камае
    6330: Божественный  ветер
    6331: Божественный ветер
    6365: Лабрис
    6369: Стальной Лабрис
    6370: Великий Лабрис
    6371: Боевые поножи
    6372: Кожаные боевые поножи
    6373: Клепанные боевые поножи
    6374: Кольчужные боевые поножи
    6375: Латные боевые поножи
    6376: Древние боевые поножи
    6380: Стальной  браслет
    6386: Аспис
    6389: Железный Аспис
    6390: Стальной Аспис
    6391: Великий Аспис
    6400: Гилд оружейника (1 ступень)
    6401: Гилд оружейника (2 ступень)
    6402: Гилд оружейника (3 ступень)
    6403: Гилд оружейника (4 ступень)
    6404: Гилд оружейника (5 ступень)
    6405: Гилд оружейника (6 ступень)
    6410: Мастерство
    6420: Бегство
    6450: Порошок невидимости
    6451: Рунический меч 
    6452: Топор Мастера
    6453: Молот прочности
    6454: Арбалет Альбеорна
    6455: Эликсир оружейника
    6470: Деревянный щит долголетия
    6471: Большой щит долголетия
    6472: Обитый деревянный щит долголетия
    6473: Обитый большой щит долголетия
    6474: Железный щит долголетия
    6475: Древний щит долголетия
    6476: Пояс долголетия
    6477: Кожаный пояс долголетия
    6478: Клепанный пояс долголетия
    6479: Кольчужный пояс долголетия
    6480: Железный пояс долголетия
    6481: Древний пояс долголетия
    6500: Гилд кузнеца (1 ступень)
    6501: Гилд кузнеца (2 ступень)
    6502: Гилд кузнеца (3 ступень)
    6503: Гилд кузнеца (4 ступень)
    6504: Гилд кузнеца (5 ступень)
    6505: Гилд кузнеца (6 ступень)
    6510: Мастерство
    6520: Бегство
    6551: Тяжёлая броня Калиестра
    6552: Кузнечный молот
    6555: Стальное кольцо
    6556: Серебряное кольцо
    6557: Кузнечный Порошок
    6559: Деревянный кузнечный щит
    6560: Большой кузнечный щит
    6561: Обитый кузнечный щит
    6563: Железный кузнечный щит
    6564: Древний кузнечный щит
    6565: Амулет отторжения
    6571: Ботинки независимости
    6572: Кожаные ботинки независимости
    6573: Клепанные ботинки независимости
    6574: Кольчужные ботинки независимости
    6575: Латные ботинки независимости
    6576: Древние ботинки независимости
    6600: Гилд чародея (1 ступень)
    6601: Гилд чародея (2 ступень)
    6602: Гилд чародея (3 ступень)
    6603: Гилд чародея (4 ступень)
    6604: Гилд чародея (5 ступень)
    6605: Гилд чародея (6 ступень)
    6610: Чары
    6620: Смерч
    6630: Ржавые доспехи
    6649: Порошок невидимости
    6650: Мантра разложения
    6651: Мантра слабости
    6653: Эликсир свободы
    6654: Кольцо умельца
    6655: Кольцо ауры
    6656: Четырёхцветное кольцо
    6657: Пояс лёгкости
    6658: Посох Чародея
    6660: Деревянный зачарованный щит
    6661: Большой зачарованный щит
    6662: Обитый зачарованный щит
    6664: Железный зачарованный щит
    6665: Древний зачарованный щит
    6666: Зачарованные ботинки
    6667: Кожаные зачарованные ботинки
    6668: Клепанные зачарованные ботинки
    6669: Кольчужные зачарованные ботинки
    6670: Латные зачарованные ботинки
    6671: Древние зачарованные ботинки
    6700: Гилд некроманта (1 ступень)
    6701: Гилд некроманта (2 ступень)
    6702: Гилд некроманта (3 ступень)
    6703: Гилд некроманта (4 ступень)
    6704: Гилд некроманта (5 ступень)
    6705: Гилд некроманта (6 ступень)
    6720: Воскрешение
    6725: Воскрешение
    6750: Посох увечий
    6751: Посох тьмы
    6752: Посох жертвоприношения
    6753: Роджер
    6754: Кольцо лича
    6755: Костяной амулет
    6756: Костяной щит
    6758: Болотный пояс
    6759: Порошок деградации
    6760: Костяные поножи
    6761: Змеиный браслет
    6763: Ботинки нежити
    6764: Кожаные ботинки нежити
    6765: Клепанные ботинки нежити
    6766: Кольчужные ботинки нежити
    6767: Латные ботинки нежити
    6768: Древние ботинки нежити
    6800: Гилд бандиера (1 ступень)
    6801: Гилд бандиера (2 ступень)
    6802: Гилд бандиера (3 ступень)
    6803: Гилд бандиера (4 ступень)
    6804: Гилд бандиера (5 ступень)
    6805: Гилд бандиера (6 ступень)
    6850: Флаг храбрости
    6856: Знамя защитников
    6862: Штандарт стихий
    6868: Куртка надежности
    6869: Кожаная куртка надежности
    6870: Клепанная куртка надежности
    6871: Кольчуга надежности
    6872: Кираса надежности
    6873: Броня надежности
    6900: Мантра Хака сапида однака
    7413: Посох Пастуха