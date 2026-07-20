"""
Simple UI language helper for Python-side explanations / verdicts.
Default: English. Reads SIMPLEML_LANG (en|zh).
"""

from __future__ import annotations

import os
from typing import Dict


def lang() -> str:
    raw = (os.environ.get("SIMPLEML_LANG") or "en").strip().lower()
    if raw in ("zh", "zh-cn", "zh_cn", "cn", "chinese", "中文"):
        return "zh"
    return "en"


def is_zh() -> bool:
    return lang() == "zh"


def t(en: str, zh: str) -> str:
    return zh if is_zh() else en


def pick(mapping: Dict[str, str]) -> str:
    return mapping.get(lang(), mapping.get("en", ""))
