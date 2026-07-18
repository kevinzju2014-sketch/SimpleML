# 示例 2：聚类 + 一键上色

1. `环境体检` → PASS  
2. `加载示例数据集` → `make_blobs` → Dataset  
3. `智能训练` → Task=`clustering`, K=3  
4. 在 Rhino 中准备点（或用 make_blobs 特征当 2D 点）  
5. `一键聚类上色` → Points + Model → Colors 接 Custom Preview  
6. `评估` 或 `轮廓系数` 查看 Verdict  

也可：`新手向导` Task=`clustering`。
