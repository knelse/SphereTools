#import bpy
import struct
from math import radians
from glob import glob
import os
from pathlib import Path

#bpy.ops.wm.console_toggle()

    
# def import_file (path):
    # file_name = Path(file_path).stem
    # with open (file_path, mode='rb') as file:
        # file_contents = file.read()
        # vertex_count = struct.unpack('H',file_contents[4:6])
        # offset = 0x68c0
        # vertices = []
        # faces = []
        # for i in range (vertex_count[0]):
            # offset_end = offset + 32
            # vertex = struct.unpack('ffffffff',file_contents[offset:offset_end])
            # offset += 40
            # # blender is Z up
            # vertices.append([vertex[0], vertex[2], vertex[1]])
        # while offset < len(file_contents):
            # offset_end = offset + 6
            # face = struct.unpack('HHH', file_contents[offset:offset_end])
            # offset += 28
            # faces.append([face[0], face[1], face[2]])
        # edges = []
        # new_mesh = bpy.data.meshes.new(f'{file_name}_mesh')
        # new_mesh.from_pydata(vertices, edges, faces)
        # new_mesh.update()
        # object_name = f'{file_name}'
        # if object_name in bpy.data.objects:
            # bpy.data.objects.remove(bpy.data.objects[object_name], do_unlink=True)
        # new_object = bpy.data.objects.new(object_name, new_mesh)
        # new_object.rotation_euler = (0, radians(180), 0)
        # return new_object

# import
# uncomment if needed

#if 'landscape' not in bpy.data.collections:
#    bpy.data.collections.new('landscape')
#    landscape = bpy.data.collections['landscape']
#    bpy.context.scene.collection.children.link(landscape)
#else:
#    landscape = bpy.data.collections['landscape']

#for file_path in glob('C:\\Games\\Sfera_std\\landscape\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_hr\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_ph\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_rd\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#if 'terrain' not in bpy.data.collections:
#    bpy.data.collections.new('terrain')
#    terrain = bpy.data.collections['terrain']
#    bpy.context.scene.collection.children.link(terrain)
#else:
#    terrain = bpy.data.collections['terrain']

#with open ('C:\\Games\\Sfera_std\\landscape\\map.bin', mode='rb') as mapfile:
#    file_contents = mapfile.read()
#    offset = 0
#    index = 0
#    while offset < len(file_contents):
#        name_end_offset = offset + 20
#        name_bytes = struct.unpack('20s', file_contents[offset:name_end_offset])[0]
#        name_chars = []
#        for i in range (len(name_bytes)):
#            if name_bytes[i] == 0:
#                break
#            name_chars.append(name_bytes[i])
#        name = str(bytes(name_chars), encoding='ascii').lower()
#        variant_1 = struct.unpack('B',file_contents[name_end_offset:(name_end_offset + 1)])[0]
#        variant_2 = struct.unpack('B',file_contents[(name_end_offset + 1):(name_end_offset + 2)])[0]
#        offset += 22
#        index += 1
#        object_name = f'{name}_{variant_1}{variant_2}'
#        object = bpy.data.objects.get(object_name, None)
#        if object is None:
#            # happens with patches
#            object_name = object_name.replace('patch', 'Patch')
#            object = bpy.data.objects.get(object_name, None)
#        x_coord = index % 80 * 100
#        y_coord = index // 80 * 100
#        z_coord = 0
#        clone = object.copy()
#        terrain.objects.link(clone)
#        clone.location.x = x_coord
#        clone.location.y = y_coord
#        clone.location.z = 0
#        clone.rotation_euler = (0, 0, radians(90))

print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
        
with open ('C:\\Games\\Sfera_std\\landscape\\castle1_00.lnd', mode='rb') as mskfile:
    file_contents = mskfile.read()
    test_offset = 12
    while test_offset < 0x1b20:
        t = struct.unpack('BBBBBBBBBBBB',file_contents[test_offset:(test_offset+12)])
#        print (f'{t[0]} {t[4]} {t[8]}')
        test_offset += 12
    offset = 0x1b20
    count = 0
    vertices = []
    while offset + 12 < 0x68c0:
        offset_end = offset + 12
        vertex = struct.unpack('fff',file_contents[offset:offset_end])
        offset += 12
        print(vertex)
        count += 1
        if count > 465:
            break
        #if (count %8 == 0):
#            print('----')
            #offset += 24
        # blender is Z up
        vertices.append([vertex[0], vertex[2], vertex[1]])
#    new_mesh = bpy.data.meshes.new('test_mesh')
#    new_mesh.from_pydata(vertices, [], [])
#    new_mesh.update()
#    new_object = bpy.data.objects.new('test_object', new_mesh)
#    print(count)
#    if 'landscape' not in bpy.data.collections:
#        bpy.data.collections.new('landscape')
#        landscape = bpy.data.collections['landscape']
#        bpy.context.scene.collection.children.link(landscape)
#    landscape = bpy.data.collections['landscape']
#    landscape.objects.link(new_object)