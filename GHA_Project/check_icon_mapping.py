# -*- coding: utf-8 -*-
"""
检查图标文件与组件名称的映射关系
"""
from pathlib import Path
import re

icons_dir = Path(__file__).parent / 'icons'
components_dir = Path(__file__).parent / 'Components'

# 获取所有图标文件
icon_files = {f.name.lower(): f.name for f in icons_dir.glob('*.png') if f.is_file()}

# 获取所有组件文件
component_files = list(components_dir.glob('*.cs'))

# 从ComponentIconMap.cs中提取映射关系
icon_map_file = Path(__file__).parent / 'ComponentIconMap.cs'
with open(icon_map_file, 'r', encoding='utf-8') as f:
    content = f.read()

# 提取映射关系
pattern = r'\{\s*"(\w+Component)"\s*,\s*"([^"]+)"\s*\}'
matches = re.findall(pattern, content)

component_to_icon = {}
icon_to_components = {}

for component_name, icon_name in matches:
    component_to_icon[component_name] = icon_name
    if icon_name not in icon_to_components:
        icon_to_components[icon_name] = []
    icon_to_components[icon_name].append(component_name)

print('=' * 80)
print('图标文件与组件映射关系检查')
print('=' * 80)

print(f'\n图标目录中的文件数量: {len(icon_files)}')
print(f'组件数量: {len(component_to_icon)}')
print(f'映射的图标文件名数量: {len(icon_to_components)}')

# 检查每个组件需要的图标文件是否存在
print('\n' + '=' * 80)
print('1. 检查组件需要的图标文件是否存在')
print('=' * 80)

missing_icons = []
for component_name, icon_name in component_to_icon.items():
    icon_lower = icon_name.lower()
    if icon_lower not in icon_files:
        missing_icons.append((component_name, icon_name))
        print(f'✗ {component_name} -> {icon_name} (图标文件不存在)')
    else:
        actual_name = icon_files[icon_lower]
        if actual_name != icon_name:
            print(f'⚠ {component_name} -> {icon_name} (实际文件: {actual_name})')

if not missing_icons:
    print('✓ 所有组件需要的图标文件都存在')

# 检查哪些图标文件没有对应的组件
print('\n' + '=' * 80)
print('2. 检查哪些图标文件没有对应的组件')
print('=' * 80)

# 获取所有需要的图标文件名（小写）
required_icons_lower = {icon.lower() for icon in icon_to_components.keys()}

unused_icons = []
for icon_lower, icon_name in icon_files.items():
    if icon_lower not in required_icons_lower:
        unused_icons.append(icon_name)
        print(f'⚠ {icon_name} (没有对应的组件)')

if not unused_icons:
    print('✓ 所有图标文件都有对应的组件')

# 根据图标文件名推断应该对应的组件
print('\n' + '=' * 80)
print('3. 根据图标文件名推断应该对应的组件（按命令名称）')
print('=' * 80)

# 图标文件名到组件名称的映射规则
icon_to_component_map = {
    'read_csv.png': 'ReadCSVComponent',
    'read_excel.png': 'ReadExcelComponent',
    'write_csv.png': 'WriteCSVComponent',
    'write_excel.png': 'WriteExcelComponent',
    'load_dataset.png': 'LoadDatasetComponent',
    'calculate_statistics.png': 'CalculateStatisticsComponent',
    'calculate correlation.png': 'CalculateCorrelationComponent',  # 注意空格
    'get_data_summary.png': 'GetDataSummaryComponent',
    'describe_features.png': 'DescribeFeaturesComponent',
    'create_dataset.png': 'CreateDatasetComponent',
    'deconstruct_dataset.png': 'DeconstructDatasetComponent',
    'split_data.png': 'SplitDatasetComponent',
    'train_classifier.png': 'TrainClassifierComponent',
    'train_regressor.png': 'TrainRegressorComponent',
    'train_cluster.png': 'TrainClusterComponent',
    'random_forest_classifier.png': 'TrainRandomForestClassifierComponent',
    'support_vector_machine_classifier.png': 'TrainSVMClassifierComponent',
    'k_nearest_neighbors_classifier.png': 'TrainKNNClassifierComponent',
    'logistic_regression.png': 'TrainLogisticRegressionClassifierComponent',
    'naive_bayes_classifier.png': 'TrainNaiveBayesClassifierComponent',
    'decision_tree_classifier.png': 'TrainDecisionTreeClassifierComponent',
    'random_forest_regressor.png': 'TrainRandomForestRegressorComponent',
    'support_vector_regression.png': 'TrainSVRComponent',
    'linear_regression.png': 'TrainLinearRegressionComponent',
    'ridge_regression.png': 'TrainRidgeRegressionComponent',
    'lasso_regression.png': 'TrainLassoRegressionComponent',
    'k_nearest_neighbors_regressor.png': 'TrainKNNRegressorComponent',
    'k_means.png': 'TrainKMeansComponent',
    'density_baised_spatial_clistering_of_applications_with_noise.png': 'TrainDBSCANComponent',
    'agglomerative_clustering.png': 'TrainAgglomerativeClusteringComponent',
    'predict_classifier.png': 'PredictClassifierComponent',
    'predict_regressor.png': 'PredictRegressorComponent',
    'predict_cluster.png': 'PredictClusterComponent',
    'evaluate_classification.png': 'EvaluateClassificationComponent',
    'evaluate_regression.png': 'EvaluateRegressionComponent',
    'evaluate_clustering.png': 'EvaluateClusteringComponent',
    'save_model.png': 'SaveModelComponent',
    'load_model.png': 'LoadModelComponent',
}

print('\n建议的映射关系（按图标文件名直接对应组件）：')
print('-' * 80)

for icon_name, expected_component in icon_to_component_map.items():
    icon_lower = icon_name.lower()
    if icon_lower in icon_files:
        actual_icon = icon_files[icon_lower]
        current_icon = component_to_icon.get(expected_component, '未映射')
        if current_icon.lower() != icon_lower:
            print(f'{expected_component:45} 当前: {current_icon:40} 应该: {actual_icon}')

# 检查About和InstallationGuide组件
print('\n' + '=' * 80)
print('4. 特殊组件检查')
print('=' * 80)

special_components = ['AboutComponent', 'InstallationGuideComponent']
for comp in special_components:
    if comp not in component_to_icon:
        print(f'⚠ {comp} 没有在ComponentIconMap.cs中映射（可能需要添加图标）')

print('\n' + '=' * 80)
print('总结')
print('=' * 80)
print(f'缺失的图标文件: {len(missing_icons)} 个')
print(f'未使用的图标文件: {len(unused_icons)} 个')
if unused_icons:
    print('\n未使用的图标文件列表:')
    for icon in sorted(unused_icons):
        print(f'  - {icon}')
