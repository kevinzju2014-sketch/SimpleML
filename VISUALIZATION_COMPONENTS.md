# SimpleML 可视化组件使用指南

## 概述

SimpleML 插件提供了多个可视化组件，帮助用户直观地理解机器学习模型的结果和数据特征。这些组件按照优先级实现，解决用户在不同场景下的可视化需求。

## 组件列表

### ★★★★★ 最高优先级（必须做）

#### 1. Visualize Cluster Labels（聚类标签可视化）
**组件名称**: `Visualize Cluster Labels`  
**功能**: 根据聚类标签给几何对象着色预览  
**输入**:
- Geometry (G): 要着色的几何对象（点、曲线、曲面等，Tree结构）
- Labels (L): 聚类标签（Tree结构，每个分支包含一个标签）
- Colors (C): 聚类颜色列表（可选，默认使用自动生成的颜色）

**输出**:
- Colored Geometry (CG): 着色后的几何对象（Tree结构）
- Color Map (CM): 颜色映射表（Tree结构）

**使用场景**:
- 聚类结果可视化
- 根据聚类标签给3D模型着色
- 空间数据聚类分析

**示例工作流**:
```
Create Dataset → Train Cluster → Predict Cluster → Visualize Cluster Labels
```

---

#### 2. Visualize Classification Labels（分类标签可视化）
**组件名称**: `Visualize Classification Labels`  
**功能**: 根据分类标签给几何对象着色预览  
**输入**:
- Geometry (G): 要着色的几何对象（Tree结构）
- Labels (L): 分类标签（Tree结构，每个分支包含一个标签）
- Colors (C): 类别颜色列表（可选）

**输出**:
- Colored Geometry (CG): 着色后的几何对象
- Color Map (CM): 颜色映射表

**使用场景**:
- 分类结果可视化
- 根据预测类别给几何对象着色
- 多类别数据可视化

**示例工作流**:
```
Create Dataset → Train Classifier → Predict Classifier → Visualize Classification Labels
```

---

### ★★★★☆ 高优先级（强烈推荐）

#### 3. Reduce Dimensions（降维可视化）
**组件名称**: `Reduce Dimensions`  
**功能**: 将高维数据降维到2D/3D并可视化  
**输入**:
- X: 特征数据（Tree结构）
- Method (M): 降维方法，'pca' 或 'tsne'，默认'pca'
- Dimensions (D): 降维后的维度（2或3），默认2
- Labels (L): 标签（可选，用于着色）
- Colors (C): 标签颜色列表（可选）

**输出**:
- Points (P): 降维后的点（2D或3D，Tree结构）
- Explained Variance (EV): 解释方差（仅PCA，列表）
- Readme (R): 组件使用说明

**使用场景**:
- 高维数据可视化
- 数据分布探索
- 聚类/分类结果在低维空间的展示
- 特征关系分析

**示例工作流**:
```
Create Dataset → Reduce Dimensions → Visualize (Points)
Create Dataset → Train Classifier → Predict Classifier → Reduce Dimensions (with Labels)
```

**降维方法说明**:
- **PCA (主成分分析)**: 线性降维，速度快，适合大多数情况
- **t-SNE**: 非线性降维，保留局部结构，适合复杂数据分布

---

### ★★★☆☆ 第二批（高价值）

#### 4. Elbow Method（Elbow方法）
**组件名称**: `Elbow Method`  
**功能**: 使用Elbow方法确定最佳聚类数  
**输入**:
- X: 特征数据（Tree结构）
- Max Clusters (MC): 最大聚类数，默认10

**输出**:
- N Clusters (NC): 聚类数列表（1到Max Clusters）
- Scores (S): 对应的SSE得分列表（误差平方和）
- Readme (R): 组件使用说明

**使用场景**:
- 确定K-Means等算法的最佳聚类数
- 聚类参数调优
- 数据聚类分析

**示例工作流**:
```
Create Dataset → Elbow Method → (查看SSE曲线，找到"肘部"点) → Train Cluster (使用最佳K值)
```

**使用方法**:
1. 连接数据到X输入
2. 设置最大聚类数（如10）
3. 查看输出的Scores，绘制曲线
4. 找到曲线的"肘部"（SSE下降速度突然变慢的点）
5. 该点对应的聚类数即为最佳聚类数

---

#### 5. Silhouette Score（Silhouette得分）
**组件名称**: `Silhouette Score`（将在后续版本中添加）  
**功能**: 计算Silhouette得分，评估聚类质量  
**输入**:
- X: 特征数据（Tree结构）
- Labels: 聚类标签（Tree结构）

**输出**:
- Score: Silhouette得分（-1到1之间）
- Readme: 组件使用说明

**使用场景**:
- 评估聚类质量
- 比较不同聚类算法的效果
- 聚类参数优化

