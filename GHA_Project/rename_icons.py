# -*- coding: utf-8 -*-
import os
import shutil

icons_dir = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'

# 需要的目标文件名映射
# 格式: 目标文件名 -> [可能的源文件名列表]
rename_mapping = {
    # 共享图标（多个组件使用同一个图标）
    'file_io_components.png': ['read_csv.png', 'read_excel.png', 'write_csv.png', 'write_excel.png'],
    'statistics_components.png': ['calculate_statistics.png', 'calculate correlation.png', 'get_data_summary.png', 'describe_features.png'],
    'dataset_components.png': ['create_dataset.png', 'deconstruct_dataset.png', 'split_data.png'],
    'train_components.png': ['train_classifier.png', 'train_regressor.png', 'train_cluster.png'],
    'predict_components.png': ['predict_classifier.png', 'predict_regressor.png', 'predict_cluster.png'],
    'evaluate_components.png': ['evaluate_classification.png', 'evaluate_regression.png', 'evaluate_clustering.png'],
    'model_io_components.png': ['save_model.png', 'load_model.png'],
    'dataset_loader.png': ['load_dataset.png'],
    
    # 算法特定图标（需要添加train_前缀）
    'train_random_forest_classifier.png': ['random_forest_classifier.png'],
    'train_svm_classifier.png': ['support_vector_machine_classifier.png'],
    'train_knn_classifier.png': ['k_nearest_neighbors_classifier.png'],
    'train_logistic_regression_classifier.png': ['logistic_regression.png'],
    'train_naive_bayes_classifier.png': ['naive_bayes_classifier.png'],
    'train_decision_tree_classifier.png': ['decision_tree_classifier.png'],
    'train_random_forest_regressor.png': ['random_forest_regressor.png'],
    'train_svr.png': ['support_vector_regression.png'],
    'train_linear_regression.png': ['linear_regression.png'],
    'train_ridge_regression.png': ['ridge_regression.png'],
    'train_lasso_regression.png': ['lasso_regression.png'],
    'train_knn_regressor.png': ['k_nearest_neighbors_regressor.png'],
    'train_kmeans.png': ['k_means.png'],
    'train_dbscan.png': ['density_baised_spatial_clistering_of_applications_with_noise.png'],
    'train_agglomerative_clustering.png': ['agglomerative_clustering.png'],
}

print('=' * 70)
print('图标文件重命名和创建')
print('=' * 70)

# 获取当前文件（大小写不敏感）
current_files = {}
for f in os.listdir(icons_dir):
    if f.endswith('.png'):
        current_files[f.lower()] = f

print(f'\n当前图标文件数量: {len(current_files)}')

# 创建备份目录
backup_dir = os.path.join(icons_dir, 'backup')
os.makedirs(backup_dir, exist_ok=True)

created = []
skipped = []
missing = []

for target_file, source_options in rename_mapping.items():
    target_path = os.path.join(icons_dir, target_file)
    
    # 如果目标文件已存在，跳过
    if os.path.exists(target_path):
        print(f'✓ {target_file} 已存在')
        skipped.append(target_file)
        continue
    
    # 查找第一个存在的源文件
    source_found = None
    for source_option in source_options:
        source_lower = source_option.lower()
        if source_lower in current_files:
            source_found = current_files[source_lower]
            break
    
    if source_found:
        source_path = os.path.join(icons_dir, source_found)
        # 备份源文件
        backup_path = os.path.join(backup_dir, source_found)
        if not os.path.exists(backup_path):
            shutil.copy2(source_path, backup_path)
        
        # 复制为目标文件名
        shutil.copy2(source_path, target_path)
        print(f'✓ 创建 {target_file} (从 {source_found})')
        created.append(target_file)
    else:
        print(f'⚠ 未找到 {target_file} 的源文件')
        print(f'  尝试的源文件: {", ".join(source_options)}')
        missing.append((target_file, source_options))

print('\n' + '=' * 70)
print(f'完成！')
print(f'- 创建了 {len(created)} 个图标文件')
print(f'- 跳过了 {len(skipped)} 个已存在的文件')
if missing:
    print(f'- 警告：{len(missing)} 个图标文件未找到源文件')

# 列出最终文件
print('\n' + '=' * 70)
print('最终图标文件列表:')
final_files = sorted([f for f in os.listdir(icons_dir) if f.endswith('.png') and not f.startswith('backup')])
for f in final_files:
    print(f'  {f}')
print(f'\n总计: {len(final_files)} 个图标文件')
