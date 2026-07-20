"""
跨平台环境引导：自动发现 Rhino 7+ / Rhino 8+ / macOS 的 site-packages。
避免写死用户名、单一 py39-rh8 或 Windows 绝对路径。
"""

from __future__ import annotations

import os
import site
import sys
from pathlib import Path
from typing import Iterable, List


def _unique_existing(paths: Iterable[str]) -> List[str]:
    seen = set()
    result = []
    for p in paths:
        if not p:
            continue
        norm = os.path.normpath(p)
        if norm in seen:
            continue
        if os.path.isdir(norm):
            seen.add(norm)
            result.append(norm)
    return result


def iter_rhinocode_roots() -> List[Path]:
    """返回可能的 .rhinocode 根目录（Windows / macOS）。"""
    roots = []
    home = Path.home()
    roots.append(home / ".rhinocode")

    for base in (
        os.environ.get("APPDATA"),
        os.environ.get("LOCALAPPDATA"),
        os.environ.get("USERPROFILE"),
    ):
        if base:
            roots.append(Path(base) / ".rhinocode")

    # macOS Rhino / Rhinocode 常见位置
    roots.append(home / "Library" / "Application Support" / "McNeel" / "Rhinoceros" / ".rhinocode")
    roots.append(home / "Library" / "Application Support" / ".rhinocode")

    unique = []
    seen = set()
    for r in roots:
        key = str(r)
        if key not in seen:
            seen.add(key)
            unique.append(r)
    return unique


def _python_lib_site_packages(env_root: Path) -> List[Path]:
    """枚举环境内可能的 site-packages（含多 Python 小版本）。"""
    found = []
    lib = env_root / "lib"
    if lib.is_dir():
        try:
            for child in lib.iterdir():
                if child.is_dir() and child.name.startswith("python"):
                    sp = child / "site-packages"
                    if sp.is_dir():
                        found.append(sp)
        except OSError:
            pass
    for candidate in (
        env_root / "Lib" / "site-packages",
        env_root / "lib" / "site-packages",
        env_root / "site-packages",
    ):
        if candidate.is_dir():
            found.append(candidate)
    return found


def discover_site_env_dirs() -> List[str]:
    """发现 Rhinocode site-envs / 环境目录（rh7/rh8/rh9…）。"""
    found = []
    for root in iter_rhinocode_roots():
        if not root.exists():
            continue
        try:
            children = sorted(root.iterdir(), key=lambda p: p.name, reverse=True)
        except OSError:
            continue
        for child in children:
            if not child.is_dir():
                continue
            site_envs = child / "site-envs"
            if site_envs.is_dir():
                found.append(str(site_envs))
            for sp in _python_lib_site_packages(child):
                found.append(str(sp))
    return _unique_existing(found)


def expand_site_env_packages(site_envs_dirs: Iterable[str]) -> List[str]:
    packages = []
    for site_envs in site_envs_dirs:
        # 既可能是 site-envs 父目录，也可能已是 site-packages
        base = Path(site_envs)
        if base.name == "site-packages":
            packages.append(str(base))
            continue
        try:
            for item in os.listdir(site_envs):
                env_path = os.path.join(site_envs, item)
                if not os.path.isdir(env_path):
                    continue
                packages.append(env_path)
                for sp in _python_lib_site_packages(Path(env_path)):
                    packages.append(str(sp))
        except OSError:
            continue
    return _unique_existing(packages)


def ensure_project_on_path(project_dir: str = None) -> str:
    """确保 SimpleML 项目根目录在 sys.path 中。"""
    if project_dir is None:
        env = os.environ.get("SIMPLEML_PATH")
        if env and os.path.isdir(env):
            project_dir = env
        else:
            current_dir = os.path.dirname(os.path.abspath(__file__))
            project_dir = os.path.dirname(current_dir)

    if project_dir and project_dir not in sys.path:
        sys.path.insert(0, project_dir)
    return project_dir


def bootstrap_python_paths(project_dir: str = None) -> List[str]:
    """
    配置 sys.path，返回已添加的路径列表。
    可在任意组件模块顶部调用。
    """
    added = []
    root = ensure_project_on_path(project_dir)
    if root:
        added.append(root)

    try:
        for sp in site.getsitepackages():
            if sp not in sys.path and os.path.isdir(sp):
                sys.path.insert(0, sp)
                added.append(sp)
    except Exception:
        pass

    try:
        usp = site.getusersitepackages()
        if usp and usp not in sys.path and os.path.isdir(usp):
            sys.path.insert(0, usp)
            added.append(usp)
    except Exception:
        pass

    for pkg in expand_site_env_packages(discover_site_env_dirs()):
        if pkg not in sys.path:
            sys.path.insert(0, pkg)
            added.append(pkg)

    return added


# 模块导入时自动引导（保持与旧组件兼容）
bootstrap_python_paths()
