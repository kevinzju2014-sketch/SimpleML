# Food4Rhino / Yak 上架准备

## 产品信息（可直接粘贴）

**Name:** SimpleML  
**Version:** 1.2.0  
**Author:** 参数化凯通学  
**License:** MIT  
**Compatible with:** Rhino 7, Rhino 8+ (Windows & macOS)  
**Requires:** Python 3.9+, scikit-learn, numpy, pandas, joblib  

**Short description:**  
Make machine learning easy inside Grasshopper. Load sample data, Smart Train, predict, evaluate — with plain-language results.

**Long description:**  
SimpleML is a free Grasshopper plugin that wraps scikit-learn for designers.  
It supports classification, regression, and clustering with a beginner path:

1. Health Check  
2. Load Dataset (iris / blobs / diabetes…)  
3. Smart Train  
4. Predict / Evaluate (auto)  

Includes Wizard recipes, Quick Cluster Color for geometry, and cross-platform install scripts.

## 打包步骤

1. 编译（推荐 Rhino 7 引用以兼容 7/8）  
   `dotnet build GHA_Project/SimpleML.csproj -c Release -p:RhinoMajorVersion=7`
2. 运行 `install.bat` 或 `install.sh` 验证本地安装  
3. 准备 Yak 目录：
   ```
   yak/dist/
     SimpleML.gha
     myML/   (components + core + requirements.txt + examples)
     manifest.yml
   ```
4. `yak build` / 上传 Food4Rhino  

## 截图建议

1. 新手向导 + 体检 PASS  
2. 智能训练 → 评估 Verdict  
3. 一键聚类上色（几何着色）  
4. Ribbon 全览（01–09）  

## 支持

邮箱：zhao_guijia@outlook.com  
仓库：https://github.com/kevinzju2014-sketch/SimpleML  
