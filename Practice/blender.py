import bpy
import math

# Очистка сцены
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Импорт модели
bpy.ops.import_scene.gltf(filepath="C:\\fruto\\frutonya_2.gltf")

# Создать камеру
bpy.ops.object.camera_add(location=(0, 0, 0))
camera = bpy.context.object
# Установить созданную камеру как активную
bpy.context.scene.camera = camera

# Создать точечный источник света
bpy.ops.object.light_add(type='POINT', location=(0, 0, 0))
light = bpy.context.object
light.data.energy = 300  # Настройка мощности света

# Найти импортированную модель
model = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH'][0]

# Радиус орбиты камеры
radius = 2.0
height = 0.57  # Высота камеры над землёй

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
light.location = (0.6, 2.1, 1.5)

# Рендеринг фотографий
output_path = "C:\\render\\"
angles = [45, 90, 135, 180, 225]  # Углы для снимков

for i, angle in enumerate(angles):
    # Снимки слева
    set_camera_position(angle, is_left=True)
    bpy.context.scene.render.filepath = f"{output_path}left_{i+1}.png"
    bpy.ops.render.render(write_still=True)

    # Снимки справа
    set_camera_position(angle, is_left=False)
    bpy.context.scene.render.filepath = f"{output_path}right_{i+1}.png"
    bpy.ops.render.render(write_still=True)