**得分说明**:
- 接近1: 聚类质量很好，样本被正确分配到聚类
- 接近0: 聚类质量一般，样本可能在聚类边界
- 接近-1: 聚类质量很差，样本可能被分配到错误的聚类

---

### ★★☆☆☆ 回归任务必备

#### 6. Visualize Regression（回归可视化）
**组件名称**: `Visualize Regression`  
**功能**: 显示预测值 vs 实际值散点图 + 拟合线  
**输入**:
- Y True (YT): 实际值（Tree结构）
- Y Pred (YP): 预测值（Tree结构）
- Show Fit Line (SFL): 是否显示拟合线（y=x），默认True

**输出**:
- Scatter Points (SP): 散点（x=实际值，y=预测值，Tree结构）
- Fit Line (FL): 拟合线（y=x，Curve）
- Readme (R): 组件使用说明

**使用场景**:
- 回归模型评估
- 预测准确性可视化
- 模型性能分析

**示例工作流**:
```
Create Dataset → Split Dataset → Train Regressor → Predict Regressor
→ Evaluate Regression → Visualize Regression (Y True + Y Pred)
```

**解读方法**:
- 理想情况：所有点都在y=x线上
- 点越接近y=x线，预测越准确
- 点分布在线上方：预测值偏高
- 点分布在线下方：预测值偏低

---

### ★★☆☆☆ 可选（后期添加）

#### 7. Feature Importance（特征重要性）
**组件名称**: `Feature Importance`（将在后续版本中添加）  
**功能**: 显示特征重要性条形图  
**输入**:
- Model: 训练好的模型（支持Random Forest等）
- Feature Names: 特征名称列表（可选）

**输出**:
- Feature Names: 特征名称列表
- Importance Scores: 重要性得分列表
- Visualization: 可视化图表

**使用场景**:
- 特征选择
- 模型解释性分析
- 理解模型决策过程

**支持的模型**:
- Random Forest Classifier/Regressor
- Decision Tree Classifier/Regressor
- 其他支持feature_importances_属性的模型

---

## 使用技巧

### 1. 颜色自定义
- 所有可视化组件都支持自定义颜色
- 如果不提供颜色，组件会自动生成10种默认颜色
- 颜色会循环使用，如果类别数超过10个

### 2. 几何对象类型支持
- **点 (Point)**: 直接着色显示
- **曲线 (Curve)**: 按颜色绘制曲线
- **曲面 (Surface)**: 按颜色绘制曲面
- **网格 (Mesh)**: 按颜色绘制网格线框
- **Brep**: 按颜色绘制Brep线框

### 3. 降维可视化建议
- **PCA**: 适合线性关系明显的数据，速度快
- **t-SNE**: 适合复杂非线性数据，但速度较慢
- 2D降维适合快速预览，3D降维适合详细分析

### 4. 回归可视化
- 使用Evaluate Regression组件获取Y True和Y Pred
- 拟合线y=x表示理想预测（预测值=实际值）
- 可以通过散点分布判断模型是否存在系统性偏差

---

## 常见问题

### Q1: 可视化组件不显示预览？
**A**: 确保：
1. 几何对象和标签数量匹配
2. 标签格式正确（聚类标签是整数，分类标签是字符串）
3. 在Grasshopper视图中启用"Preview"显示

### Q2: 降维后点都在一个位置？
**A**: 可能原因：
1. 数据特征值完全相同或几乎相同
2. 数据需要先标准化
3. 尝试使用t-SNE代替PCA

### Q3: Elbow方法没有明显的"肘部"？
**A**: 可能原因：
1. 数据本身不适合聚类
2. 需要增加最大聚类数
3. 考虑使用Silhouette Score等其他方法

### Q4: 回归可视化中所有点都不在拟合线上？
**A**: 这是正常的，说明：
1. 模型有预测误差
2. 可以通过R²等指标量化误差
3. 如果误差过大，考虑：
   - 增加训练数据
   - 尝试其他算法
   - 检查特征工程

---

## 更新日志

### v1.0.0 (2026-01-28)
- ✅ 实现聚类标签可视化（Custom Preview）
- ✅ 实现分类标签可视化（Custom Preview）
- ✅ 实现降维可视化（PCA/t-SNE，2D/3D）
- ✅ 实现Elbow方法组件
- ✅ 实现回归可视化（预测vs实际+拟合线）
- ✅ 创建Python端可视化组件支持

### 计划功能
- ⏳ Silhouette Score组件
- ⏳ Feature Importance组件
- ⏳ 更多图表类型（混淆矩阵、ROC曲线等）

---

## 技术支持

如有问题或建议，请联系：
- 邮箱：zhao_guijia@outlook.com
- B站主页：https://space.bilibili.com/387841705
- 小红书主页：https://xhslink.com/m/8TiSSoH3TuS
