#!/usr/bin/env bash

INPUT_FILE="CHANGELOG.md"
OUTPUT_FILE="LATEST_CHANGELOG.md"

if [ ! -f "$INPUT_FILE" ]; then
    echo "找不到文件：$INPUT_FILE"
    exit 1
fi

# 找到所有以 "## " 开头的行号
mapfile -t headers < <(grep -n "^## " "$INPUT_FILE" | cut -d: -f1)

if [ ${#headers[@]} -eq 0 ]; then
    echo "CHANGELOG.md 中没有找到任何版本标题（## 开头）"
    exit 1
fi

start=${headers[0]}

# 如果只有一个版本标题
if [ ${#headers[@]} -eq 1 ]; then
    sed -n "${start},\$p" "$INPUT_FILE" > "$OUTPUT_FILE"
else
    next=${headers[1]}
    sed -n "${start},$((next-1))p" "$INPUT_FILE" > "$OUTPUT_FILE"
fi

echo "最新版本内容已写入：$OUTPUT_FILE"
