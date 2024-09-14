from pathlib import Path
import struct

file_position = 0

def read_binary(data:bytes, format: str, length: int) -> any:
    global file_position
    end = file_position + length
    result = struct.unpack(format, data[file_position:end])
    file_position = end
    return result[0]

def read_float(data: bytes) -> float:
    return read_binary(data, 'f', 4)

def read_ushort(data: bytes) -> int:
    return read_binary(data, 'H', 2)

def read_byte(data: bytes) -> int:
    return read_binary(data, 'B', 1)

def read_string(data: bytes, length: int) -> str:
    return read_binary(data, f'{length}s', length)


data = Path('C:\\Games\\Sfera\\models\\House7.mdl').read_bytes()
magic_bytes = read_string(data, 4)
vertex_count = read_ushort(data)
triangle_count = read_ushort(data)
surface_count = read_ushort(data)
texture_count = read_byte(data)
texture_names_length = read_ushort(data)
object_count = read_byte(data)
object_index_count = read_ushort(data)
transform_key_count = read_ushort(data)
transform_key_index_count = read_ushort(data)
action_count = read_byte(data)

file_position = 0x102

model_names = []
vertices = []
triangles = []
surfaces = []
objects = []
object_indices = []

for i in range (texture_count):
    name_length = read_byte(data)
    name_string = read_string(data, name_length)
    model_names.append(name_string)

for i in range (vertex_count):
    vertex = {}
    vertex['x'] = read_float(data)
    vertex['y'] = read_float(data)
    vertex['z'] = read_float(data)
    vertex['nX'] = read_float(data)
    vertex['nY'] = read_float(data)
    vertex['nZ'] = read_float(data)
    vertex['u'] = read_float(data)
    vertex['v'] = read_float(data)
    vertices.append(vertex)

for i in range (triangle_count):
    triangle = {}
    triangle['v1'] = read_ushort(data)
    triangle['v2'] = read_ushort(data)
    triangle['v3'] = read_ushort(data)
    triangle['flags'] = read_ushort(data)
    triangle['reserved'] = read_ushort(data)
    triangles.append(triangle)

for i in range (surface_count):
    surface = {}
    surface['object_index'] = read_byte(data)
    surface['texture_index'] = read_byte(data)
    surface['first_triangle_index'] = read_ushort(data)
    surface['triangle_count'] = read_ushort(data)
    surface['first_vertex_index'] = read_ushort(data)
    surface['vertex_count'] = read_ushort(data)
    read_binary(data, '5b', 5)
    surfaces.append(surface)

for i in range (object_count):
    object_3d = {}
    object_3d['name'] = read_string(data, 32)
    object_3d['bone_type'] = read_byte(data)
    object_3d['connected_bone_count'] = read_byte(data)
    object_3d['object_index_offset'] = read_byte(data)
    object_3d['is_animated'] = read_byte(data)
    object_3d['key_index'] = read_ushort(data)
    object_3d['parent_index'] = read_byte(data)
    objects.append(object_3d)

for i in range (object_index_count):
    object_indices.append(read_byte(data))

print(model_names)