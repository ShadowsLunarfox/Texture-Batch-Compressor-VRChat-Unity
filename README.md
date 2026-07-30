# Texture Batch Compressor

![Texture Batch Compressor Preview](docs/images/texture-batch-compressor-preview.png)

A multilingual Unity Editor tool for batch texture compression, built for VRChat world optimization.

## Languages

- [English](#english)
- [中文](#中文)
- [日本語](#日本語)
- [한국어](#한국어)

## English

Texture Batch Compressor helps Unity and VRChat creators scan one or more folders inside `Assets`, preview texture importer changes, and apply compression settings in bulk for PC and mobile targets.

### Features

- Multi-folder texture scanning with duplicate protection
- Presets for `Map > PC`, `Map > Mobile`, `Model > PC`, and `Model > Mobile`
- Standalone and Android platform overrides
- Dry Run preview before modifying assets
- Filters for normal maps, Sprite/UI textures, extensions, path keywords, and small textures
- Progress bar with cancel support
- Multilingual UI: English, Chinese, Japanese, and Korean
- Built-in guide window that follows the selected language

### Usage

Open from Unity:

```text
Tools > Texture Batch Compressor
```

Recommended flow: add target folders, scan all, choose a preset, preview with Dry Run, then apply after confirming the result.

## 中文

Texture Batch Compressor 是一个 Unity 编辑器批量贴图压缩工具，适合 VRChat 世界优化。它可以扫描 `Assets` 内的一个或多个文件夹，先预览导入设置变化，再批量应用电脑端或手机端贴图压缩设置。

### 功能

- 支持多个目标文件夹，并自动去重
- 预设分类：`地图 > 电脑`、`地图 > 手机`、`模型 > 电脑`、`模型 > 手机`
- 支持 Standalone 和 Android 平台覆盖
- 支持 Dry Run，仅预览不修改资源
- 可过滤法线贴图、Sprite/UI、扩展名、路径关键字和已小于目标尺寸的贴图
- 带进度条和取消功能
- 界面支持英文、中文、日语、韩语
- 内置教程窗口，内容会跟随当前语言切换

### 使用方式

在 Unity 菜单打开：

```text
Tools > Texture Batch Compressor
```

推荐流程：添加目标文件夹，扫描全部，选择预设，先用 Dry Run 预览，确认后再正式应用。

## 日本語

Texture Batch Compressor は、VRChat ワールド最適化向けの Unity Editor 用テクスチャ一括圧縮ツールです。`Assets` 内の 1 つ以上のフォルダーをスキャンし、インポート設定の変更内容を確認してから、PC またはモバイル向け設定を一括適用できます。

### 機能

- 複数フォルダーのスキャンと重複防止
- `マップ > PC`、`マップ > モバイル`、`モデル > PC`、`モデル > モバイル` のプリセット
- Standalone と Android のプラットフォーム上書き
- アセットを変更しない Dry Run プレビュー
- 法線マップ、Sprite/UI、拡張子、パスキーワード、小さいテクスチャのフィルター
- キャンセル可能な進行バー
- 英語、中国語、日本語、韓国語の UI
- 選択中の言語に追従する内蔵ガイドウィンドウ

### 使い方

Unity メニューから開きます：

```text
Tools > Texture Batch Compressor
```

推奨手順：対象フォルダーを追加し、すべてスキャンしてプリセットを選び、Dry Run で確認してから適用します。

## 한국어

Texture Batch Compressor는 VRChat 월드 최적화를 위한 Unity Editor 텍스처 일괄 압축 도구입니다. `Assets` 안의 하나 이상의 폴더를 스캔하고, 임포트 설정 변경을 먼저 확인한 뒤 PC 또는 모바일 대상 설정을 일괄 적용할 수 있습니다.

### 기능

- 여러 대상 폴더 스캔 및 중복 방지
- `맵 > PC`, `맵 > 모바일`, `모델 > PC`, `모델 > 모바일` 프리셋
- Standalone 및 Android 플랫폼 오버라이드
- 에셋을 수정하지 않는 Dry Run 미리보기
- 노멀 맵, Sprite/UI, 확장자, 경로 키워드, 작은 텍스처 필터
- 취소 가능한 진행 표시줄
- 영어, 중국어, 일본어, 한국어 UI
- 선택한 언어를 따라가는 내장 가이드 창

### 사용 방법

Unity 메뉴에서 엽니다:

```text
Tools > Texture Batch Compressor
```

권장 흐름: 대상 폴더를 추가하고, 모두 스캔한 뒤 프리셋을 선택하고, Dry Run으로 확인한 다음 적용합니다.

## Documentation

Full tutorial and risk notes:

[TextureBatchCompressor_UserGuide.md](TextureBatchCompressor_UserGuide.md)

## Important Risk Notice

This tool changes Unity texture importer settings and `.meta` files. Use version control or back up your project before applying large batches.
