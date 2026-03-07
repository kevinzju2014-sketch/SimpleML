# -*- coding: utf-8 -*-
"""
检查并重命名图标文件，使其与ComponentIconMap.cs中的映射匹配
"""

import os
import shutil

icons_dir = r'D:\Helio\250928_机器学习课程\myML\GHA_Project\icons'

# 定义重命名映射：当前文件名 -> 目标文件名
rename_map = {
    # 数据输入组件
    'read_csv.png': 'file_io_components.png',
    'read_excel.png': 'file_io_components.png',
    'write_csv.png': 'file_io_components.png',
    'write_excel.png': 'file_io_components.png',
    'load_dataset.png': 'dataset_loader.png',
    
    # 数据分析组件
    'calculate_statistics.png': 'statistics_components.png',
    'calculate correlation.png': 'statistics_components.png',  # 有空格的文件名
    'get_data_summary.png': 'statistics_components.png',
    'describe_features.png': 'statistics_components.png',
    
    # 数据集组件
    'create_dataset.png': 'dataset_components.png',
    'deconstruct_dataset.png': 'dataset_components.png',
    'split_data.png': 'dataset_components.png',
    
    # 训练组件
    'train_classifier.png': 'train_components.png',
    'train_regressor.png': 'train_components.png',
    'train_cluster.png': 'train_components.png',
    
    # 算法特定训练组件 - 分类
    'random_forest_classifier.png': 'train_random_forest_classifier.png',
    'support_vector_machine_classifier.png': 'train_svm_classifier.png',
    'k_nearest_neighbors_classifier.png': 'train_knn_classifier.png',
    'logistic_regression.png': 'train_logistic_regression_classifier.png',
    'naive_bayes_classifier.png': 'train_naive_bayes_classifier.png',
    'decision_tree_classifier.png': 'train_decision_tree_classifier.png',
    
    # 算法特定训练组件 - 回归
    'random_forest_regressor.png': 'train_random_forest_regressor.png',
    'support_vector_regression.png': 'train_svr.png',
    'linear_regression.png': 'train_linear_regression.png',
    'ridge_regression.png': 'train_ridge_regression.png',
    'lasso_regression.png': 'train_lasso_regression.png',
    'k_nearest_neighbors_regressor.png': 'train_knn_regressor.png',
    
    # 算法特定训练组件 - 聚类
    'k_means.png': 'train_kmeans.png',
    'density_baised_spatial_clistering_of_applications_with_noise.png': 'train_dbscan.png',
    'agglomerative_clustering.png': 'train_agglomerative_clustering.png',
    
    # 预测组件
    'predict_classifier.png': 'predict_components.png',
    'predict_regressor.png': 'predict_components.png',
    'predict_cluster.png': 'predict_components.png',
    
    # 评估组件
    'evaluate_classification.png': 'evaluate_components.png',
    'evaluate_regression.png': 'evaluate_components.png',
    'evaluate_clustering.png': 'evaluate_components.png',
    
    # 模型IO组件
    'save_model.png': 'model_io_components.png',
    'load_model.png': 'model_io_components.png',
}

# 需要创建的目标文件列表（多个源文件映射到同一个目标文件）
target_files_needed = {
    'file_io_components.png': ['read_csv.png', 'read_excel.png', 'write_csv.png', 'write_excel.png'],
    'statistics_components.png': ['calculate_statistics.png', 'calculate correlation.png', 'get_data_summary.png', 'describe_features.png'],
    'dataset_components.png': ['create_dataset.png', 'deconstruct_dataset.png', 'split_data.png'],
    'train_components.png': ['train_classifier.png', 'train_regressor.png', 'train_cluster.png'],
    'predict_components.png': ['predict_classifier.png', 'predict_regressor.png', 'predict_cluster.png'],
    'evaluate_components.png': ['evaluate_classification.png', 'evaluate_regression.png', 'evaluate_clustering.png'],
    'model_io_components.png': ['save_model.png', 'load_model.png'],
}

print('=' * 60)
print('图标文件检查和重命名')
print('=' * 60)

# 获取当前所有图标文件
current_files = [f for f in os.listdir(icons_dir) if f.endswith('.png')]
print(f'\n当前图标文件数量: {len(current_files)}')
print(f'图标目录: {icons_dir}\n')

