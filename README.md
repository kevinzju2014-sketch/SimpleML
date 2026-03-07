# SimpleML - Grasshopper机器学习插件

基于 scikit-learn 的 Grasshopper 机器学习插件，提供完整的机器学习工作流程，包括分类、回归、聚类等功能。

## 快速开始

1. **安装Python依赖**：
   ```bash
   pip install -r requirements.txt
   ```

2. **在Grasshopper中使用**：
   - 将myML文件夹复制到可访问的位置
   - 在Python组件中添加路径：
   ```python
   import sys
   sys.path.append(r'[myML文件夹路径]')
   ```

## 文档

- **README_SIMPLEML.md** - 完整的使用说明
- **SIMPLEML_CATEGORIES.md** - 电池分类和使用指南
- **SIMPLEML_GRASSHOPPER_SETUP.md** - Grasshopper设置指南
- **COMPONENTS_EXPLANATION.md** - 组件详细说明
- **WORKFLOW_DIAGRAM.md** - 工作流程图
- **DATASET_GUIDE.md** - 数据集使用指南
- **ALGORITHMS_GUIDE.md** - 算法使用指南
- **LOAD_MODEL_GUIDE.md** - 模型加载指南

## 项目结构

```
myML/
├── components/          # 组件代码
│   ├── file_io_components.py
│   ├── statistics_components.py
│   ├── dataset_components.py
│   ├── train_components.py
│   ├── predict_components.py
│   ├── evaluate_components.py
│   └── algorithms/     # 算法特定组件
├── core/               # 核心功能
│   ├── ml_models.py
│   ├── data_preprocessing.py
│   └── model_io.py
└── examples/           # 示例文件
```

## 功能特性

- ✅ 数据输入（CSV、Excel）
- ✅ 数据分析和统计
- ✅ 数据预处理
- ✅ 数据集管理
- ✅ 模型训练（15种算法）
- ✅ 模型预测
- ✅ 模型评估
- ✅ 模型保存和加载

## 支持的算法

### 分类
- 随机森林、SVM、逻辑回归、KNN、决策树、朴素贝叶斯

### 回归
- 随机森林、SVR、线性回归、岭回归、Lasso、KNN

### 聚类
- K-Means、DBSCAN、层次聚类

## 许可证

MIT License

---

**版本**: 1.0.0  
**更新日期**: 2026年1月26日
