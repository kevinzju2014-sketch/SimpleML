# myML 文件夹清理计划

## 保留的文件（生成GHA必需）

### GHA_Project 文件夹
**必需文件：**
- `SimpleML.csproj` - 项目文件
- `SimpleMLPlugin.cs` - 插件信息
- `PythonScriptExecutor.cs` - Python执行器
- `TreeConverter.cs` - Tree转换器
- `Components/` - 所有C#组件文件（.cs文件）

**保留的文档：**
- `ICON_DESIGN_GUIDE.md` - 图标设计指南
- `ICON_CHECKLIST.md` - 图标检查清单
- `COMPONENTS_DOCUMENTATION.md` - 组件文档（可选，但建议保留）

### components 文件夹
**必需文件：**
- 所有 `.py` 文件（运行时需要）

### core 文件夹
**必需文件：**
- 所有 `.py` 文件（运行时需要）

### 根目录
**必需文件：**
- `requirements.txt` - Python依赖说明（可选，但建议保留）

---

## 可以删除的文件

### 根目录文档文件（过程文档）
- `ALGORITHM_DEFAULTS_GUIDE.md`
- `ALGORITHMS_GUIDE.md`
- `CLEAN_FOR_DISTRIBUTION.md`
- `COMPONENTS_EXPLANATION.md`
- `COMPONENTS_REFERENCE.md`
- `DATASET_GUIDE.md`
- `FINAL_MODIFICATIONS_COMPLETE.md`
- `FINAL_STRUCTURE.md`
- `LOAD_MODEL_GUIDE.md`
- `MODIFICATION_SUMMARY.md`
- `README_CLEAN.md`
- `README_SIMPLEML.md`
- `README.md`（可选，可以保留一个）
- `SIMPLEML_CATEGORIES.md`
- `SIMPLEML_GRASSHOPPER_SETUP.md`
- `SIMPLEML_SUMMARY.md`
- `SPLIT_AND_EVALUATE_GUIDE.md`
- `WORKFLOW_DIAGRAM.md`

### 根目录脚本文件
- `cleanup.bat`

### examples 文件夹
- 整个 `examples/` 文件夹（示例文件，不是运行时必需）

### .vs 文件夹
- 整个 `.vs/` 文件夹（Visual Studio缓存，可以删除）

### GHA_Project 文件夹中的过程文件

**文档文件（过程文档）：**
- `COMPONENT_CATEGORY_UPDATE.md`
- `COMPONENT_FIX_SUMMARY.md`
- `COMPONENT_FIX_TEMPLATE.md`
- `COMPONENTS_COMPLETE.md`
- `COMPONENTS_DOCUMENTATION_FINAL.md`
- `COMPONENTS_STATUS.md`
- `COMPONENTS_VERIFICATION.md`
- `CSHARP_COMPONENTS_UPDATE_SUMMARY.md`
- `FINAL_COMPONENTS_UPDATE_SUMMARY.md`
- `FINAL_FIX_SUMMARY.md`
- `FIND_PYTHON_PATH.md`
- `FIX_COMPLETE.md`
- `FIX_ENCODING_ERROR.md`
- `FIX_IMPORT_ERROR_SIMPLE.md`
- `FIX_IMPORT_ERROR.md`
- `FIX_OPENPYXL_ERROR.md`
- `FIX_PANDAS_ERROR.md`
- `FIX_PYTHON_ENV_NOW.md`
- `FIX_RHINO_SITE_PACKAGES.md`
- `FIX_RHINO_VENV_SUMMARY.md`
- `FIX_SUMMARY.md`
- `FIX_UNICODE_ENCODING.md`
- `GHA_BUILD_GUIDE.md`
- `GUID_FIX_ALL.md`
- `GUID_FIX_COMPLETE.md`
- `GUID_FIX_FINAL.md`
- `GUID_FIX_SUMMARY.md`
- `INSTALL_GHA.md`
- `INSTALL_VISUAL_STUDIO.md`
- `NEW_COMPONENTS_UPDATE_SUMMARY.md`
- `PYTHON_ENV_FIX.md`
- `QUICK_BUILD.md`
- `QUICK_FIX_PANDAS.md`
- `QUICK_START.md`
- `README.md`
- `READY_FOR_BUILD.md`
- `RHINO_PYTHON_SETUP.md`
- `RHINO_VENV_FIX_COMPLETE.md`
- `RHINO_VENV_FIX_FINAL.md`
- `SETUP_RHINO_PYTHON.md`
- `TREE_STRUCTURE_UPDATE_SUMMARY.md`
- `UPDATE_READ_EXCEL_TREE.md`
- `USE_RHINO_PYTHON_FINAL.md`
- `USE_RHINO_PYTHON.md`
- `VISUAL_STUDIO_BUILD_GUIDE.md`
- `如何正确生成项目.md`

**脚本文件：**
- `create_algorithm_components.py`
- `create_remaining_components.md`
- `DIAGNOSE_PYTHON_ENV.bat`
- `find_msbuild.bat`
- `fix_all_components.py`
- `FIX_IMPORT_ERROR_SIMPLE.md`
- `fix_unicode_encoding.py`
- `generate_all_components_v2.py`
- `generate_all_components.py`
- `generate_components.py`
- `INSTALL_OPENPYXL.bat`
- `INSTALL_PANDAS_RHINO.bat`
- `INSTALL_RHINO_PYTHON_LIBS.bat`
- `SET_PYTHON_PATH.bat`
- `update_algorithms.py`
- `update_all_components_site_packages.py`
- `UPDATE_ALL_PYTHON_FILES.bat`
- `update_python_files.py`

**缓存文件夹：**
- `GHA_Project/.vs/` - Visual Studio缓存

---

## 清理后的文件夹结构

```
myML/
├── .gitignore
├── requirements.txt
├── components/
│   ├── __init__.py
│   ├── algorithms/
│   │   └── [所有.py文件]
│   └── [所有.py文件]
├── core/
│   └── [所有.py文件]
└── GHA_Project/
    ├── SimpleML.csproj
    ├── SimpleMLPlugin.cs
    ├── PythonScriptExecutor.cs
    ├── TreeConverter.cs
    ├── Components/
    │   └── [所有.cs文件]
    ├── ICON_DESIGN_GUIDE.md
    ├── ICON_CHECKLIST.md
    └── COMPONENTS_DOCUMENTATION.md（可选）
```

---

## 清理步骤

1. 备份整个myML文件夹（以防万一）
2. 删除列出的过程文件
3. 删除.vs文件夹
4. 删除examples文件夹
5. 验证GHA项目仍能正常编译
