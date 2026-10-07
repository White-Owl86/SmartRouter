import os
from PIL import Image, ImageDraw

assets_dir = r"F:\Smart Anti-Sanction & Latency Router\src\SmartRouter.UI\Assets"
os.makedirs(assets_dir, exist_ok=True)

size = (256, 256)
img = Image.new("RGBA", size, (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Outer glow / background circle
# Gradient effect: Dark navy to deep cyan-blue
for r in range(120, 0, -1):
    alpha = int(255 * (r / 120))
    # gradient color from #0f172a to #1e293b
    ratio = r / 120.0
    red = int(15 + (30 - 15) * (1 - ratio))
    green = int(23 + (58 - 23) * (1 - ratio))
    blue = int(42 + (90 - 42) * (1 - ratio))
    draw.ellipse([128 - r, 128 - r, 128 + r, 128 + r], fill=(red, green, blue, 255))

# Outer cyber ring
draw.ellipse([18, 18, 238, 238], outline=(56, 189, 248, 200), width=4)
draw.ellipse([26, 26, 230, 230], outline=(168, 85, 247, 140), width=2)

# Central Shield shape
shield_points = [
    (128, 48),   # top center
    (198, 76),   # top right
    (198, 144),  # mid right
    (128, 208),  # bottom point
    (58, 144),   # mid left
    (58, 76),    # top left
]
draw.polygon(shield_points, fill=(17, 24, 39, 240), outline=(56, 189, 248, 255))
draw.polygon([(p[0], p[1]) for p in shield_points], outline=(147, 51, 234, 255))

# Inner lightning bolt / speed router icon
bolt_points = [
    (134, 68),
    (96, 134),
    (124, 134),
    (118, 188),
    (164, 120),
    (136, 120),
]
draw.polygon(bolt_points, fill=(56, 189, 248, 255))
# Inner accent highlight
draw.polygon([(p[0] + 1, p[1] + 1) for p in bolt_points], outline=(255, 255, 255, 180))

# Network node circles around shield
nodes = [(40, 60), (216, 60), (40, 196), (216, 196), (128, 228)]
for nx, ny in nodes:
    draw.ellipse([nx - 7, ny - 7, nx + 7, ny + 7], fill=(168, 85, 247, 255), outline=(255, 255, 255, 200), width=2)

png_path = os.path.join(assets_dir, "icon.png")
ico_path = os.path.join(assets_dir, "icon.ico")

img.save(png_path, "PNG")

# Generate multi-size ICO
icon_sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
img.save(ico_path, format="ICO", sizes=icon_sizes)

print(f"Saved {png_path} and {ico_path}")
