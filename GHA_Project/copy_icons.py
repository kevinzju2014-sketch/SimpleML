# -*- coding: utf-8 -*-
"""
直接复制图标文件，创建ComponentIconMap.cs需要的所有图标文件
"""
import os
import shutil

# 使用绝对路径
icons_dir = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'

if not os.path.exists(icons_dir):
    print(f'错误：目录不存在: {icons_dir}')
    exit(1)

os.chdir(icons_dir)

# 获取所有PNG文件（大小写不敏感）
all_files = os.listdir('.')
png_files = {f.lower(): f for f in all_files if f.lower().endswith('.png')}

print(f'找到 {len(png_files)} 个PNG文件\n')

# 映射关系
mappings = [
    ('read_csv.png', 'file_io_components.png'),
    ('calculate_statistics.png', 'statistics_components.png'),
    ('create_dataset.png', 'dataset_components.png'),
    ('train_classifier.png', 'train_components.png'),
    ('predict_classifier.png', 'predict_components.png'),
    ('evaluate_classification.png', 'evaluate_components.png'),
    ('save_model.png', 'model_io_components.png'),
    ('load_dataset.png', 'dataset_loader.png'),
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

print('=' * 70)
print('创建必需的图标文件')
print('=' * 70)

created = 0
skipped = 0
missing = []

for source, target in mappings:
    source_lower = source.lower()
    
    # 检查目标文件是否已存在
    if os.path.exists(target):
        print(f'✓ {target} (已存在)')
        skipped += 1
        continue
    
    # 查找源文件（大小写不敏感）
    if source_lower in png_files:
        source_file = png_files[source_lower]
        try:
            shutil.copy2(source_file, target)
            print(f'✓ 创建 {target} (从 {source_file})')
            created += 1
        except Exception as e:
            print(f'✗ 错误：无法创建 {target}: {e}')
            missing.append((source, target))
    else:
        print(f'⚠ {target} (源文件 {source} 不存在)')
        missing.append((source, target))

print('\n' + '=' * 70)
print(f'完成！')
print(f'  - 创建了 {created} 个文件')
print(f'  - 跳过了 {skipped} 个已存在的文件')
if missing:
    print(f'  - 警告：{len(missing)} 个文件未创建')

# 列出所有需要的文件
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

final_files = {f.lower(): f for f in os.listdir('.') if f.lower().endswith('.png')}
missing_required = [r for r in required if r.lower() not in final_files]

if missing_required:
    print(f'\n⚠ 缺失 {len(missing_required)} 个必需文件:')
    for m in missing_required:
        print(f'  - {m}')
else:
    print('\n✓ 所有必需文件都存在！')

print(f'\n总计图标文件: {len([f for f in os.listdir(".") if f.lower().endswith(".png")])} 个')
