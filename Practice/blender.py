import bpy
import math
import sys

# Аргументы
angle = float(sys.argv[-4])  # Угол
lightEnergy = float(sys.argv[-3])  # Освещённость
model_path = sys.argv[-2]  # Путь к модели
texture_path = sys.argv[-1]  # Путь к текстуре

# Очистка сцены
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Импорт модели
bpy.ops.import_scene.gltf(filepath=model_path)

# Найти модель
model = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH'][0]

# Создание материала
material = bpy.data.materials.new(name="CustomMaterial")
material.use_nodes = True

nodes = material.node_tree.nodes
links = material.node_tree.links

# Удаление стандартных узлов
for node in nodes:
    nodes.remove(node)

# Добавление текстуры
image_texture = nodes.new(type='ShaderNodeTexImage')
image_texture.image = bpy.data.images.load(texture_path)

principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
material_output = nodes.new(type='ShaderNodeOutputMaterial')

links.new(image_texture.outputs['Color'], principled_bsdf.inputs['Base Color'])
links.new(principled_bsdf.outputs['BSDF'], material_output.inputs['Surface'])

# Применение материала
if model.data.materials:
    model.data.materials[0] = material
else:
    model.data.materials.append(material)

# Камера и свет
bpy.ops.object.camera_add(location=(0, 0, 7))
camera = bpy.context.object
bpy.context.scene.camera = camera

bpy.ops.object.light_add(type='POINT', location=(0, 0, 5))
light = bpy.context.object
light.data.energy = lightEnergy

# Позиция камеры
camera.location = (7, 0, 1)
camera.rotation_euler = (math.radians(90), 0, math.radians(-angle))

# Рендеринг
bpy.context.scene.render.filepath = "C:/blender_render/photo.png"
bpy.ops.render.render(write_still=True)
