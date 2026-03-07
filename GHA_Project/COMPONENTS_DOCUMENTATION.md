# SimpleML 组件文档

本文档详细介绍了SimpleML插件中所有组件的功能、输入输出参数和数据形式。

---

## 目录

1. [数据输入组件](#数据输入组件)
2. [数据分析组件](#数据分析组件)
3. [数据预处理组件](#数据预处理组件)
4. [数据集管理组件](#数据集管理组件)
5. [通用训练组件](#通用训练组件)
6. [算法特定训练组件](#算法特定训练组件)
7. [模型预测组件](#模型预测组件)
8. [模型评估组件](#模型评估组件)
9. [模型管理组件](#模型管理组件)

---

## 数据输入组件

### 1. Read CSV
**功能**: 读取CSV文件并转换为Grasshopper Tree结构

**输入参数**:
- `Filepath` (Text): CSV文件路径
- `Header` (Integer): 表头行号，默认0（0表示第一行是表头，-1表示无表头）
- `Separator` (Text): 分隔符，默认","（逗号）

**输出参数**:
- `Data` (Tree): 数据Tree结构，每行数据作为一个分支
- `Labels` (Tree): 列名的列表（Tree结构，单个分支）
- `Info` (Text): 字符串，描述文件位置、文件名、行数、列数
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Data`: Tree结构，每个分支代表一行数据，分支内的元素是列值
- `Labels`: Tree结构，单个分支 {0}，包含所有列名
- `Info`: 字符串，描述文件位置、文件名、行数、列数


---

### 2. Read Excel
**功能**: 读取Excel文件并转换为Grasshopper Tree结构

**输入参数**:
- `Filepath` (Text): Excel文件路径（.xlsx或.xls）
- `Sheet Name` (Text): 工作表名称，默认"Sheet1"（也可以使用数字索引）
- `Header` (Integer): 表头行号，默认0

**输出参数**:
- `Data` (Tree): 数据Tree结构，每行数据作为一个分支
- `Labels` (Tree): 列名的列表（Tree结构，单个分支）
- `Info` (Text): 字符串，描述文件位置、文件名、行数、列数
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Data`: Tree结构，每个分支代表一行数据
- `Labels`: Tree结构，单个分支 {0}，包含所有列名
- `Info`: 字符串，描述文件位置、文件名、行数、列数

---

### 3. Write CSV
**功能**: 将数据写入CSV文件

**输入参数**:
- `Data` (Tree): 要写入的数据（Tree结构）
- `Filepath` (Text): 保存路径（CSV文件路径）
- `Index` (Boolean): 是否写入行索引，默认False
- `Encoding` (Text): 文件编码，默认'utf-8'
- `Separator` (Text): 分隔符，默认','（逗号）
- `Write` (Boolean): 选择True时执行写入命令，默认False

**输出参数**:
- `Saved Path` (Text): 保存的文件路径
- `Info` (Text): 保存信息（行数、列数等）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Saved Path`: 文本字符串，保存的文件完整路径
- `Info`: 文本字符串，包含保存信息

---

### 4. Write Excel
**功能**: 将数据写入Excel文件

**输入参数**:
- `Data` (Tree): 要写入的数据（Tree结构）
- `Filepath` (Text): 保存路径（Excel文件路径，.xlsx或.xls）
- `Sheet Name` (Text): 工作表名称，默认'Sheet1'
- `Index` (Boolean): 是否写入行索引，默认False
- `Write` (Boolean): 选择True时执行写入命令，默认False

**输出参数**:
- `Saved Path` (Text): 保存的文件路径
- `Info` (Text): 保存信息（行数、列数等）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Saved Path`: 文本字符串，保存的文件完整路径
- `Info`: 文本字符串，包含保存信息

---

### 5. Load Dataset
**功能**: 加载scikit-learn内置数据集，供测试使用

**输入参数**:
- `Dataset Name` (Text): 数据集名称，默认'iris'（可选：iris, wine, breast_cancer, digits, diabetes, california_housing, linnerud, make_classification, make_regression, make_blobs）

**输出参数**:
- `Data` (Tree): 数据Tree结构，每行数据作为一个分支
- `Labels` (Tree): 列名的列表（Tree结构，单个分支）
- `Info` (Text): 数据集信息（描述、样本数、特征数等）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Data`: Tree结构，每个分支代表一行数据，包含特征和target列
- `Labels`: Tree结构，单个分支 {0}，包含所有列名（特征列名和target）
- `Info`: 文本字符串，包含数据集描述、样本数、特征数等信息

**可用数据集**:
- **分类数据集**: iris（鸢尾花，150样本，4特征，3类别）、wine（葡萄酒，178样本，13特征，3类别）、breast_cancer（乳腺癌，569样本，30特征，2类别）、digits（手写数字，1797样本，64特征，10类别）
- **回归数据集**: diabetes（糖尿病，442样本，10特征）、california_housing（加州房价，20640样本，8特征）、linnerud（多输出回归，20样本，3特征，3目标）
- **生成数据集**: make_classification（生成分类数据集）、make_regression（生成回归数据集）、make_blobs（生成聚类数据集）

---

## 数据分析组件

### 6. Calculate Statistics
**功能**: 计算数据的描述性统计（均值、标准差、最小值、最大值等）

**输入参数**:
- `Data` (Tree): 输入数据

**输出参数**:
- `Statistics` (Text): 各个统计信息构成的字符串
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Statistics`: 字符串，包含各列的统计信息

---

### 7. Calculate Correlation
**功能**: 计算特征之间的相关性矩阵

**输入参数**:
- `Data` (Tree): 输入数据
- `Method` (Text): 相关性计算方法，默认"pearson"（可选：pearson, spearman, kendall）

**输出参数**:
- `Correlation Matrix` (Tree): 相关性矩阵
- `Analysis` (Text): 对相关性矩阵的解释说明
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Correlation Matrix`: JSON对象字符串，包含各特征之间的相关系数
- `Analysis`: 字符串，对相关性矩阵的解释说明

---

### 8. Get Data Summary
**功能**: 获取数据摘要（统计、相关性、缺失值等）

**输入参数**:
- `Data` (Tree): 输入数据

**输出参数**:
- `Summary` (Text): 描述全面信息
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Summary`: 字符串，包含数据的全面信息

---

### 9. Describe Features
**功能**: 描述特征（count, mean, std, min, 25%, 50%, 75%, max）

**输入参数**:
- `Data` (Tree): 输入数据
- `Feature Names` (Text): 特征名称列表，可选，默认输入全部

**输出参数**:
- `Description` (Text): 带有说明的文字
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Description`: 字符串，包含每个特征的统计信息

---

## 数据预处理组件

### 10. Create Dataset
**功能**: 准备数据（标准化、处理缺失值、移除异常值），创建数据集

**输入参数**:
- `X` (Tree): 特征数据
- `Labels` (Tree): 标签数据，可选
- `Normalize` (Boolean): 是否标准化，默认False
- `Normalize Method` (Text): 标准化方法，默认"standard"（可选：standard, minmax, robust）
- `Remove Outliers` (Boolean): 是否移除异常值，默认False
- `Handle Missing` (Boolean): 是否处理缺失值，默认False
- `Missing Strategy` (Text): 缺失值处理策略，默认"mean"（可选：mean, median, mode, drop）

**输出参数**:
- `Dataset` (Generic): 数据集对象，能够连接后续的运算器，包含特征数据和标签数据
- `Info` (Text): 描述当前数据处理的方式，数据集的shape
- `Readme` (Text): 组件使用说明




### 11. Deconstruct Dataset
**功能**: 从Dataset对象中提取X和y

**输入参数**:
- `Dataset` (Generic): Dataset对象

**输出参数**:
- `X` (Tree): 特征数据Tree结构
- `labels` (Tree): 标签数据Tree结构
- `Readme` (Text): 组件使用说明

**数据形式**:
- `X`: Tree结构，每个分支是一行特征数据
- `labels`: Tree结构，每个分支包含一个标签值

---

### 12. Split Dataset
**功能**: 分割数据集为训练集和测试集

**输入参数**:
- `Dataset` (Generic): Dataset对象
- `Test Size` (Number): 测试集比例，默认0.2
- `Random State` (Integer): 随机种子，默认42

**输出参数**:
- `Train Dataset` (Generic): 训练集Dataset对象
- `Test Dataset` (Generic): 测试集Dataset对象
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Train Dataset`, `Test Dataset`: Base64编码的pickle对象（Generic类型）

---


## 通用训练组件

### 13. Train Classifier
**功能**: 训练分类器（通用，执行实际训练）

**输入参数**:
- `Dataset` (Generic): Dataset对象（必需）
- `Algorithm` (Text/JSON): 算法参数配置，可以来自算法特定训练组件的 `Algorithm Params` 输出，或直接使用算法名称字符串（如"random_forest"）

**输出参数**:
- `Model` (Generic): 训练好的模型对象
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Model`: Base64编码的pickle对象（Generic类型）

**使用说明**:
- 这是实际执行训练的组件
- `Algorithm` 输入可以接收算法特定训练组件输出的 `Algorithm Params`（JSON格式），也可以直接使用算法名称字符串
- 推荐使用算法特定训练组件配置参数，然后连接到本组件的 `Algorithm` 输入

---

### 14. Train Regressor
**功能**: 训练回归器（通用，执行实际训练）

**输入参数**:
- `Dataset` (Generic): Dataset对象（必需）
- `Algorithm` (Text/JSON): 算法参数配置，可以来自算法特定训练组件的 `Algorithm Params` 输出，或直接使用算法名称字符串（如"linear_regression"）

**输出参数**:
- `Model` (Generic): 训练好的模型对象
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Model`: Base64编码的pickle对象（Generic类型）

**使用说明**:
- 这是实际执行训练的组件
- `Algorithm` 输入可以接收算法特定训练组件输出的 `Algorithm Params`（JSON格式），也可以直接使用算法名称字符串
- 推荐使用算法特定训练组件配置参数，然后连接到本组件的 `Algorithm` 输入

---

### 15. Train Cluster
**功能**: 训练聚类模型（通用，执行实际训练）

**输入参数**:
- `Dataset` (Generic): Dataset对象（必需）
- `Algorithm` (Text/JSON): 算法参数配置，可以来自算法特定训练组件的 `Algorithm Params` 输出，或直接使用算法名称字符串（如"kmeans"）

**输出参数**:
- `Model` (Generic): 训练好的模型对象
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Model`: Base64编码的pickle对象（Generic类型）

**使用说明**:
- 这是实际执行训练的组件
- `Algorithm` 输入可以接收算法特定训练组件输出的 `Algorithm Params`（JSON格式），也可以直接使用算法名称字符串
- 推荐使用算法特定训练组件配置参数，然后连接到本组件的 `Algorithm` 输入

---

## 算法特定训练组件

**重要说明**: 算法特定训练组件**不进行实际训练**，它们只负责配置算法参数。这些组件的输出需要连接到通用训练组件（Train Classifier、Train Regressor 或 Train Cluster）的 `Algorithm` 输入来完成实际训练。

**工作流程**:
1. 使用算法特定组件配置算法参数
2. 将 `Algorithm Params` 输出连接到通用训练组件的 `Algorithm` 输入
3. 将 `Dataset` 连接到通用训练组件的 `Dataset` 输入
4. 通用训练组件完成实际训练并输出模型

---

### 16. Random Forest Classifier
**功能**: 配置随机森林分类器参数（不进行实际训练）

**输入参数**:
- `N Estimators` (Integer): 树的数量，默认100
- `Max Depth` (Integer): 最大深度，默认None（0表示None）
- `Min Samples Split` (Integer): 内部节点再划分所需最小样本数，默认2
- `Min Samples Leaf` (Integer): 叶子节点所需最小样本数，默认1
- `Max Features` (Text): 最大特征数，默认"sqrt"（可选：sqrt, log2, None或整数）
- `Bootstrap` (Boolean): 是否使用bootstrap采样，默认True
- `Random State` (Integer): 随机种子，默认None
- `Class Weight` (Text): 类别权重，默认None（可选：balanced, balanced_subsample）
- `Criterion` (Text): 划分标准，默认"gini"（可选：gini, entropy, log_loss）
- 其他可用的随机森林分类参数 (见sklearn.ensemble.RandomForestClassifier)

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明（包含算法名称、适用范围、参数范围、预期结果）

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 17. Random Forest Regressor
**功能**: 配置随机森林回归器参数（不进行实际训练）

**输入参数**:
- `N Estimators` (Integer): 树的数量，默认100
- `Max Depth` (Integer): 最大深度，默认None（0表示None）
- `Min Samples Split` (Integer): 内部节点再划分所需最小样本数，默认2
- `Min Samples Leaf` (Integer): 叶子节点所需最小样本数，默认1
- `Max Features` (Text): 最大特征数，默认"sqrt"（可选：sqrt, log2, None或整数）
- `Bootstrap` (Boolean): 是否使用bootstrap采样，默认True
- `Random State` (Integer): 随机种子，默认None
- `Criterion` (Text): 划分标准，默认"squared_error"（可选：squared_error, absolute_error, friedman_mse, poisson）
- 其他可用的随机森林回归参数 (见sklearn.ensemble.RandomForestRegressor)

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 18. SVM Classifier
**功能**: 配置SVM分类器参数（不进行实际训练）

**输入参数**:
- `Kernel` (Text): 核函数类型，默认"rbf"（可选：linear, poly, sigmoid, rbf, precomputed）
- `C` (Float): 惩罚系数，默认1.0
- `Gamma` (Text): 核函数系数，默认"scale"（可选：scale, auto或浮点数）
- `Degree` (Integer): 多项式核的度数，默认3（仅用于poly核）
- `Coef0` (Float): 核函数中的独立项，默认0.0（用于poly和sigmoid核）
- `Probability` (Boolean): 是否启用概率估计，默认False
- `Random State` (Integer): 随机种子，默认None
- `Class Weight` (Text): 类别权重，默认None（可选：balanced）
- `Tol` (Float): 停止训练的容差，默认1e-3
- 其他可用的SVM分类参数 (见sklearn.svm.SVC)

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 19. SVR
**功能**: 配置支持向量回归器参数（不进行实际训练）

**输入参数**:
- `Kernel` (Text): 核函数类型，默认"rbf"（可选：linear, poly, sigmoid, rbf, precomputed）
- `C` (Float): 惩罚系数，默认1.0
- `Gamma` (Text): 核函数系数，默认"scale"（可选：scale, auto或浮点数）
- `Degree` (Integer): 多项式核的度数，默认3（仅用于poly核）
- `Coef0` (Float): 核函数中的独立项，默认0.0（用于poly和sigmoid核）
- `Epsilon` (Float): Epsilon-tube的宽度，默认0.1
- `Tol` (Float): 停止训练的容差，默认1e-3
- 其他可用的SVR参数 (见sklearn.svm.SVR)

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 20. Linear Regression
**功能**: 配置线性回归器参数（不进行实际训练）

**输入参数**:
- `Fit Intercept` (Boolean): 是否拟合截距，默认True
- `Normalize` (Boolean): 是否标准化，默认False（已弃用，建议在Create Dataset中处理）
- `Copy X` (Boolean): 是否复制X，默认True
- `N Jobs` (Integer): 并行任务数，默认None（-1表示使用所有CPU）
- 其他可用线性回归参数（见sklearn.linear_model.LinearRegression）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 21. Ridge Regression
**功能**: 配置岭回归器参数（不进行实际训练）

**输入参数**:
- `Alpha` (Float): 正则化强度，默认1.0
- `Fit Intercept` (Boolean): 是否拟合截距，默认True
- `Normalize` (Boolean): 是否标准化，默认False（已弃用）
- `Solver` (Text): 求解器，默认"auto"（可选：auto, svd, cholesky, lsqr, sparse_cg, sag, saga, lbfgs）
- `Max Iter` (Integer): 最大迭代次数，默认None
- `Tol` (Float): 停止训练的容差，默认1e-4
- `Random State` (Integer): 随机种子，默认None（用于sag和saga求解器）
- 其他可用岭回归参数（见sklearn.linear_model.Ridge）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 22. Lasso Regression
**功能**: 配置Lasso回归器参数（不进行实际训练）

**输入参数**:
- `Alpha` (Float): 正则化强度，默认1.0
- `Fit Intercept` (Boolean): 是否拟合截距，默认True
- `Normalize` (Boolean): 是否标准化，默认False（已弃用）
- `Max Iter` (Integer): 最大迭代次数，默认1000
- `Tol` (Float): 停止训练的容差，默认1e-4
- `Selection` (Text): 变量选择策略，默认"cyclic"（可选：cyclic, random）
- `Random State` (Integer): 随机种子，默认None（用于random选择）
- `Warm Start` (Boolean): 是否使用热启动，默认False
- 其他可用Lasso参数（见sklearn.linear_model.Lasso）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 23. KNN Classifier
**功能**: 配置KNN分类器参数（不进行实际训练）

**输入参数**:
- `N Neighbors` (Integer): 邻居数，默认5
- `Weights` (Text): 权重函数，默认"uniform"（可选：uniform, distance）
- `Algorithm` (Text): 计算最近邻的算法，默认"auto"（可选：auto, ball_tree, kd_tree, brute）
- `Leaf Size` (Integer): 叶子节点大小，默认30（用于ball_tree和kd_tree）
- `P` (Integer): 闵可夫斯基距离的幂参数，默认2（2为欧氏距离，1为曼哈顿距离）
- `Metric` (Text): 距离度量，默认"minkowski"（可选：euclidean, manhattan, chebyshev等）
- `N Jobs` (Integer): 并行任务数，默认None（-1表示使用所有CPU）
- 其他可用KNN分类参数（见sklearn.neighbors.KNeighborsClassifier）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 24. KNN Regressor
**功能**: 配置KNN回归器参数（不进行实际训练）

**输入参数**:
- `N Neighbors` (Integer): 邻居数，默认5
- `Weights` (Text): 权重函数，默认"uniform"（可选：uniform, distance）
- `Algorithm` (Text): 计算最近邻的算法，默认"auto"（可选：auto, ball_tree, kd_tree, brute）
- `Leaf Size` (Integer): 叶子节点大小，默认30（用于ball_tree和kd_tree）
- `P` (Integer): 闵可夫斯基距离的幂参数，默认2（2为欧氏距离，1为曼哈顿距离）
- `Metric` (Text): 距离度量，默认"minkowski"（可选：euclidean, manhattan, chebyshev等）
- `N Jobs` (Integer): 并行任务数，默认None（-1表示使用所有CPU）
- 其他可用KNN回归参数（见sklearn.neighbors.KNeighborsRegressor）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Regressor”）以完成训练。

---

### 25. Logistic Regression Classifier
**功能**: 配置逻辑回归分类器参数（不进行实际训练）

**输入参数**:
- `Penalty` (Text): 正则化类型，默认"l2"（可选：l1, l2, elasticnet, None）
- `C` (Float): 惩罚系数的倒数，默认1.0（值越小，正则化越强）
- `Solver` (Text): 优化算法，默认"lbfgs"（可选：lbfgs, liblinear, newton-cg, sag, saga）
- `Max Iter` (Integer): 最大迭代次数，默认100
- `Tol` (Float): 停止训练的容差，默认1e-4
- `Multi Class` (Text): 多分类策略，默认"auto"（可选：ovr, multinomial, auto）
- `Random State` (Integer): 随机种子，默认None
- `L1 Ratio` (Float): Elastic-Net混合参数，默认None（仅用于elasticnet惩罚）
- `Class Weight` (Text): 类别权重，默认None（可选：balanced）
- 其他可用Logistic参数（见sklearn.linear_model.LogisticRegression）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 26. Naive Bayes Classifier
**功能**: 配置朴素贝叶斯分类器参数（不进行实际训练）

**输入参数**:
- `Type` (Text): 贝叶斯类型，默认"GaussianNB"（可选：GaussianNB, MultinomialNB, BernoulliNB, ComplementNB, CategoricalNB）
- `Var Smoothing` (Float): 方差平滑参数，默认1e-9（仅用于GaussianNB）
- `Alpha` (Float): 平滑参数，默认1.0（用于MultinomialNB和BernoulliNB）
- `Fit Prior` (Boolean): 是否学习类别先验概率，默认True（用于MultinomialNB和BernoulliNB）
- `Class Prior` (Text): 类别先验概率，默认None（可选：None或数组）
- 其他可用朴素贝叶斯参数（见sklearn.naive_bayes.*）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 27. Decision Tree Classifier
**功能**: 配置决策树分类器参数（不进行实际训练）

**输入参数**:
- `Criterion` (Text): 划分标准，默认"gini"（可选：gini, entropy, log_loss）
- `Max Depth` (Integer): 最大深度，默认None（0表示None）
- `Min Samples Split` (Integer): 内部节点再划分所需最小样本数，默认2
- `Min Samples Leaf` (Integer): 叶子节点所需最小样本数，默认1
- `Min Weight Fraction Leaf` (Float): 叶子节点所需最小权重比例，默认0.0
- `Max Features` (Text): 最大特征数，默认None（可选：sqrt, log2, None或整数）
- `Random State` (Integer): 随机种子，默认None
- `Max Leaf Nodes` (Integer): 最大叶子节点数，默认None
- `Class Weight` (Text): 类别权重，默认None（可选：balanced）
- 其他可用决策树分类参数（见sklearn.tree.DecisionTreeClassifier）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Classifier”）以完成训练。

---

### 28. K-Means
**功能**: 配置K-Means聚类参数（不进行实际训练）

**输入参数**:
- `N Clusters` (Integer): 聚类数量，默认3
- `Init` (Text): 初始化方式，默认"k-means++"（可选：k-means++, random或数组）
- `N Init` (Integer): 不同初始化的运行次数，默认10
- `Max Iter` (Integer): 单次运行的最大迭代次数，默认300
- `Tol` (Float): 收敛容差，默认1e-4
- `Random State` (Integer): 随机种子，默认None
- `Algorithm` (Text): K-means算法，默认"lloyd"（可选：lloyd, elkan, auto）
- 其他可用KMeans参数（见sklearn.cluster.KMeans）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Cluster”）以完成训练。

---

### 29. DBSCAN
**功能**: 配置DBSCAN聚类参数（不进行实际训练）

**输入参数**:
- `Eps` (Float): 邻域半径，默认0.5
- `Min Samples` (Integer): 最小样本数，默认5
- `Metric` (Text): 距离度量，默认"euclidean"（可选：euclidean, manhattan, cosine等）
- `Algorithm` (Text): 最近邻算法，默认"auto"（可选：auto, ball_tree, kd_tree, brute）
- `Leaf Size` (Integer): 叶子节点大小，默认30（用于ball_tree和kd_tree）
- `P` (Float): 闵可夫斯基距离的幂参数，默认2（仅用于minkowski距离）
- `N Jobs` (Integer): 并行任务数，默认None（-1表示使用所有CPU）
- 其他可用DBSCAN参数（见sklearn.cluster.DBSCAN）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Cluster”）以完成训练。

---

### 30. Agglomerative Clustering
**功能**: 配置层次聚类参数（不进行实际训练）

**输入参数**:
- `N Clusters` (Integer): 聚类数量，默认3
- `Linkage` (Text): 聚合方式，默认"ward"（可选：ward, complete, average, single）
- `Affinity` (Text): 距离度量，默认"euclidean"（可选：euclidean, l1, l2, manhattan, cosine等）
- `Compute Full Tree` (Boolean): 是否计算完整树，默认False
- `Distance Threshold` (Float): 距离阈值，默认None（如果设置，n_clusters必须为None）
- `Compute Distances` (Boolean): 是否计算距离，默认False
- 其他可用Agglomerative参数（见sklearn.cluster.AgglomerativeClustering）

**输出参数**:
- `Algorithm Params` (JSON): 本次配置的参数字典（JSON格式），连接到通用训练组件的 `Algorithm` 输入
- `Readme` (Text): 算法说明

**用途说明**:
- 该组件只负责输出算法相关参数，不参与实际训练。其输出需要连接到通用训练器（如“Train Cluster”）以完成训练。

---


## 模型预测组件

### 31. Predict Classifier
**功能**: 使用训练好的分类模型进行预测

**输入参数**:
- `Model` (Generic): 训练好的分类模型对象
- `X` (Tree): 待预测的特征数据（Tree结构）

**输出参数**:
- `Predictions` (Tree): 预测结果（Tree结构，每个分支包含一个预测类别）
- `Probabilities` (Tree): 预测概率（Tree结构，每个分支包含各类别的概率）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Predictions`: Tree结构，每个分支包含一个预测类别标签
- `Probabilities`: Tree结构，每个分支包含各类别的预测概率（如果模型支持）

---

### 32. Predict Regressor
**功能**: 使用训练好的回归模型进行预测

**输入参数**:
- `Model` (Generic): 训练好的回归模型对象
- `X` (Tree): 待预测的特征数据（Tree结构）

**输出参数**:
- `Predictions` (Tree): 预测结果（Tree结构，每个分支包含一个预测值）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Predictions`: Tree结构，每个分支包含一个预测数值

---

### 33. Predict Cluster
**功能**: 使用训练好的聚类模型进行预测（分配聚类标签）

**输入参数**:
- `Model` (Generic): 训练好的聚类模型对象
- `X` (Tree): 待预测的特征数据（Tree结构）

**输出参数**:
- `Labels` (Tree): 聚类标签（Tree结构，每个分支包含一个聚类标签）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Labels`: Tree结构，每个分支包含一个聚类标签（整数）

---

## 模型评估组件

### 34. Evaluate Classification
**功能**: 评估分类模型的性能

**输入参数**:
- `Model` (Generic): 训练好的分类模型对象
- `X` (Tree): 测试特征数据
- `Y True` (Tree): 真实标签数据
- `Metrics` (Text): 要计算的评估指标，默认"all"（可选：accuracy, precision, recall, f1, confusion_matrix等，或"all"）

**输出参数**:
- `Metrics` (Text/JSON): 评估指标字典（JSON格式）
- `Report` (Text): 详细的评估报告
- `Confusion Matrix` (Tree): 混淆矩阵（如果计算）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Metrics`: JSON对象字符串，包含各种评估指标
- `Report`: 字符串，包含详细的分类报告
- `Confusion Matrix`: Tree结构，表示混淆矩阵

---

### 35. Evaluate Regression
**功能**: 评估回归模型的性能

**输入参数**:
- `Model` (Generic): 训练好的回归模型对象
- `X` (Tree): 测试特征数据
- `Y True` (Tree): 真实标签数据
- `Metrics` (Text): 要计算的评估指标，默认"all"（可选：mse, rmse, mae, r2, explained_variance等，或"all"）

**输出参数**:
- `Metrics` (Text/JSON): 评估指标字典（JSON格式）
- `Report` (Text): 详细的评估报告
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Metrics`: JSON对象字符串，包含各种评估指标（如MSE、RMSE、MAE、R²等）
- `Report`: 字符串，包含详细的回归评估报告

---

### 36. Evaluate Clustering
**功能**: 评估聚类模型的性能

**输入参数**:
- `Model` (Generic): 训练好的聚类模型对象
- `X` (Tree): 测试特征数据
- `Y True` (Tree): 真实标签数据（可选，用于有监督评估指标）
- `Metrics` (Text): 要计算的评估指标，默认"all"（可选：silhouette_score, davies_bouldin_score, calinski_harabasz_score等，或"all"）

**输出参数**:
- `Metrics` (Text/JSON): 评估指标字典（JSON格式）
- `Report` (Text): 详细的评估报告
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Metrics`: JSON对象字符串，包含各种聚类评估指标
- `Report`: 字符串，包含详细的聚类评估报告

---

## 模型管理组件

### 37. Save Model
**功能**: 保存模型

**输入参数**:
- `Model` (Generic): 要保存的模型对象，通常来自训练组件
- `Filepath` (Text): 保存路径，文件扩展名建议使用.pkl或.model
- `Save` (Boolean): 选择True的时候执行存储的命令

**输出参数**:
- `Saved Path` (Text): 保存的文件路径
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Saved Path`: Text字符串，保存的文件完整路径

---


### 38. Load Model
**功能**: 加载模型

**输入参数**:
- `Filepath` (Text): 模型文件路径

**输出参数**:
- `Model` (Generic): 加载的模型对象，可以用于预测
- `Metadata` (Text/JSON): 模型元数据字典（JSON格式）
- `Readme` (Text): 组件使用说明

**数据形式**:
- `Model`: Base64编码的pickle对象（Generic类型）
- `Metadata`: JSON对象字符串，包含模型的训练信息

---



## 数据形式说明

### Tree结构
- **用途**: 用于2D数组数据（每行一个分支）和1D数组数据（单分支）
- **2D数组示例**: 特征数据X，每行是一个样本，每列是一个特征
  - 分支路径: {0}, {1}, {2}...（行索引）
  - 分支内容: [特征1值, 特征2值, 特征3值, ...]
- **1D数组示例**: 标签数据y，每个分支包含一个标签值
  - 分支路径: {0}, {1}, {2}...（样本索引）
  - 分支内容: [标签值]
- **列名Tree结构**: Read CSV/Excel的Labels输出，单个分支 {0}，包含所有列名
- **在Grasshopper中**: 每个分支代表一行数据或一个样本，分支内的元素是列值或标签值

### Text/JSON格式
- **用途**: 用于元数据、信息、指标等
- **示例**: 
  - 统计信息（Calculate Statistics输出）
  - 评估指标（Evaluate组件输出，JSON格式）
  - 数据集信息（Info输出）
  - 模型元数据（Load Model的Metadata输出）
- **在Grasshopper中**: 文本字符串，可以解析为JSON

### Generic类型（Base64编码的pickle对象）
- **用途**: 用于Python对象（Dataset、Model等）
- **示例**: 
  - Dataset对象（Create Dataset、Split Dataset输出）
  - 训练好的模型（Train Classifier/Regressor/Cluster输出）
  - 模型管理器对象（某些算法特定训练组件输出）
- **在Grasshopper中**: 作为Generic类型传递，内部是Base64编码的pickle数据
- **注意事项**: 
  - 这些对象不能直接在Grasshopper中查看内容
  - 需要通过相应的组件（如Deconstruct Dataset、Predict、Evaluate）来使用
  - 可以使用Save Model保存，使用Load Model加载

---

## 使用流程示例

### 分类任务流程
1. **Read Excel/CSV** 或 **Load Dataset** → 读取数据
2. **Create Dataset** → 创建数据集（从Tree数据中提取特征和标签）
3. **Split Dataset** → 分割训练集和测试集
4. **Random Forest Classifier** (或其他算法特定组件) → 配置算法参数
5. **Train Classifier** → 接收 `Algorithm Params` 和 `Dataset`，执行实际训练
6. **Predict Classifier** → 使用模型进行预测
7. **Evaluate Classification** → 评估模型性能
8. **Write CSV/Excel** → 保存预测结果（可选）

**连接方式**:
```
Random Forest Classifier (Algorithm Params输出) → Train Classifier (Algorithm输入)
Split Dataset (Train Dataset输出) → Train Classifier (Dataset输入)
Train Classifier (Model输出) → Predict Classifier (Model输入)
Train Classifier (Model输出) → Evaluate Classification (Model输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → X → Predict Classifier (X输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → X → Evaluate Classification (X输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → Labels → Evaluate Classification (Y True输入)
Predict Classifier (Predictions输出) → Write CSV/Excel (Data输入)
```

### 回归任务流程
1. **Read CSV/Excel** 或 **Load Dataset** → 读取数据
2. **Create Dataset** → 创建数据集（从Tree数据中提取特征和标签）
3. **Split Dataset** → 分割训练集和测试集
4. **Linear Regression** (或其他算法特定组件) → 配置算法参数
5. **Train Regressor** → 接收 `Algorithm Params` 和 `Dataset`，执行实际训练
6. **Predict Regressor** → 使用模型进行预测
7. **Evaluate Regression** → 评估模型性能
8. **Write CSV/Excel** → 保存预测结果（可选）

**连接方式**:
```
Linear Regression (Algorithm Params输出) → Train Regressor (Algorithm输入)
Split Dataset (Train Dataset输出) → Train Regressor (Dataset输入)
Train Regressor (Model输出) → Predict Regressor (Model输入)
Train Regressor (Model输出) → Evaluate Regression (Model输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → X → Predict Regressor (X输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → X → Evaluate Regression (X输入)
Split Dataset (Test Dataset输出) → Deconstruct Dataset → Labels → Evaluate Regression (Y True输入)
Predict Regressor (Predictions输出) → Write CSV/Excel (Data输入)
```

### 聚类任务流程
1. **Read Excel/CSV** 或 **Load Dataset** → 读取数据
2. **Create Dataset** → 创建数据集（仅需要特征数据，不需要标签）
3. **K-Means** (或其他算法特定组件) → 配置算法参数
4. **Train Cluster** → 接收 `Algorithm Params` 和 `Dataset`，执行实际训练
5. **Predict Cluster** → 使用模型进行预测（分配聚类标签）
6. **Evaluate Clustering** → 评估聚类性能
7. **Write CSV/Excel** → 保存聚类结果（可选）

**连接方式**:
```
K-Means (Algorithm Params输出) → Train Cluster (Algorithm输入)
Create Dataset (Dataset输出) → Train Cluster (Dataset输入)
Train Cluster (Model输出) → Predict Cluster (Model输入)
Train Cluster (Model输出) → Evaluate Clustering (Model输入)
Create Dataset (Dataset输出) → Deconstruct Dataset → X → Predict Cluster (X输入)
Create Dataset (Dataset输出) → Deconstruct Dataset → X → Evaluate Clustering (X输入)
Predict Cluster (Labels输出) → Write CSV/Excel (Data输入)
```

---

## 注意事项

1. **数据格式**: 确保输入数据格式正确，Tree结构的数据每行应该是一个分支
2. **数据类型**: 注意数值型和分类型的区别，某些算法对数据类型有要求
3. **缺失值**: 使用Create Dataset组件的Handle Missing选项处理缺失值
4. **标准化**: 某些算法（如SVM、KNN）建议先标准化数据，可在Create Dataset组件中启用Normalize选项
5. **模型保存**: 使用Save Model组件保存模型，直接连接Model输出即可。保存时需将Save参数设置为True
6. **数据保存**: 使用Write CSV/Excel组件保存数据，保存时需将Write参数设置为True
7. **Python环境**: 确保Rhino Python环境中已安装pandas、numpy、scikit-learn等库
8. **算法特定组件**: 算法特定训练组件（如Random Forest Classifier）只负责配置参数，不进行实际训练。必须将其 `Algorithm Params` 输出连接到通用训练组件（Train Classifier/Regressor/Cluster）的 `Algorithm` 输入才能完成训练
9. **通用训练组件**: Train Classifier、Train Regressor、Train Cluster 是实际执行训练的组件，需要同时接收 `Dataset` 和 `Algorithm` 输入，输出Model对象可直接用于预测和保存
10. **预测和评估**: 使用Predict组件进行预测，使用Evaluate组件评估模型性能。预测和评估组件需要接收Model和相应的数据输入
11. **数据提取**: 使用Deconstruct Dataset组件从Dataset对象中提取X和Labels，用于预测和评估
12. **内置数据集**: Load Dataset组件可以加载scikit-learn内置数据集，方便快速测试和学习
13. **文件路径**: 所有文件路径参数支持绝对路径和相对路径，包含中文或特殊字符时需确保编码正确
14. **Tree结构**: Read CSV/Excel输出的是Labels（Tree结构），不是Columns（Text），注意数据类型的区别

---

---

## 组件索引

### 数据输入组件（5个）
1. Read CSV - 读取CSV文件
2. Read Excel - 读取Excel文件
3. Write CSV - 写入CSV文件
4. Write Excel - 写入Excel文件
5. Load Dataset - 加载scikit-learn内置数据集

### 数据分析组件（4个）
6. Calculate Statistics - 计算统计信息
7. Calculate Correlation - 计算相关性矩阵
8. Get Data Summary - 获取数据摘要
9. Describe Features - 描述特征

### 数据集管理组件（3个）
10. Create Dataset - 创建数据集
11. Deconstruct Dataset - 解构数据集
12. Split Dataset - 分割数据集

### 通用训练组件（3个）
13. Train Classifier - 训练分类器
14. Train Regressor - 训练回归器
15. Train Cluster - 训练聚类模型

### 算法特定训练组件（15个）
16. Random Forest Classifier - 配置随机森林分类器
17. Random Forest Regressor - 配置随机森林回归器
18. SVM Classifier - 配置SVM分类器
19. SVR - 配置支持向量回归器
20. Linear Regression - 配置线性回归器
21. Ridge Regression - 配置岭回归器
22. Lasso Regression - 配置Lasso回归器
23. KNN Classifier - 配置KNN分类器
24. KNN Regressor - 配置KNN回归器
25. Logistic Regression Classifier - 配置逻辑回归分类器
26. Naive Bayes Classifier - 配置朴素贝叶斯分类器
27. Decision Tree Classifier - 配置决策树分类器
28. K-Means - 配置K-Means聚类
29. DBSCAN - 配置DBSCAN聚类
30. Agglomerative Clustering - 配置层次聚类

### 模型预测组件（3个）
31. Predict Classifier - 分类预测
32. Predict Regressor - 回归预测
33. Predict Cluster - 聚类预测

### 模型评估组件（3个）
34. Evaluate Classification - 评估分类模型
35. Evaluate Regression - 评估回归模型
36. Evaluate Clustering - 评估聚类模型

### 模型管理组件（2个）
37. Save Model - 保存模型
38. Load Model - 加载模型

**总计：38个组件**

---

*本文档由SimpleML插件自动生成，最后更新日期：2026-01-27*

**更新说明**：
- 算法特定训练组件名称已更新，去掉了"Train"前缀（如"Random Forest Classifier"而非"Train Random Forest Classifier"）
- 通用训练组件（Train Classifier、Train Regressor、Train Cluster）名称保持不变
