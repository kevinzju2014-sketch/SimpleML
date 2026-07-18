# 示例 1：鸢尾花分类（打开即照做）

> 在 Grasshopper 中也可直接放「新手向导」，Task=`classification`。

## 电池连接（按顺序）

1. `环境体检` → Run=true → Status 应为 **PASS**  
   - 若 FAIL：看 **Next Steps**，或 AutoFix=true
2. `加载示例数据集` → DN=`iris` → 使用 **Dataset** 输出  
3. `分割数据集` → Dataset  
4. `智能训练` → Train Dataset，Task=`classification`  
5. `预测` → Model + Test Dataset  
6. `评估` → Model + Test Dataset → 阅读 **Verdict**  
7. （可选）`特征重要性`

## 预期

- 准确率通常很高（演示数据）  
- Verdict 显示「很强 / 可用 …」  
