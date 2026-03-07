# -*- coding: utf-8 -*-
"""
分析图标文件与组件的映射关系
"""
from pathlib import Path

icons_dir = Path(r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons')

# 获取所有图标文件
icon_files = sorted([f.name for f in icons_dir.glob('*.png') if f.is_file()])

print('=' * 80)
print('图标文件列表')
print('=' * 80)
for i, icon in enumerate(icon_files, 1):
    print(f'{i:2}. {icon}')

print(f'\n总计: {len(icon_files)} 个图标文件')

# 根据图标文件名推断组件名称
icon_to_component = {
    'read_csv.png': 'ReadCSVComponent',
    'read_excel.png': 'ReadExcelComponent',
    'write_csv.png': 'WriteCSVComponent',
    'write_excel.png': 'WriteExcelComponent',
    'load_dataset.png': 'LoadDatasetComponent',
    'calculate_statistics.png': 'CalculateStatisticsComponent',
    'calculate correlation.png': 'CalculateCorrelationComponent',  # 注意有空格
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

print('\n' + '=' * 80)
print('图标文件与组件的映射关系')
print('=' * 80)

mapped_icons = []
unmapped_icons = []

for icon in icon_files:
    if icon in icon_to_component:
        component = icon_to_component[icon]
        mapped_icons.append((icon, component))
        print(f'✓ {icon:60} -> {component}')
    else:
        unmapped_icons.append(icon)
        print(f'⚠ {icon:60} -> (没有对应的组件)')

print('\n' + '=' * 80)
print('总结')
print('=' * 80)
print(f'已映射的图标: {len(mapped_icons)} 个')
print(f'未映射的图标: {len(unmapped_icons)} 个')

if unmapped_icons:
    print('\n未映射的图标文件（需要手动处理）：')
    for icon in unmapped_icons:
        print(f'  - {icon}')
