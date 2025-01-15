import bpy
import math
import argparse

# Разбор аргументов командной строки
parser = argparse.ArgumentParser(description="Render an image in Blender")
parser.add_argument("--filepath", type=str, required=True, help="Output file path")
parser.add_argument("--modelpath", type=str, required=True, help="Path to the 3D model")
parser.add_argument("--texturepath", type=str, required=True, help="Path to the texture image")
parser.add_argument("--angle", type=float, required=False, help="Camera angle", default=45.0)
parser.add_argument("--light", type=float, required=False, help="Light intensity", default=300.0)
args = parser.parse_args()

# Проверьте полученные аргументы
print(f"Arguments received: filepath={args.filepath}, modelpath={args.modelpath}, texturepath={args.texturepath}, angle={args.angle}, light={args.light}")

# Очистка сцены
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Импорт модели
bpy.ops.import_scene.gltf(filepath=args.modelpath)

# Найти импортированную модель
model = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH'][0]

# Настройка текстуры
material = bpy.data.materials.new(name="CustomMaterial")
material.use_nodes = True
nodes = material.node_tree.nodes
links = material.node_tree.links

# Удаление стандартных узлов
for node in nodes:
    nodes.remove(node)

# Узлы
image_texture = nodes.new(type='ShaderNodeTexImage')
image_texture.image = bpy.data.images.load(args.texturepath)
principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
material_output = nodes.new(type='ShaderNodeOutputMaterial')

# Связи узлов
links.new(image_texture.outputs['Color'], principled_bsdf.inputs['Base Color'])
links.new(principled_bsdf.outputs['BSDF'], material_output.inputs['Surface'])

# Применение материала
if model.data.materials:
    model.data.materials[0] = material
else:
    model.data.materials.append(material)

# Камера
bpy.ops.object.camera_add(location=(0, 0, 0))
camera = bpy.context.object
bpy.context.scene.camera = camera

# Свет
bpy.ops.object.light_add(type='POINT', location=(0.6, 2.1, 1.5))
light = bpy.context.object
light.data.energy = args.light

# Радиус камеры
radius = 8
height = 1.5

# Позиция камеры
def set_camera_position(angle):
    x = radius * math.cos(math.radians(angle))
    y = radius * math.sin(math.radians(angle))
    camera.location = (x, y, height)
    camera.rotation_euler = (math.radians(90), 0, math.radians(90 - angle))

set_camera_position(args.angle)

# Рендеринг
bpy.context.scene.render.filepath = args.filepath
bpy.ops.render.render(write_still=True)
