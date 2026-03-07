# -*- coding: utf-8 -*-
"""
验证最终配置 - 检查所有图标文件和映射关系
"""
from pathlib import Path
import re

icons_dir = Path(__file__).parent / 'icons'
component_map_file = Path(__file__).parent / 'ComponentIconMap.cs'

print('=' * 80)
print('最终配置验证')
print('=' * 80)

# 获取所有图标文件
icon_files = {f.name.lower(): f.name for f in icons_dir.glob('*.png') if f.is_file()}
print(f'\n图标目录中的文件数量: {len(icon_files)}')

# 读取ComponentIconMap.cs
with open(component_map_file, 'r', encoding='utf-8') as f:
    content = f.read()

# 提取映射关系
pattern = r'\{\s*"(\w+Component)"\s*,\s*"([^"]+)"\s*\}'
matches = re.findall(pattern, content)

component_to_icon = {}
for component_name, icon_name in matches:
    component_to_icon[component_name] = icon_name

print(f'ComponentIconMap.cs中的映射数量: {len(component_to_icon)}')

# 验证每个映射的图标文件是否存在
print('\n' + '=' * 80)
print('验证图标文件是否存在')
print('=' * 80)

missing_icons = []
for component_name, icon_name in component_to_icon.items():
    icon_lower = icon_name.lower()
    if icon_lower in icon_files:
        actual_name = icon_files[icon_lower]
        if actual_name == icon_name:
            print(f'✓ {component_name:45} -> {icon_name}')
        else:
            print(f'⚠ {component_name:45} -> {icon_name} (实际: {actual_name})')
    else:
        print(f'✗ {component_name:45} -> {icon_name} (文件不存在)')
        missing_icons.append((component_name, icon_name))

if missing_icons:
    print(f'\n⚠ 警告：{len(missing_icons)} 个图标文件不存在')
else:
    print('\n✓ 所有映射的图标文件都存在')

# 检查是否有未映射的图标文件
print('\n' + '=' * 80)
print('检查未映射的图标文件')
print('=' * 80)

mapped_icons_lower = {icon.lower() for icon in component_to_icon.values()}
unmapped_icons = [name for name_lower, name in icon_files.items() if name_lower not in mapped_icons_lower]

if unmapped_icons:
    print(f'\n⚠ 有 {len(unmapped_icons)} 个图标文件未映射到组件:')
    for icon in sorted(unmapped_icons):
        print(f'  - {icon}')
else:
    print('\n✓ 所有图标文件都已映射到组件')

# 总结
print('\n' + '=' * 80)
print('总结')
print('=' * 80)
print(f'图标文件总数: {len(icon_files)}')
print(f'组件映射总数: {len(component_to_icon)}')
print(f'缺失的图标文件: {len(missing_icons)}')
print(f'未映射的图标文件: {len(unmapped_icons)}')

if not missing_icons and not unmapped_icons:
    print('\n✅ 所有配置正确！可以开始构建GHA文件了！')
else:
    print('\n⚠ 请检查上述问题后再构建GHA文件')

print('=' * 80)
