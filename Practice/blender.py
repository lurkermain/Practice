import bpy
import math
import sys

angle = float(sys.argv[-3])  # Угол для снимков
lightEnergy = float(sys.argv[-2])  # Сила освещения
texture_path = sys.argv[-1]  # Путь к текстуре

# Очистка сцены
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Импорт модели
bpy.ops.import_scene.gltf(filepath="C:/blender_fruto/front_fruto_nyanya_half.gltf")

# Найти импортированную модель
model = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH'][0]

# Создать новый материал
material = bpy.data.materials.new(name="FrontImageMaterial")
material.use_nodes = True

# Узлы материала
nodes = material.node_tree.nodes
links = material.node_tree.links

# Удалить стандартные узлы
for node in nodes:
    nodes.remove(node)

# Добавить узел текстуры изображения
image_texture = nodes.new(type='ShaderNodeTexImage')
image_texture.image = bpy.data.images.load(texture_path)

# Добавить узел шейдера Principled BSDF
principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')

# Добавить узел вывода материала
material_output = nodes.new(type='ShaderNodeOutputMaterial')

# Связать узлы
links.new(image_texture.outputs['Color'], principled_bsdf.inputs['Base Color'])
links.new(principled_bsdf.outputs['BSDF'], material_output.inputs['Surface'])

# Применить материал к модели
if model.data.materials:
    model.data.materials[0] = material
else:
    model.data.materials.append(material)

# Создать камеру
bpy.ops.object.camera_add(location=(0, 0, 0))
camera = bpy.context.object
bpy.context.scene.camera = camera

# Создать точечный источник света
bpy.ops.object.light_add(type='POINT', location=(0, 0, 0))
light = bpy.context.object
light.data.energy = lightEnergy  # Настройка мощности света

# Радиус орбиты камеры
radius = 7
height = 1  # Высота камеры над землёй

# Функция для установки позиции камеры
def set_camera_position(angle, is_left):
    direction = -1 if is_left else 1  # Для левого и правого угла
    x = radius * math.cos(math.radians(angle))
    y = radius * math.sin(math.radians(angle)) * direction
    camera.location = (x, y, height)
    camera.rotation_euler = (
        math.radians(90),  # Камера смотрит горизонтально
        0,
        math.radians(90 - angle) if is_left else math.radians(90 + angle)
    )

# Установить положение источника света
light.location = (0, -5, 1)

# Рендеринг фотографий
output_path = "C:/blender_render/"

# Создаём кадр
set_camera_position(angle, is_left=True)
bpy.context.scene.render.filepath = f"{output_path}photo.png"
bpy.ops.render.render(write_still=True)
