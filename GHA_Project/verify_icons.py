# -*- coding: utf-8 -*-
"""
验证图标文件是否与ComponentIconMap.cs匹配
"""
import os

icons_dir = os.path.join(os.path.dirname(__file__), 'icons')

# 根据ComponentIconMap.cs，需要的所有图标文件
required_files = {
    'file_io_components.png',
    'statistics_components.png',
    'dataset_components.png',
    'train_components.png',
    'predict_components.png',
    'evaluate_components.png',
    'model_io_components.png',
    'dataset_loader.png',
    'train_random_forest_classifier.png',
    'train_svm_classifier.png',
    'train_knn_classifier.png',
    'train_logistic_regression_classifier.png',
    'train_naive_bayes_classifier.png',
    'train_decision_tree_classifier.png',
    'train_random_forest_regressor.png',
    'train_svr.png',
    'train_linear_regression.png',
    'train_ridge_regression.png',
    'train_lasso_regression.png',
    'train_knn_regressor.png',
    'train_kmeans.png',
    'train_dbscan.png',
    'train_agglomerative_clustering.png',
}

print('=' * 70)
print('图标文件验证')
print('=' * 70)

if not os.path.exists(icons_dir):
    print(f'错误：图标目录不存在: {icons_dir}')
    exit(1)

# 获取当前文件（大小写不敏感）
current_files = {f.lower(): f for f in os.listdir(icons_dir) if f.endswith('.png')}
print(f'\n图标目录: {icons_dir}')
print(f'当前PNG文件数量: {len(current_files)}')

# 检查必需文件
missing = []
found = []

for req_file in required_files:
    req_lower = req_file.lower()
    if req_lower in current_files:
        found.append(req_file)
        print(f'✓ {req_file}')
    else:
        missing.append(req_file)
        print(f'✗ {req_file} (缺失)')

print('\n' + '=' * 70)
print(f'验证结果:')
print(f'  - 找到: {len(found)}/{len(required_files)} 个必需文件')
if missing:
    print(f'  - 缺失: {len(missing)} 个文件')
    print('\n缺失的文件:')
    for m in missing:
        print(f'  - {m}')
    print('\n请运行 prepare_icons.py 来创建缺失的文件')
else:
    print('  ✓ 所有必需文件都存在！')
    print('\n可以开始构建GHA文件了！')

print('=' * 70)
