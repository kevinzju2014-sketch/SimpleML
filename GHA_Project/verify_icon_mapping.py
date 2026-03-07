# -*- coding: utf-8 -*-
"""
验证图标映射 - 检查about.png和installation_guide.png的映射
"""
from pathlib import Path
import re

icons_dir = Path(__file__).parent / 'icons'
component_map_file = Path(__file__).parent / 'ComponentIconMap.cs'

print('=' * 80)
print('验证图标映射')
print('=' * 80)

# 检查图标文件是否存在
about_icon = icons_dir / 'about.png'
install_guide_icon = icons_dir / 'installation_guide.png'

print(f'\n检查图标文件:')
print(f'  about.png: {"✓ 存在" if about_icon.exists() else "✗ 不存在"}')
print(f'  installation_guide.png: {"✓ 存在" if install_guide_icon.exists() else "✗ 不存在"}')

# 读取ComponentIconMap.cs
with open(component_map_file, 'r', encoding='utf-8') as f:
    content = f.read()

# 检查映射
print(f'\n检查映射关系:')
if '"AboutComponent", "about.png"' in content:
    print(f'  AboutComponent -> about.png: ✓ 已映射')
else:
    print(f'  AboutComponent -> about.png: ✗ 未映射')

if '"InstallationGuideComponent", "installation_guide.png"' in content:
    print(f'  InstallationGuideComponent -> installation_guide.png: ✓ 已映射')
else:
    print(f'  InstallationGuideComponent -> installation_guide.png: ✗ 未映射')

# 提取所有映射
pattern = r'\{\s*"(\w+Component)"\s*,\s*"([^"]+)"\s*\}'
matches = re.findall(pattern, content)

about_mapped = False
install_guide_mapped = False

for component_name, icon_name in matches:
    if component_name == 'AboutComponent' and icon_name == 'about.png':
        about_mapped = True
    if component_name == 'InstallationGuideComponent' and icon_name == 'installation_guide.png':
        install_guide_mapped = True

print(f'\n最终验证:')
print(f'  AboutComponent映射: {"✓ 正确" if about_mapped else "✗ 错误"}')
print(f'  InstallationGuideComponent映射: {"✓ 正确" if install_guide_mapped else "✗ 错误"}')

if about_mapped and install_guide_mapped:
    print(f'\n✅ 所有映射正确！')
else:
    print(f'\n⚠ 请检查映射关系')

print('=' * 80)
