import bpy
import math
import sys
import os

# Debug: Print all received arguments
print("Received arguments:", sys.argv)

# Parse arguments
try:
    angle_light = float(sys.argv[-6]) #6th
    angle_vertical = float(sys.argv[-5]) # 5th
    angle_horizontal = float(sys.argv[-4])         # 4th argument from the end
    lightEnergy = float(sys.argv[-3])  # 3rd argument from the end
    texture_path = sys.argv[-2]        # 2nd argument from the end
    output_path = sys.argv[-1]         # Last argument
except ValueError as e:
    print(f"Error parsing arguments: {e}")
    sys.exit(1)

# Debug: Print parsed arguments
#print(f"Angle: {angle}, Light Energy: {lightEnergy}")
#print(f"Texture Path: {texture_path}, Output Path: {output_path}")

# Verify paths
if not os.path.exists(texture_path):
    raise FileNotFoundError(f"Texture file not found: {texture_path}")
if not os.path.exists(os.path.dirname(output_path)):
    raise FileNotFoundError(f"Output directory does not exist: {os.path.dirname(output_path)}")

# Load .blend file
#bpy.ops.wm.open_mainfile(filepath="C:/Users/jenya/Downloads/Telegram Desktop/banka3ReadyToo.blend")

# Find the first mesh in the scene
model = next((obj for obj in bpy.context.scene.objects if obj.type == 'MESH'), None)
if not model:
    raise RuntimeError("No mesh object found in the scene.")

# Debug: Print model name
print(f"Using model: {model.name}")


# Настройка рендера для скорости
bpy.context.scene.cycles.samples = 64  # Количество сэмплов (можно уменьшить для скорости)
bpy.context.scene.cycles.use_adaptive_sampling = False  # Адаптивные сэмплы
bpy.context.scene.cycles.use_denoising = False  # Включаем шумоподавление
bpy.context.scene.cycles.use_fast_gi = False  # Ускоренное глобальное освещение
bpy.context.scene.render.resolution_x = 1024
bpy.context.scene.render.resolution_y = 1024
bpy.context.scene.render.resolution_percentage = 100



# Rotate the model
model.rotation_euler = (0, math.radians(angle_vertical), math.radians(angle_horizontal))  # Rotate around Z-axis

# Apply the texture
material = bpy.data.materials.new(name="CustomMaterial")
material.use_nodes = True

nodes = material.node_tree.nodes
links = material.node_tree.links

# Remove default nodes
for node in nodes:
    nodes.remove(node)

# Create texture nodes
image_texture = nodes.new(type='ShaderNodeTexImage')
image_texture.image = bpy.data.images.load(texture_path)

principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
material_output = nodes.new(type='ShaderNodeOutputMaterial')

# Link nodes
links.new(image_texture.outputs['Color'], principled_bsdf.inputs['Base Color'])
links.new(principled_bsdf.outputs['BSDF'], material_output.inputs['Surface'])

# Assign material to the model
if model.data.materials:
    model.data.materials[0] = material
else:
    model.data.materials.append(material)

# Делаем сцену активной
bpy.context.view_layer.objects.active = None
bpy.ops.object.select_all(action='DESELECT')

# Set up camera
camera_location = (8, 0, 1)
bpy.ops.object.camera_add(location=camera_location)
camera = bpy.context.object
bpy.context.scene.camera = camera

# Point camera at the model
direction = model.location - camera.location
camera.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()



# Радиус окружности для света
light_radius = 8  

# Вычисляем новую позицию света на круге
light_x = light_radius * math.cos(math.radians(angle_light))
light_y = light_radius * math.sin(math.radians(angle_light))
light_z = 5  # Высота света


# Добавляем сферический источник (POINT)
bpy.ops.object.light_add(type='POINT', location=(light_x, light_y, light_z))
light = bpy.context.object

#light.rotation_euler = (0, 0, math.radians(angle_light))  # Поворот света

light.data.energy = lightEnergy * 25  # Set light energy
light.data.use_shadow = True  # Отключаем тени для ускорения



# Configure render output
bpy.context.scene.render.filepath = output_path
bpy.context.scene.render.image_settings.file_format = 'PNG'

# Render the scene
bpy.ops.render.render(write_still=True)
print(f"Rendered image saved to {output_path}")
