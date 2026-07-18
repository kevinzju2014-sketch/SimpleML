"""
安装与运行环境体检。
"""

from __future__ import annotations

import os
import platform
import sys
from typing import Dict, List, Tuple

from core.env_bootstrap import bootstrap_python_paths, discover_site_env_dirs, ensure_project_on_path


REQUIRED_PACKAGES = [
    ("numpy", "1.20.0"),
    ("pandas", "1.3.0"),
    ("sklearn", "1.0.0"),
    ("joblib", "1.0.0"),
]

OPTIONAL_PACKAGES = [
    ("openpyxl", "3.0.0"),
]


def _pkg_version(mod_name: str) -> Tuple[bool, str]:
    try:
        if mod_name == "sklearn":
            import sklearn
            return True, getattr(sklearn, "__version__", "unknown")
        mod = __import__(mod_name)
        return True, getattr(mod, "__version__", "unknown")
    except Exception as exc:
        return False, str(exc)


def run_health_check(project_dir: str = None) -> Dict:
    added = bootstrap_python_paths(project_dir)
    root = ensure_project_on_path(project_dir)

    checks = []
    ok_count = 0

    def add(name, ok, detail):
        nonlocal ok_count
        checks.append({"name": name, "ok": bool(ok), "detail": detail})
        if ok:
            ok_count += 1

    add("操作系统", True, f"{platform.system()} {platform.release()} ({platform.machine()})")
    add("Python", True, f"{sys.version.split()[0]} @ {sys.executable}")
    add("SIMPLEML_PATH", bool(root and os.path.isdir(root)), root or "未找到项目路径")
    add("components 目录", bool(root and os.path.isdir(os.path.join(root, "components"))),
        os.path.join(root, "components") if root else "N/A")
    add("core 目录", bool(root and os.path.isdir(os.path.join(root, "core"))),
        os.path.join(root, "core") if root else "N/A")

    rhino_envs = discover_site_env_dirs()
    add("Rhino site-envs", True, f"发现 {len(rhino_envs)} 处: {', '.join(rhino_envs[:3]) or '无（将使用当前 Python）'}")

    for name, _min_ver in REQUIRED_PACKAGES:
        found, ver = _pkg_version(name)
        label = "scikit-learn" if name == "sklearn" else name
        add(f"依赖 {label}", found, ver if found else f"缺失: {ver}")

    for name, _min_ver in OPTIONAL_PACKAGES:
        found, ver = _pkg_version(name)
        add(f"可选 {name}", True, ver if found else f"未安装（Excel .xlsx 可能不可用）: {ver}")

    # 快速功能探测
    try:
        from sklearn.ensemble import RandomForestClassifier
        import numpy as np
        clf = RandomForestClassifier(n_estimators=5, random_state=42)
        X = np.array([[0, 0], [1, 1], [0, 1], [1, 0]])
        y = np.array([0, 1, 0, 1])
        clf.fit(X, y)
        pred = clf.predict([[0, 0]])
        add("sklearn 训练探测", True, f"预测样例={pred.tolist()}")
    except Exception as exc:
        add("sklearn 训练探测", False, str(exc))

    total = len(checks)
    required_failed = [c for c in checks if not c["ok"] and not c["name"].startswith("可选")]
    status = "PASS" if not required_failed else "FAIL"

    lines: List[str] = []
    lines.append("SimpleML 环境体检报告")
    lines.append("=" * 56)
    lines.append(f"状态: {status}  |  通过项: {ok_count}/{total}")
    lines.append(f"已注入路径数: {len(added)}")
    lines.append("")
    for c in checks:
        mark = "✓" if c["ok"] else "✗"
        lines.append(f"{mark} {c['name']}: {c['detail']}")

    lines.append("")
    lines.append("修复建议")
    lines.append("-" * 56)
    if required_failed:
        lines.append("1) 在当前 Python 中安装依赖:")
        lines.append("   python -m pip install -r requirements.txt")
        lines.append("   # 或: python -m pip install scikit-learn numpy pandas joblib openpyxl")
        lines.append("2) 若 Grasshopper 找不到代码，设置环境变量 SIMPLEML_PATH 指向插件 Python 根目录")
        lines.append("   （该目录应包含 components/ 与 core/）")
        lines.append("3) macOS/Linux 请确认使用 python3，并与 Rhino 调用的解释器一致")
    else:
        lines.append("环境正常。建议从 examples/ 打开示例工作流开始。")

    report = "\n".join(lines)
    summary = f"{status}: {ok_count}/{total} checks passed"
    return {
        "status": status,
        "summary": summary,
        "report": report,
        "checks": checks,
        "project_dir": root,
        "python": sys.executable,
    }
