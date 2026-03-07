"""
批量修复所有训练组件的Python导入顺序
"""
import re
from pathlib import Path

components_dir = Path(__file__).parent / "Components"

# 标准的路径设置代码块
path_setup_template = """# -*- coding: utf-8 -*-
import sys
import os
import json
import site
import io
import pickle
import base64

# 设置标准输出编码为UTF-8，避免中文输出错误
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if sys.stderr.encoding != 'utf-8':
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# 添加项目路径
sys.path.insert(0, r'{mymlPath}')

# 确保Rhino Python的site-packages在路径中
try:
    site_packages = site.getsitepackages()
    for sp in site_packages:
        if sp not in sys.path:
            sys.path.insert(0, sp)
    
    rhino_site_envs = r'C:\\Users\\Administrator\\.rhinocode\\py39-rh8\\site-envs'
    if os.path.exists(rhino_site_envs):
        for item in os.listdir(rhino_site_envs):
            env_path = os.path.join(rhino_site_envs, item)
            if os.path.isdir(env_path):
                if env_path not in sys.path:
                    sys.path.insert(0, env_path)
                site_pkg = os.path.join(env_path, 'Lib', 'site-packages')
                if os.path.exists(site_pkg) and site_pkg not in sys.path:
                    sys.path.insert(0, site_pkg)
except Exception as e:
    pass

"""

# 需要修复的训练组件文件
train_components = [
    "TrainKMeansComponent.cs",
    "TrainDBSCANComponent.cs",
    "TrainAgglomerativeClusteringComponent.cs",
    "TrainRandomForestClassifierComponent.cs",
    "TrainRandomForestRegressorComponent.cs",
    "TrainSVMClassifierComponent.cs",
    "TrainSVRComponent.cs",
    "TrainLinearRegressionComponent.cs",
    "TrainRidgeRegressionComponent.cs",
    "TrainLassoRegressionComponent.cs",
    "TrainKNNClassifierComponent.cs",
    "TrainKNNRegressorComponent.cs",
    "TrainLogisticRegressionClassifierComponent.cs",
    "TrainNaiveBayesClassifierComponent.cs",
    "TrainDecisionTreeClassifierComponent.cs",
]

updated_count = 0

for filename in train_components:
    file_path = components_dir / filename
    if not file_path.exists():
        print(f"跳过: {filename} (文件不存在)")
        continue
    
    content = file_path.read_text(encoding='utf-8')
    original_content = content
    
    # 查找Python代码块
    python_code_match = re.search(
        r'string pythonCode = \$@"([^"]*(?:"")?[^"]*)"',
        content,
        re.DOTALL
    )
    
    if not python_code_match:
        print(f"跳过: {filename} (未找到Python代码)")
        continue
    
    python_code = python_code_match.group(1)
    
    # 检查是否已经修复
    if '# -*- coding: utf-8 -*-' in python_code and 'rhino_site_envs' in python_code:
        print(f"跳过: {filename} (已修复)")
        continue
    
    # 提取from导入语句
    from_imports = re.findall(r'^from\s+[\w\.]+\s+import.*$', python_code, re.MULTILINE)
    
    # 构建新的Python代码
    new_python_code = path_setup_template.format(mymlPath="{mymlPath}")
    
    # 添加from导入
    for from_import in from_imports:
        new_python_code += from_import + "\n"
    
    # 提取业务逻辑（从第一个非导入语句开始）
    # 找到第一个非导入、非注释的行
    lines = python_code.split('\n')
    start_idx = 0
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped and not stripped.startswith('#') and not stripped.startswith('import') and not stripped.startswith('from'):
            start_idx = i
            break
    
    # 添加业务逻辑
    business_logic = '\n'.join(lines[start_idx:])
    new_python_code += "\n" + business_logic
    
    # 替换Python代码块
    new_content = content.replace(
        f'string pythonCode = $@"{python_code}"',
        f'string pythonCode = $@"{new_python_code}"'
    )
    
    if new_content != original_content:
        file_path.write_text(new_content, encoding='utf-8')
        updated_count += 1
        print(f"已修复: {filename}")

print(f"\n完成！更新了 {updated_count} 个文件。")