# 检查每个目标文件
renamed_count = 0
created_count = 0
backup_dir = os.path.join(icons_dir, 'backup')
os.makedirs(backup_dir, exist_ok=True)

for target_file, source_files in target_files_needed.items():
    target_path = os.path.join(icons_dir, target_file)
    
    # 检查目标文件是否已存在
    if os.path.exists(target_path):
        print(f'✓ {target_file} 已存在')
        continue
    
    # 查找第一个存在的源文件
    source_found = None
    for source_file in source_files:
        source_path = os.path.join(icons_dir, source_file)
        if os.path.exists(source_path):
            source_found = source_file
            break
    
    if source_found:
        # 备份源文件
        backup_path = os.path.join(backup_dir, source_found)
        shutil.copy2(os.path.join(icons_dir, source_found), backup_path)
        
        # 复制并重命名
        shutil.copy2(os.path.join(icons_dir, source_found), target_path)
        print(f'✓ 创建 {target_file} (从 {source_found})')
        created_count += 1
    else:
        print(f'⚠ 警告: 未找到 {target_file} 的源文件 ({", ".join(source_files)})')

# 处理算法特定组件的重命名
algorithm_rename_map = {
    'random_forest_classifier.png': 'train_random_forest_classifier.png',
    'support_vector_machine_classifier.png': 'train_svm_classifier.png',
    'k_nearest_neighbors_classifier.png': 'train_knn_classifier.png',
    'logistic_regression.png': 'train_logistic_regression_classifier.png',
    'naive_bayes_classifier.png': 'train_naive_bayes_classifier.png',
    'decision_tree_classifier.png': 'train_decision_tree_classifier.png',
    'random_forest_regressor.png': 'train_random_forest_regressor.png',
    'support_vector_regression.png': 'train_svr.png',
    'linear_regression.png': 'train_linear_regression.png',
    'ridge_regression.png': 'train_ridge_regression.png',
    'lasso_regression.png': 'train_lasso_regression.png',
    'k_nearest_neighbors_regressor.png': 'train_knn_regressor.png',
    'k_means.png': 'train_kmeans.png',
    'density_baised_spatial_clistering_of_applications_with_noise.png': 'train_dbscan.png',
    'agglomerative_clustering.png': 'train_agglomerative_clustering.png',
}

print('\n' + '-' * 60)
print('处理算法特定组件图标重命名')
print('-' * 60)

for old_name, new_name in algorithm_rename_map.items():
    old_path = os.path.join(icons_dir, old_name)
    new_path = os.path.join(icons_dir, new_name)
    
    if os.path.exists(old_path) and not os.path.exists(new_path):
        # 备份
        backup_path = os.path.join(backup_dir, old_name)
        shutil.copy2(old_path, backup_path)
        
        # 复制并重命名
        shutil.copy2(old_path, new_path)
        print(f'✓ 创建 {new_name} (从 {old_name})')
        created_count += 1
    elif os.path.exists(new_path):
        print(f'✓ {new_name} 已存在')
    elif not os.path.exists(old_path):
        print(f'⚠ 警告: 源文件 {old_name} 不存在')

# 处理有空格的文件名
space_file = os.path.join(icons_dir, 'calculate correlation.png')
if os.path.exists(space_file):
    # 如果 statistics_components.png 还不存在，使用这个文件
    target_path = os.path.join(icons_dir, 'statistics_components.png')
    if not os.path.exists(target_path):
        backup_path = os.path.join(backup_dir, 'calculate correlation.png')
        shutil.copy2(space_file, backup_path)
        shutil.copy2(space_file, target_path)
        print(f'✓ 创建 statistics_components.png (从 calculate correlation.png)')
        created_count += 1

print('\n' + '=' * 60)
print(f'完成！')
print(f'- 创建/更新了 {created_count} 个图标文件')
print(f'- 备份文件保存在: {backup_dir}')
print('=' * 60)

# 验证最终文件列表
print('\n最终图标文件列表:')
final_files = sorted([f for f in os.listdir(icons_dir) if f.endswith('.png') and f != 'backup'])
for f in final_files:
    print(f'  - {f}')
print(f'\n总计: {len(final_files)} 个图标文件')
