# -*- coding: utf-8 -*-
"""
修复图标文件名，使其与ComponentIconMap.cs中的映射匹配
"""
import os
import shutil

icons_dir = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'
os.chdir(icons_dir)

# 获取当前文件（大小写不敏感）
files = {f.lower(): f for f in os.listdir('.') if f.endswith('.png')}
print(f'找到 {len(files)} 个PNG文件\n')

# 共享图标映射
shared_mappings = [
    ('read_csv.png', 'file_io_components.png'),
    ('calculate_statistics.png', 'statistics_components.png'),
    ('create_dataset.png', 'dataset_components.png'),
    ('train_classifier.png', 'train_components.png'),
    ('predict_classifier.png', 'predict_components.png'),
    ('evaluate_classification.png', 'evaluate_components.png'),
    ('save_model.png', 'model_io_components.png'),
    ('load_dataset.png', 'dataset_loader.png'),
]

# 算法特定图标映射
algorithm_mappings = [
    ('random_forest_classifier.png', 'train_random_forest_classifier.png'),
    ('support_vector_machine_classifier.png', 'train_svm_classifier.png'),
    ('k_nearest_neighbors_classifier.png', 'train_knn_classifier.png'),
    ('logistic_regression.png', 'train_logistic_regression_classifier.png'),
    ('naive_bayes_classifier.png', 'train_naive_bayes_classifier.png'),
    ('decision_tree_classifier.png', 'train_decision_tree_classifier.png'),
    ('random_forest_regressor.png', 'train_random_forest_regressor.png'),
    ('support_vector_regression.png', 'train_svr.png'),
    ('linear_regression.png', 'train_linear_regression.png'),
    ('ridge_regression.png', 'train_ridge_regression.png'),
    ('lasso_regression.png', 'train_lasso_regression.png'),
    ('k_nearest_neighbors_regressor.png', 'train_knn_regressor.png'),
    ('k_means.png', 'train_kmeans.png'),
    ('density_baised_spatial_clistering_of_applications_with_noise.png', 'train_dbscan.png'),
    ('agglomerative_clustering.png', 'train_agglomerative_clustering.png'),
]

all_mappings = shared_mappings + algorithm_mappings

print('=' * 70)
print('开始创建/复制图标文件')
print('=' * 70)

created = 0
skipped = 0
missing = []

for source, target in all_mappings:
    source_lower = source.lower()
    
    # 检查目标文件是否已存在
    if os.path.exists(target):
        print(f'✓ {target} 已存在')
        skipped += 1
        continue
    
    # 检查源文件是否存在
    if source_lower in files:
        source_file = files[source_lower]
        shutil.copy2(source_file, target)
        print(f'✓ 创建 {target} (从 {source_file})')
        created += 1
    else:
        print(f'⚠ 源文件不存在: {source}')
        missing.append((source, target))

print('\n' + '=' * 70)
print(f'完成！')
print(f'- 创建了 {created} 个文件')
print(f'- 跳过了 {skipped} 个已存在的文件')
if missing:
    print(f'- 警告：{len(missing)} 个源文件未找到')

# 验证最终文件
print('\n' + '=' * 70)
print('验证必需文件')
print('=' * 70)

required = [
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
]

final_files = {f.lower(): f for f in os.listdir('.') if f.endswith('.png')}
missing_required = [r for r in required if r.lower() not in final_files]

if missing_required:
    print(f'\n⚠ 缺失 {len(missing_required)} 个必需文件:')
    for m in missing_required:
        print(f'  - {m}')
else:
    print('\n✓ 所有必需文件都存在！')

print(f'\n总计图标文件: {len([f for f in os.listdir(".") if f.endswith(".png")])} 个')
