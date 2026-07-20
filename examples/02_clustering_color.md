# 示例 2：聚类 + 一键上色

目标：快速得到簇标签，并在 Rhino 里给点上色。

## 画布电池

1. `Health Check` → PASS
2. `Load Dataset` → DN=`make_blobs` → Dataset  
   （或自备 Points + `Quick Dataset`）
3. `Smart Train` → Task=`clustering`，K=3（需要时）
4. `Quick Cluster Color` / 一键聚类上色 → Points + Model → Colors 接 Custom Preview
5. `Evaluate` 或 `Silhouette Score` 看 Verdict

## 换数据源

| 场景 | 做法 |
|------|------|
| 换示例 | DN 改为其他无标签/聚类友好数据 |
| 用几何点 | 把点列表接到 `Quick Cluster Color` 的 Points；模型仍由 Dataset 训练 |
| 用 CSV 特征 | `Read CSV` → `Quick Dataset` → `Smart Train` |

## 也可用

`Beginner Wizard`，Task=`clustering`。
