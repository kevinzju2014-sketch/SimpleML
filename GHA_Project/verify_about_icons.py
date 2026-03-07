# -*- coding: utf-8 -*-
"""
验证 About 和 InstallationGuide 组件的图标配置
"""
from pathlib import Path
import re

icons_dir = Path(__file__).parent / 'icons'
about_file = Path(__file__).parent / 'Components' / 'AboutComponent.cs'
install_file = Path(__file__).parent / 'Components' / 'InstallationGuideComponent.cs'
map_file = Path(__file__).parent / 'ComponentIconMap.cs'

print('=' * 80)
print('验证 About 和 InstallationGuide 组件图标配置')
print('=' * 80)

# 检查图标文件
print('\n1. 检查图标文件:')
about_icon = icons_dir / 'about.png'
install_icon = icons_dir / 'installation_guide.png'
print(f'   about.png: {"✓ 存在" if about_icon.exists() else "✗ 不存在"}')
print(f'   installation_guide.png: {"✓ 存在" if install_icon.exists() else "✗ 不存在"}')

# 检查 ComponentIconMap.cs 中的映射
print('\n2. 检查 ComponentIconMap.cs 中的映射:')
with open(map_file, 'r', encoding='utf-8') as f:
    map_content = f.read()

if '"AboutComponent", "about.png"' in map_content:
    print('   AboutComponent -> about.png: ✓ 已映射')
else:
    print('   AboutComponent -> about.png: ✗ 未映射')

if '"InstallationGuideComponent", "installation_guide.png"' in map_content:
    print('   InstallationGuideComponent -> installation_guide.png: ✓ 已映射')
else:
    print('   InstallationGuideComponent -> installation_guide.png: ✗ 未映射')

# 检查组件代码
print('\n3. 检查组件代码:')

# 检查 AboutComponent.cs
with open(about_file, 'r', encoding='utf-8') as f:
    about_content = f.read()

if 'using SimpleML.Core;' in about_content:
    print('   AboutComponent.cs: ✓ 已导入 SimpleML.Core')
else:
    print('   AboutComponent.cs: ✗ 未导入 SimpleML.Core')

if 'IconLoader.LoadComponentIcon(nameof(AboutComponent))' in about_content:
    print('   AboutComponent.cs: ✓ 已使用 IconLoader')
else:
    print('   AboutComponent.cs: ✗ 未使用 IconLoader')

# 检查 InstallationGuideComponent.cs
with open(install_file, 'r', encoding='utf-8') as f:
    install_content = f.read()

if 'using SimpleML.Core;' in install_content:
    print('   InstallationGuideComponent.cs: ✓ 已导入 SimpleML.Core')
else:
    print('   InstallationGuideComponent.cs: ✗ 未导入 SimpleML.Core')

if 'IconLoader.LoadComponentIcon(nameof(InstallationGuideComponent))' in install_content:
    print('   InstallationGuideComponent.cs: ✓ 已使用 IconLoader')
else:
    print('   InstallationGuideComponent.cs: ✗ 未使用 IconLoader')

print('\n' + '=' * 80)
print('验证完成')
print('=' * 80)
