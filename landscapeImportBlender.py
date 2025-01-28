import bpy
import struct
from math import radians
from glob import glob
import os
from pathlib import Path

#bpy.ops.wm.console_toggle()

    
def import_file (file_path):
    file_name = Path(file_path).stem
    with open (file_path, mode='rb') as file:
        file_contents = file.read()
#        vertex_count = struct.unpack('H',file_contents[4:6])
#        offset = 0x68c0
#        vertices = []
#        faces = []
#        for i in range (vertex_count[0]):
#            offset_end = offset + 32
#            vertex = struct.unpack('ffffffff',file_contents[offset:offset_end])
#            offset += 40
#            # blender is Z up
#            vertices.append([vertex[0], vertex[2], vertex[1]])
#        while offset < len(file_contents):
#            offset_end = offset + 6
#            face = struct.unpack('HHH', file_contents[offset:offset_end])
#            offset += 28
#            faces.append([face[0], face[1], face[2]])
#        edges = []
#        new_mesh = bpy.data.meshes.new(f'{file_name}_mesh')
#        new_mesh.from_pydata(vertices, edges, faces)
#        new_mesh.update()
#        object_name = f'{file_name}'
#        if object_name in bpy.data.objects:
#            bpy.data.objects.remove(bpy.data.objects[object_name], do_unlink=True)
#        new_object = bpy.data.objects.new(object_name, new_mesh)
#        new_object.rotation_euler = (0, radians(180), 0)
#        return new_object
    
    
        offset = 0x1b20
        count = 0
        vertices = []
#        vertex_dedup = {}
        vertex_dedup2 = {}
        while offset + 12 < 0x68c0:
            offset_end = offset + 12
            vertex = struct.unpack('fff',file_contents[offset:offset_end])
            offset += 12
            count += 1
            x = vertex[0]
            y = 160 - vertex[1]
            if y == 160:
                y = 0
            z = 100 - vertex[2]
#            if x not in vertex_dedup:
#                vertex_dedup[x] = {}
#            if z not in vertex_dedup[x]:
#                vertex_dedup[x][z] = y
            if x not in vertex_dedup2:
                vertex_dedup2[x] = {}
            vertex_dedup2[x][z] = y
            if (count %8 == 0):
    #            print('----')
                offset += 24
    #        if count > 100:
    #            break
            # blender is Z up
    #        vertices.append([vertex[0], vertex[2], vertex[1]])
    #    print(vertex_dedup)

#        dedup_count = 0
#        for x, z_to_y in sorted(vertex_dedup.items(), key = lambda el: el[0]):
#            for z, y in sorted(z_to_y.items(), key = lambda el: el[0]):
#                print(f'{x} {y} {z}')
#                vertices.append([x, -z, y])
#                dedup_count += 1
#        print (dedup_count)
#        vertex_offset = 0x68c0 + 40 * vertex_count[0]
#        faces = []
#        for i in range (12):
#            for j in range (0, 12):
#                faces.append([i+j*13, i+j*13+1, i+j*13+13])
#                faces.append([i+j*13+1, i+j*13+13, i+j*13+14])
#    #        faces.append([i, i+1, i+13])
#    #        faces.append([i+1, i+13, i+14])
#        print (faces)
#        new_mesh = bpy.data.meshes.new('test_mesh')
#        new_mesh.from_pydata(vertices, [], faces)
#        new_mesh.update()
#        new_object = bpy.data.objects.new('test_object', new_mesh)
#        print(count)
#        if 'landscape' not in bpy.data.collections:
#            bpy.data.collections.new('landscape')
#            landscape = bpy.data.collections['landscape']
#            bpy.context.scene.collection.children.link(landscape)
#        landscape = bpy.data.collections['landscape']
#        landscape.objects.link(new_object)
        
        vertices2 = []
    
        for x, z_to_y in sorted(vertex_dedup2.items(), key = lambda el: el[0]):
            for z, y in sorted(z_to_y.items(), key = lambda el: el[0]):
#                print(f'{x} {y} {z}')
                vertices2.append([x, -z, y])
    #        dedup_count += 1
    #    print (dedup_count)
    #    vertex_offset = 0x68c0 + 40 * vertex_count[0]
        faces2 = []
        for i in range (12):
            for j in range (0, 12):
                faces2.append([i+j*13, i+j*13+1, i+j*13+13])
                faces2.append([i+j*13+1, i+j*13+13, i+j*13+14])
    #        faces.append([i, i+1, i+13])
    #        faces.append([i+1, i+13, i+14])

        edges = []
        new_mesh = bpy.data.meshes.new(f'{file_name}_mesh')
        new_mesh.from_pydata(vertices2, edges, faces2)
        new_mesh.update()
        object_name = f'{file_name}'
        if object_name in bpy.data.objects:
            bpy.data.objects.remove(bpy.data.objects[object_name], do_unlink=True)
        new_object = bpy.data.objects.new(object_name, new_mesh)
        return new_object
# import
# uncomment if needed

if 'landscape' not in bpy.data.collections:
    bpy.data.collections.new('landscape')
    landscape = bpy.data.collections['landscape']
    bpy.context.scene.collection.children.link(landscape)
else:
    landscape = bpy.data.collections['landscape']

for file_path in glob('C:\\Games\\Sfera_std\\landscape\\*.lnd'):
    if file_path.find ("patch") != -1 or file_path.find ("Patch") != -1:
        continue
#    if file_path.find ('atch_') != -1:
#        continue
    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_hr\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_ph\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

#for file_path in glob('C:\\Games\\Sfera_std\\landscape_rd\\*.lnd'):    
#    landscape.objects.link(import_file(file_path))

