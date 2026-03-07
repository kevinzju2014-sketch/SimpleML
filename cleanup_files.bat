@echo off
chcp 65001 >nul
echo ========================================
echo SimpleML 文件夹清理脚本
echo ========================================
echo.
echo 此脚本将删除过程文件和缓存文件
echo 只保留生成GHA文件必需的文件
echo.
pause

echo.
echo [1/5] 删除根目录过程文档...
del /Q "ALGORITHM_DEFAULTS_GUIDE.md" 2>nul
del /Q "ALGORITHMS_GUIDE.md" 2>nul
del /Q "CLEAN_FOR_DISTRIBUTION.md" 2>nul
del /Q "COMPONENTS_EXPLANATION.md" 2>nul
del /Q "COMPONENTS_REFERENCE.md" 2>nul
del /Q "DATASET_GUIDE.md" 2>nul
del /Q "FINAL_MODIFICATIONS_COMPLETE.md" 2>nul
del /Q "FINAL_STRUCTURE.md" 2>nul
del /Q "LOAD_MODEL_GUIDE.md" 2>nul
del /Q "MODIFICATION_SUMMARY.md" 2>nul
del /Q "README_CLEAN.md" 2>nul
del /Q "README_SIMPLEML.md" 2>nul
del /Q "SIMPLEML_CATEGORIES.md" 2>nul
del /Q "SIMPLEML_GRASSHOPPER_SETUP.md" 2>nul
del /Q "SIMPLEML_SUMMARY.md" 2>nul
del /Q "SPLIT_AND_EVALUATE_GUIDE.md" 2>nul
del /Q "WORKFLOW_DIAGRAM.md" 2>nul
echo 完成

echo.
echo [2/5] 删除根目录脚本文件...
del /Q "cleanup.bat" 2>nul
echo 完成

echo.
echo [3/5] 删除examples文件夹...
if exist "examples" (
    rmdir /S /Q "examples"
    echo 完成
) else (
    echo examples文件夹不存在，跳过
)

echo.
echo [4/5] 删除.vs缓存文件夹...
if exist ".vs" (
    rmdir /S /Q ".vs"
    echo 完成
) else (
    echo .vs文件夹不存在，跳过
)

echo.
echo [5/5] 删除GHA_Project中的过程文件...
cd GHA_Project

REM 删除过程文档
del /Q "COMPONENT_CATEGORY_UPDATE.md" 2>nul
del /Q "COMPONENT_FIX_SUMMARY.md" 2>nul
del /Q "COMPONENT_FIX_TEMPLATE.md" 2>nul
del /Q "COMPONENTS_COMPLETE.md" 2>nul
del /Q "COMPONENTS_DOCUMENTATION_FINAL.md" 2>nul
del /Q "COMPONENTS_STATUS.md" 2>nul
del /Q "COMPONENTS_VERIFICATION.md" 2>nul
del /Q "CSHARP_COMPONENTS_UPDATE_SUMMARY.md" 2>nul
del /Q "FINAL_COMPONENTS_UPDATE_SUMMARY.md" 2>nul
del /Q "FINAL_FIX_SUMMARY.md" 2>nul
del /Q "FIND_PYTHON_PATH.md" 2>nul
del /Q "FIX_COMPLETE.md" 2>nul
del /Q "FIX_ENCODING_ERROR.md" 2>nul
del /Q "FIX_IMPORT_ERROR_SIMPLE.md" 2>nul
del /Q "FIX_IMPORT_ERROR.md" 2>nul
del /Q "FIX_OPENPYXL_ERROR.md" 2>nul
del /Q "FIX_PANDAS_ERROR.md" 2>nul
del /Q "FIX_PYTHON_ENV_NOW.md" 2>nul
del /Q "FIX_RHINO_SITE_PACKAGES.md" 2>nul
del /Q "FIX_RHINO_VENV_SUMMARY.md" 2>nul
del /Q "FIX_SUMMARY.md" 2>nul
del /Q "FIX_UNICODE_ENCODING.md" 2>nul
del /Q "GHA_BUILD_GUIDE.md" 2>nul
del /Q "GUID_FIX_ALL.md" 2>nul
del /Q "GUID_FIX_COMPLETE.md" 2>nul
del /Q "GUID_FIX_FINAL.md" 2>nul
del /Q "GUID_FIX_SUMMARY.md" 2>nul
del /Q "INSTALL_GHA.md" 2>nul
del /Q "INSTALL_VISUAL_STUDIO.md" 2>nul
del /Q "NEW_COMPONENTS_UPDATE_SUMMARY.md" 2>nul
del /Q "PYTHON_ENV_FIX.md" 2>nul
del /Q "QUICK_BUILD.md" 2>nul
del /Q "QUICK_FIX_PANDAS.md" 2>nul
del /Q "QUICK_START.md" 2>nul
del /Q "README.md" 2>nul
del /Q "READY_FOR_BUILD.md" 2>nul
del /Q "RHINO_PYTHON_SETUP.md" 2>nul
del /Q "RHINO_VENV_FIX_COMPLETE.md" 2>nul
del /Q "RHINO_VENV_FIX_FINAL.md" 2>nul
del /Q "SETUP_RHINO_PYTHON.md" 2>nul
del /Q "TREE_STRUCTURE_UPDATE_SUMMARY.md" 2>nul
del /Q "UPDATE_READ_EXCEL_TREE.md" 2>nul
del /Q "USE_RHINO_PYTHON_FINAL.md" 2>nul
del /Q "USE_RHINO_PYTHON.md" 2>nul
del /Q "VISUAL_STUDIO_BUILD_GUIDE.md" 2>nul
del /Q "如何正确生成项目.md" 2>nul

REM 删除脚本文件
del /Q "create_algorithm_components.py" 2>nul
del /Q "create_remaining_components.md" 2>nul
del /Q "DIAGNOSE_PYTHON_ENV.bat" 2>nul
del /Q "find_msbuild.bat" 2>nul
del /Q "fix_all_components.py" 2>nul
del /Q "fix_unicode_encoding.py" 2>nul
del /Q "generate_all_components_v2.py" 2>nul
del /Q "generate_all_components.py" 2>nul
del /Q "generate_components.py" 2>nul
del /Q "INSTALL_OPENPYXL.bat" 2>nul
del /Q "INSTALL_PANDAS_RHINO.bat" 2>nul
del /Q "INSTALL_RHINO_PYTHON_LIBS.bat" 2>nul
del /Q "SET_PYTHON_PATH.bat" 2>nul
del /Q "update_algorithms.py" 2>nul
del /Q "update_all_components_site_packages.py" 2>nul
del /Q "UPDATE_ALL_PYTHON_FILES.bat" 2>nul
del /Q "update_python_files.py" 2>nul

REM 删除.vs缓存文件夹
if exist ".vs" (
    rmdir /S /Q ".vs"
)

cd ..

echo 完成

echo.
echo ========================================
echo 清理完成！
echo ========================================
echo.
echo 保留的文件：
echo - GHA_Project/SimpleML.csproj
echo - GHA_Project/SimpleMLPlugin.cs
echo - GHA_Project/PythonScriptExecutor.cs
echo - GHA_Project/TreeConverter.cs
echo - GHA_Project/Components/*.cs
echo - GHA_Project/ICON_DESIGN_GUIDE.md
echo - GHA_Project/ICON_CHECKLIST.md
echo - GHA_Project/COMPONENTS_DOCUMENTATION.md
echo - components/*.py
echo - core/*.py
echo - requirements.txt
echo.
echo 请验证项目仍能正常编译！
echo.
pause
