# -*- coding: utf-8 -*-
import os
import shutil

icons_dir = os.path.join(os.path.dirname(__file__), 'icons')
os.chdir(icons_dir)

files = {f.lower(): f for f in os.listdir('.') if f.endswith('.png')}
print(f'找到 {len(files)} 个PNG文件\n')

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
created = 0
for source, target in mappings:
    if source.lower() in files and not os.path.exists(target):
        shutil.copy2(files[source.lower()], target)
        print(f'✓ {target}')
        created += 1
    elif os.path.exists(target):
        print(f'✓ {target} (已存在)')
    else:
        print(f'⚠ {target} (源文件 {source} 不存在)')

print(f'\n完成！创建了 {created} 个文件')