if 'terrain' not in bpy.data.collections:
    bpy.data.collections.new('terrain')
    terrain = bpy.data.collections['terrain']
    bpy.context.scene.collection.children.link(terrain)
else:
    terrain = bpy.data.collections['terrain']

with open ('C:\\Games\\Sfera_std\\landscape\\map.bin', mode='rb') as mapfile:
    file_contents = mapfile.read()
    offset = 0
    index = 0
    while offset < len(file_contents):
        name_end_offset = offset + 20
        name_bytes = struct.unpack('20s', file_contents[offset:name_end_offset])[0]
        name_chars = []
        for i in range (len(name_bytes)):
            if name_bytes[i] == 0:
                break
            name_chars.append(name_bytes[i])
        name = str(bytes(name_chars), encoding='ascii').lower()
        variant_1 = struct.unpack('B',file_contents[name_end_offset:(name_end_offset + 1)])[0]
        variant_2 = struct.unpack('B',file_contents[(name_end_offset + 1):(name_end_offset + 2)])[0]
        offset += 22
        index += 1
        if name.find('fill_empt') != -1:
            continue
        object_name = f'{name}_{variant_1}{variant_2}'
        object = bpy.data.objects.get(object_name, None)
        if object is None:
            # happens with patches
            object_name = object_name.replace('patch', 'Patch')
            object = bpy.data.objects.get(object_name, None)
        if object is None:
            continue
        x_coord = index % 80 * 100
        y_coord = index // 80 * 100
        z_coord = 0
        clone = object.copy()
        terrain.objects.link(clone)
        clone.location.x = x_coord
        clone.location.y = y_coord
        clone.location.z = 0
        clone.rotation_euler = (0, 0, radians(90))

#landscape.objects.link(import_file('C:\\Games\\Sfera_std\\landscape\\mount1a_00.lnd'))

print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
print ('--------------------------------------------')
        
#with open ('C:\\Games\\Sfera_std\\landscape\\mount1a_00.lnd', mode='rb') as mskfile:
#    file_contents = mskfile.read()
#    vertex_count = struct.unpack('H',file_contents[4:6])
#    test_offset = 12
##    while test_offset < 0x1b20:
##        t = struct.unpack('BBBBBBBBBBBB',file_contents[test_offset:(test_offset+12)])
###        print (f'{t[0]} {t[4]} {t[8]}')
##        test_offset += 12
#    offset = 0x1b20
#    count = 0
#    vertices = []
#    vertex_dedup = {}
#    vertex_dedup2 = {}
#    while offset + 12 < 0x68c0:
#        offset_end = offset + 12
#        vertex = struct.unpack('fff',file_contents[offset:offset_end])
#        offset += 12
#        print(vertex)
#        count += 1
#        x = vertex[0]
#        y = 160 - vertex[1]
#        if y == 160:
#            y = 0
#        z = vertex[2]
#        if x not in vertex_dedup:
#            vertex_dedup[x] = {}
#        if z not in vertex_dedup[x]:
#            vertex_dedup[x][z] = y
#        if x not in vertex_dedup2:
#            vertex_dedup2[x] = {}
#        vertex_dedup2[x][z] = y
#        if (count %8 == 0):
##            print('----')
#            offset += 24
##        if count > 100:
##            break
#        # blender is Z up
##        vertices.append([vertex[0], vertex[2], vertex[1]])
##    print(vertex_dedup)

#    dedup_count = 0
#    for x, z_to_y in sorted(vertex_dedup.items(), key = lambda el: el[0]):
#        for z, y in sorted(z_to_y.items(), key = lambda el: el[0]):
#            print(f'{x} {y} {z}')
#            vertices.append([x, -z, y])
#            dedup_count += 1
#    print (dedup_count)
#    vertex_offset = 0x68c0 + 40 * vertex_count[0]
#    faces = []
#    for i in range (12):
#        for j in range (0, 12):
#            faces.append([i+j*13, i+j*13+1, i+j*13+13])
#            faces.append([i+j*13+1, i+j*13+13, i+j*13+14])
##        faces.append([i, i+1, i+13])
##        faces.append([i+1, i+13, i+14])
#    print (faces)
#    new_mesh = bpy.data.meshes.new('test_mesh')
#    new_mesh.from_pydata(vertices, [], faces)
#    new_mesh.update()
#    new_object = bpy.data.objects.new('test_object', new_mesh)
#    print(count)
#    if 'landscape' not in bpy.data.collections:
#        bpy.data.collections.new('landscape')
#        landscape = bpy.data.collections['landscape']
#        bpy.context.scene.collection.children.link(landscape)
#    landscape = bpy.data.collections['landscape']
#    landscape.objects.link(new_object)
#    
#    vertices2 = []
#    
#    for x, z_to_y in sorted(vertex_dedup2.items(), key = lambda el: el[0]):
#        for z, y in sorted(z_to_y.items(), key = lambda el: el[0]):
#            print(f'{x} {y} {z}')
#            vertices2.append([x, -z, y])
##        dedup_count += 1
##    print (dedup_count)
##    vertex_offset = 0x68c0 + 40 * vertex_count[0]
#    faces2 = []
#    for i in range (12):
#        for j in range (0, 12):
#            faces2.append([i+j*13, i+j*13+1, i+j*13+13])
#            faces2.append([i+j*13+1, i+j*13+13, i+j*13+14])
##        faces.append([i, i+1, i+13])
##        faces.append([i+1, i+13, i+14])
#    print (faces2)
#    new_mesh2 = bpy.data.meshes.new('test_mesh2')
#    new_mesh2.from_pydata(vertices2, [], faces2)
#    new_mesh2.update()
#    new_object2 = bpy.data.objects.new('test_object2', new_mesh2)
#    new_object2.location.x = 200
#    landscape.objects.link(new_object2)