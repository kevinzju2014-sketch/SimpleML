# 算法子类别更新总结

## ✅ 已完成的更新

所有算法组件已添加子类别，在Grasshopper中将按分类、回归、聚类的顺序展示。

## 子类别结构

### 05 Algorithm|Classification (分类算法 - 6个)
1. TrainRandomForestClassifierComponent - Random Forest Classifier
2. TrainSVMClassifierComponent - SVM Classifier
3. TrainKNNClassifierComponent - KNN Classifier
4. TrainLogisticRegressionClassifierComponent - Logistic Regression Classifier
5. TrainNaiveBayesClassifierComponent - Naive Bayes Classifier
6. TrainDecisionTreeClassifierComponent - Decision Tree Classifier

### 05 Algorithm|Regression (回归算法 - 6个)
1. TrainRandomForestRegressorComponent - Random Forest Regressor
2. TrainSVRComponent - SVR
3. TrainLinearRegressionComponent - Linear Regression
4. TrainRidgeRegressionComponent - Ridge Regression
5. TrainLassoRegressionComponent - Lasso Regression
6. TrainKNNRegressorComponent - KNN Regressor

### 05 Algorithm|Clustering (聚类算法 - 3个)
1. TrainKMeansComponent - K-Means
2. TrainDBSCANComponent - DBSCAN
3. TrainAgglomerativeClusteringComponent - Agglomerative Clustering

## 显示效果

在Grasshopper中，组件将按以下结构显示：

```
SimpleML
  ├─ 01 Input
  ├─ 02 Analysis
  ├─ 03 Dataset
  ├─ 04 Model
  ├─ 05 Algorithm
  │   ├─ Classification
  │   │   ├─ Random Forest Classifier
  │   │   ├─ SVM Classifier
  │   │   ├─ KNN Classifier
  │   │   ├─ Logistic Regression Classifier
  │   │   ├─ Naive Bayes Classifier
  │   │   └─ Decision Tree Classifier
  │   ├─ Regression
  │   │   ├─ Random Forest Regressor
  │   │   ├─ SVR
  │   │   ├─ Linear Regression
  │   │   ├─ Ridge Regression
  │   │   ├─ Lasso Regression
  │   │   └─ KNN Regressor
  │   └─ Clustering
  │       ├─ K-Means
  │       ├─ DBSCAN
  │       └─ Agglomerative Clustering
  ├─ 06 Prediction
  └─ 07 others
```

## 统计

- **分类算法**: 6个
- **回归算法**: 6个
- **聚类算法**: 3个
- **总计**: 15个算法组件

## 更新详情

所有15个算法组件的类别已从：
- `"SimpleML", "05 Algorithm"`

更新为：
- `"SimpleML", "05 Algorithm|Classification"` (分类算法)
- `"SimpleML", "05 Algorithm|Regression"` (回归算法)
- `"SimpleML", "05 Algorithm|Clustering"` (聚类算法)

## 下一步

重新构建GHA文件后，算法组件将在Grasshopper中按子类别分组显示，便于查找和使用。
