import bpy
import math
import sys
import os

# Debug: Print all received arguments
print("Received arguments:", sys.argv)

# Parse arguments
try:
    angle = float(sys.argv[-5])         # 5th argument from the end
    lightEnergy = float(sys.argv[-4])  # 4th argument from the end
    model_path = sys.argv[-3]          # 3rd argument from the end
    texture_path = sys.argv[-2]        # 2nd argument from the end
    output_path = sys.argv[-1]         # Last argument
except ValueError as e:
    print(f"Error parsing arguments: {e}")
    sys.exit(1)

# Debug: Print parsed arguments
print(f"Angle: {angle}, Light Energy: {lightEnergy}")
print(f"Model Path: {model_path}, Texture Path: {texture_path}, Output Path: {output_path}")

# Verify paths
if not os.path.exists(model_path):
    raise FileNotFoundError(f"Model file not found: {model_path}")

# Check if a .bin file exists alongside the .gltf file
bin_path = model_path.replace(".gltf", ".bin")
if not os.path.exists(bin_path):
    raise FileNotFoundError(f"BIN file not found: {bin_path}")

if not os.path.exists(texture_path):
    raise FileNotFoundError(f"Texture file not found: {texture_path}")
if not os.path.exists(os.path.dirname(output_path)):
    raise FileNotFoundError(f"Output directory does not exist: {os.path.dirname(output_path)}")

# Clear the scene
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Import the model
print(f"Importing model: {model_path}")

# Ensure .bin is in the same directory as .gltf
bin_path = model_path.replace(".gltf", ".bin")
if not os.path.exists(bin_path):
    raise FileNotFoundError(f"Missing resource file: {bin_path}")

bpy.ops.import_scene.gltf(filepath=model_path)

# Find the first mesh
model = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH'][0]

# Apply the texture
print("Applying texture...")
material = bpy.data.materials.new(name="CustomMaterial")
material.use_nodes = True

nodes = material.node_tree.nodes
links = material.node_tree.links

# Remove default nodes
for node in nodes:
    nodes.remove(node)

# Create nodes
image_texture = nodes.new(type='ShaderNodeTexImage')
image_texture.image = bpy.data.images.load(texture_path)

principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
material_output = nodes.new(type='ShaderNodeOutputMaterial')

# Link nodes
links.new(image_texture.outputs['Color'], principled_bsdf.inputs['Base Color'])
links.new(principled_bsdf.outputs['BSDF'], material_output.inputs['Surface'])

# Assign material to model
if model.data.materials:
    model.data.materials[0] = material
else:
    model.data.materials.append(material)

# Set up camera
print("Setting up camera...")
bpy.ops.object.camera_add(location=(7, 0, 1))
camera = bpy.context.object
bpy.context.scene.camera = camera
camera.rotation_euler = (math.radians(90), 0, math.radians(-angle))

# Add light
print("Adding light...")
bpy.ops.object.light_add(type='POINT', location=(0, 0, 5))
light = bpy.context.object
light.data.energy = lightEnergy

# Configure render output
print("Configuring render output...")
bpy.context.scene.render.filepath = output_path
bpy.context.scene.render.image_settings.file_format = 'PNG'

# Render the scene
print(f"Rendering to {output_path}...")
bpy.ops.render.render(write_still=True)
print(f"Rendered image saved to {output_path}")
