# SimpleML 测试说明

## 运行全部测试

```bash
# 仓库根目录
python3 -m pip install -r requirements.txt
python3 tests/test_simpleml.py
```

或使用 pytest（若已安装）：

```bash
python3 -m pytest tests/test_simpleml.py -v
```

## 覆盖范围

| 用例组 | 内容 |
|--------|------|
| TestEnvAndHealth | 路径引导、环境体检 PASS |
| TestClassificationPipeline | iris 智能训练 → 预测 → 评估 → 特征重要性 |
| TestRegressionPipeline | diabetes 回归链路 |
| TestClusteringPipeline | make_blobs 聚类 + 轮廓系数 |
| TestPreprocessorBundle | 预处理随模型打包 |
| TestWizardAndDocsPresence | 向导配方与文档文件存在 |

## 补充快通脚本

```bash
python3 examples/quickstart_classification.py
```

> 以上测试验证 **Python 核心**。Grasshopper 组件需在本机 Rhino 中按 `docs/INSTALL_GUIDE.md` 验收。
