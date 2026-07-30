using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureBatchCompressor : EditorWindow
{
    // EditorPrefs key prefix used to remember tool settings between Unity sessions.
    private const string PrefPrefix = "TextureBatchCompressor.";

    // Shared option data for texture sizes, keyword parsing, and the UI language selector.
    private static readonly string[] MaxSizeLabels = { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
    private static readonly int[] MaxSizeValues = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
    private static readonly char[] KeywordSeparators = { ',', ';', '\n', '\r' };
    private static readonly string[] LanguageLabels = { "English", "中文", "日本語", "한국어" };

    // Supported interface languages. The enum order must match LanguageLabels and TextTable values.
    private enum ToolLanguage
    {
        English,
        Chinese,
        Japanese,
        Korean
    }

    // Lightweight built-in localization table. Each value array is ordered as English, Chinese, Japanese, Korean.
    private static readonly Dictionary<string, string[]> TextTable = new Dictionary<string, string[]>
    {
        { "Texture Compressor", new[] { "Texture Compressor", "贴图压缩工具", "テクスチャ圧縮", "텍스처 압축기" } },
        { "Batch Texture Compression Tool", new[] { "Batch Texture Compression Tool", "批量贴图压缩工具", "テクスチャ一括圧縮ツール", "텍스처 일괄 압축 도구" } },
        { "Language", new[] { "Language", "语言", "言語", "언어" } },
        { "Scanned: {0} | Ready: {1} | Skipped: {2}", new[] { "Scanned: {0} | Ready: {1} | Skipped: {2}", "已扫描：{0} | 可处理：{1} | 已跳过：{2}", "スキャン済み: {0} | 処理対象: {1} | スキップ: {2}", "스캔됨: {0} | 처리 가능: {1} | 건너뜀: {2}" } },
        { "Target Folders", new[] { "Target Folders", "目标文件夹", "対象フォルダー", "대상 폴더" } },
        { "Folder {0}", new[] { "Folder {0}", "文件夹 {0}", "フォルダー {0}", "폴더 {0}" } },
        { "Browse", new[] { "Browse", "浏览", "参照", "찾아보기" } },
        { "Select Folder", new[] { "Select Folder", "选择文件夹", "フォルダーを選択", "폴더 선택" } },
        { "Remove", new[] { "Remove", "移除", "削除", "제거" } },
        { "Add Folder:", new[] { "Add Folder:", "添加文件夹:", "フォルダーを追加:", "폴더 추가:" } },
        { "Add", new[] { "Add", "添加", "追加", "추가" } },
        { "Browse Add", new[] { "Browse Add", "浏览添加", "参照して追加", "찾아 추가" } },
        { "Add Folder", new[] { "Add Folder", "添加文件夹", "フォルダーを追加", "폴더 추가" } },
        { "Scan All", new[] { "Scan All", "扫描全部", "すべてスキャン", "모두 스캔" } },
        { "Presets", new[] { "Presets", "预设", "プリセット", "프리셋" } },
        { "Map", new[] { "Map", "地图", "マップ", "맵" } },
        { "Model", new[] { "Model", "模型", "モデル", "모델" } },
        { "PC", new[] { "PC", "电脑", "PC", "PC" } },
        { "Mobile", new[] { "Mobile", "手机", "モバイル", "모바일" } },
        { "Filters", new[] { "Filters", "过滤", "フィルター", "필터" } },
        { "Skip Normal Maps", new[] { "Skip Normal Maps", "跳过法线贴图", "法線マップをスキップ", "노멀 맵 건너뛰기" } },
        { "Skip Sprites / UI", new[] { "Skip Sprites / UI", "跳过 Sprite / UI", "Sprite / UI をスキップ", "스프라이트 / UI 건너뛰기" } },
        { "Skip Textures Already Under Target Size", new[] { "Skip Textures Already Under Target Size", "跳过已小于目标尺寸的贴图", "すでに目標サイズ以下のテクスチャをスキップ", "이미 목표 크기 이하인 텍스처 건너뛰기" } },
        { "Only Process Selected Extensions", new[] { "Only Process Selected Extensions", "只处理选中的扩展名", "選択した拡張子のみ処理", "선택한 확장자만 처리" } },
        { "Exclude Path Keywords", new[] { "Exclude Path Keywords", "排除路径关键字", "除外パスキーワード", "제외 경로 키워드" } },
        { "Default Import Settings", new[] { "Default Import Settings", "默认导入设置", "デフォルトインポート設定", "기본 임포트 설정" } },
        { "Max Size", new[] { "Max Size", "最大尺寸", "最大サイズ", "최대 크기" } },
        { "Compression Mode", new[] { "Compression Mode", "压缩模式", "圧縮モード", "압축 모드" } },
        { "Use Crunch Compression", new[] { "Use Crunch Compression", "使用 Crunch 压缩", "Crunch 圧縮を使用", "Crunch 압축 사용" } },
        { "Crunch Quality", new[] { "Crunch Quality", "Crunch 质量", "Crunch 品質", "Crunch 품질" } },
        { "Platform Overrides", new[] { "Platform Overrides", "平台覆盖", "プラットフォーム上書き", "플랫폼 오버라이드" } },
        { "Override {0}", new[] { "Override {0}", "覆盖 {0}", "{0} を上書き", "{0} 오버라이드" } },
        { "{0} Max Size", new[] { "{0} Max Size", "{0} 最大尺寸", "{0} 最大サイズ", "{0} 최대 크기" } },
        { "{0} Format", new[] { "{0} Format", "{0} 格式", "{0} 形式", "{0} 포맷" } },
        { "Compression and Crunch settings follow the default import settings.", new[] { "Compression and Crunch settings follow the default import settings.", "压缩和 Crunch 设置会沿用默认导入设置。", "圧縮と Crunch 設定はデフォルトインポート設定に従います。", "압축 및 Crunch 설정은 기본 임포트 설정을 따릅니다." } },
        { "Actions", new[] { "Actions", "操作", "操作", "작업" } },
        { "Dry Run / Preview Only", new[] { "Dry Run / Preview Only", "仅预览 / 不应用", "ドライラン / プレビューのみ", "드라이 런 / 미리보기 전용" } },
        { "Refresh Preview", new[] { "Refresh Preview", "刷新预览", "プレビュー更新", "미리보기 새로고침" } },
        { "Run Dry Preview", new[] { "Run Dry Preview", "运行预览", "ドライプレビュー実行", "드라이 미리보기 실행" } },
        { "Apply to Ready Textures", new[] { "Apply to Ready Textures", "应用到可处理贴图", "処理対象テクスチャに適用", "처리 가능 텍스처에 적용" } },
        { "Texture Preview", new[] { "Texture Preview", "贴图预览", "テクスチャプレビュー", "텍스처 미리보기" } },
        { "Scan a folder to list textures.", new[] { "Scan a folder to list textures.", "扫描文件夹后显示贴图列表。", "フォルダーをスキャンするとテクスチャ一覧が表示されます。", "폴더를 스캔하면 텍스처 목록이 표시됩니다." } },
        { "Texture", new[] { "Texture", "贴图", "テクスチャ", "텍스처" } },
        { "Size", new[] { "Size", "尺寸", "サイズ", "크기" } },
        { "Max", new[] { "Max", "最大", "最大", "최대" } },
        { "Compression", new[] { "Compression", "压缩", "圧縮", "압축" } },
        { "Crunch", new[] { "Crunch", "Crunch", "Crunch", "Crunch" } },
        { "Type", new[] { "Type", "类型", "種類", "유형" } },
        { "Status", new[] { "Status", "状态", "状態", "상태" } },
        { "Yes ({0})", new[] { "Yes ({0})", "是 ({0})", "はい ({0})", "예 ({0})" } },
        { "No", new[] { "No", "否", "いいえ", "아니요" } },
        { "Ready", new[] { "Ready", "就绪", "処理対象", "준비됨" } },
        { "Skipped: {0}", new[] { "Skipped: {0}", "已跳过：{0}", "スキップ: {0}", "건너뜀: {0}" } },
        { "Invalid Folder", new[] { "Invalid Folder", "无效文件夹", "無効なフォルダー", "잘못된 폴더" } },
        { "Scan failed: {0}", new[] { "Scan failed: {0}", "扫描失败：{0}", "スキャン失敗: {0}", "스캔 실패: {0}" } },
        { "Scan complete. Found {0} unique texture(s) in {1}.", new[] { "Scan complete. Found {0} unique texture(s) in {1}.", "扫描完成。在 {1} 中找到 {0} 张不重复贴图。", "スキャン完了。{1} で重複なしのテクスチャ {0} 個を検出しました。", "스캔 완료. {1}에서 중복 없는 텍스처 {0}개를 찾았습니다." } },
        { "missing importer", new[] { "missing importer", "缺少导入器", "インポーターなし", "임포터 없음" } },
        { "normal map", new[] { "normal map", "法线贴图", "法線マップ", "노멀 맵" } },
        { "sprite / UI", new[] { "sprite / UI", "Sprite / UI", "Sprite / UI", "스프라이트 / UI" } },
        { "extension", new[] { "extension", "扩展名不匹配", "拡張子", "확장자" } },
        { "excluded keyword: {0}", new[] { "excluded keyword: {0}", "排除关键字：{0}", "除外キーワード: {0}", "제외 키워드: {0}" } },
        { "already smaller than target", new[] { "already smaller than target", "已小于目标尺寸", "すでに目標サイズ以下", "이미 목표 크기 이하" } },
        { "Preview complete. Ready: {0}, skipped: {1}.", new[] { "Preview complete. Ready: {0}, skipped: {1}.", "预览完成。可处理：{0}，已跳过：{1}。", "プレビュー完了。処理対象: {0}、スキップ: {1}。", "미리보기 완료. 처리 가능: {0}, 건너뜀: {1}." } },
        { "Target default settings: max {0}, {1}, crunch {2}.", new[] { "Target default settings: max {0}, {1}, crunch {2}.", "目标默认设置：最大 {0}，{1}，Crunch {2}。", "対象デフォルト設定: 最大 {0}、{1}、Crunch {2}。", "대상 기본 설정: 최대 {0}, {1}, Crunch {2}." } },
        { "on", new[] { "on", "开启", "オン", "켜짐" } },
        { "off", new[] { "off", "关闭", "オフ", "꺼짐" } },
        { "Standalone override: max {0}, {1}.", new[] { "Standalone override: max {0}, {1}.", "Standalone 覆盖：最大 {0}，{1}。", "Standalone 上書き: 最大 {0}、{1}。", "Standalone 오버라이드: 최대 {0}, {1}." } },
        { "Android override: max {0}, {1}.", new[] { "Android override: max {0}, {1}.", "Android 覆盖：最大 {0}，{1}。", "Android 上書き: 最大 {0}、{1}。", "Android 오버라이드: 최대 {0}, {1}." } },
        { "Will process: {0} | {1}", new[] { "Will process: {0} | {1}", "将处理：{0} | {1}", "処理予定: {0} | {1}", "처리 예정: {0} | {1}" } },
        { "Skipped: {0} | {1}", new[] { "Skipped: {0} | {1}", "已跳过：{0} | {1}", "スキップ: {0} | {1}", "건너뜀: {0} | {1}" } },
        { "current max {0}, {1}, crunch {2} -> max {3}, {4}, crunch {5}", new[] { "current max {0}, {1}, crunch {2} -> max {3}, {4}, crunch {5}", "当前最大 {0}，{1}，Crunch {2} -> 最大 {3}，{4}，Crunch {5}", "現在 最大 {0}、{1}、Crunch {2} -> 最大 {3}、{4}、Crunch {5}", "현재 최대 {0}, {1}, Crunch {2} -> 최대 {3}, {4}, Crunch {5}" } },
        { "Standalone {0} -> max {1}, {2}", new[] { "Standalone {0} -> max {1}, {2}", "Standalone {0} -> 最大 {1}，{2}", "Standalone {0} -> 最大 {1}、{2}", "Standalone {0} -> 최대 {1}, {2}" } },
        { "Android {0} -> max {1}, {2}", new[] { "Android {0} -> max {1}, {2}", "Android {0} -> 最大 {1}，{2}", "Android {0} -> 最大 {1}、{2}", "Android {0} -> 최대 {1}, {2}" } },
        { "not overridden", new[] { "not overridden", "未覆盖", "上書きなし", "오버라이드 안 됨" } },
        { "No textures are ready to process. Skipped: {0}.", new[] { "No textures are ready to process. Skipped: {0}.", "没有可处理贴图。已跳过：{0}。", "処理対象のテクスチャはありません。スキップ: {0}。", "처리 가능한 텍스처가 없습니다. 건너뜀: {0}." } },
        { "Nothing to Process", new[] { "Nothing to Process", "没有可处理内容", "処理対象なし", "처리할 항목 없음" } },
        { "Applying Texture Compression", new[] { "Applying Texture Compression", "正在应用贴图压缩", "テクスチャ圧縮を適用中", "텍스처 압축 적용 중" } },
        { "Canceled by user.", new[] { "Canceled by user.", "用户已取消。", "ユーザーがキャンセルしました。", "사용자가 취소했습니다." } },
        { "Failed: {0} | {1}", new[] { "Failed: {0} | {1}", "失败：{0} | {1}", "失敗: {0} | {1}", "실패: {0} | {1}" } },
        { "Processed: {0} | {1}", new[] { "Processed: {0} | {1}", "已处理：{0} | {1}", "処理済み: {0} | {1}", "처리됨: {0} | {1}" } },
        { "Canceled", new[] { "Canceled", "已取消", "キャンセル済み", "취소됨" } },
        { "Complete", new[] { "Complete", "完成", "完了", "완료" } },
        { "{0}. Processed: {1}, skipped: {2}, failed: {3}.", new[] { "{0}. Processed: {1}, skipped: {2}, failed: {3}.", "{0}。已处理：{1}，已跳过：{2}，失败：{3}。", "{0}。処理済み: {1}、スキップ: {2}、失敗: {3}。", "{0}. 처리됨: {1}, 건너뜀: {2}, 실패: {3}." } },
        { "You must select a folder inside the Assets directory.", new[] { "You must select a folder inside the Assets directory.", "必须选择 Assets 目录内的文件夹。", "Assets ディレクトリ内のフォルダーを選択してください。", "Assets 디렉터리 안의 폴더를 선택해야 합니다." } },
        { "Folder {0}: {1}", new[] { "Folder {0}: {1}", "文件夹 {0}：{1}", "フォルダー {0}: {1}", "폴더 {0}: {1}" } },
        { "Folder path is empty.", new[] { "Folder path is empty.", "文件夹路径为空。", "フォルダーパスが空です。", "폴더 경로가 비어 있습니다." } },
        { "Folder path must be inside the Assets directory.", new[] { "Folder path must be inside the Assets directory.", "文件夹路径必须在 Assets 目录内。", "フォルダーパスは Assets ディレクトリ内である必要があります。", "폴더 경로는 Assets 디렉터리 안에 있어야 합니다." } },
        { "Folder does not exist: {0}", new[] { "Folder does not exist: {0}", "文件夹不存在：{0}", "フォルダーが存在しません: {0}", "폴더가 없습니다: {0}" } },
        { "Folder Already Added", new[] { "Folder Already Added", "文件夹已添加", "フォルダーは追加済み", "이미 추가된 폴더" } },
        { "{0} is already in the target list.", new[] { "{0} is already in the target list.", "{0} 已经在目标列表中。", "{0} はすでに対象リストにあります。", "{0}은 이미 대상 목록에 있습니다." } },
        { "Added folder: {0}", new[] { "Added folder: {0}", "已添加文件夹：{0}", "フォルダーを追加しました: {0}", "폴더 추가됨: {0}" } },
        { "Loaded preset: Map PC.", new[] { "Loaded preset: Map PC.", "已加载预设：地图 / 电脑。", "プリセットを読み込みました: マップ / PC。", "프리셋 로드됨: 맵 / PC." } },
        { "Loaded preset: Map Mobile.", new[] { "Loaded preset: Map Mobile.", "已加载预设：地图 / 手机。", "プリセットを読み込みました: マップ / モバイル。", "프리셋 로드됨: 맵 / 모바일." } },
        { "Loaded preset: Model PC.", new[] { "Loaded preset: Model PC.", "已加载预设：模型 / 电脑。", "プリセットを読み込みました: モデル / PC。", "프리셋 로드됨: 모델 / PC." } },
        { "Loaded preset: Model Mobile.", new[] { "Loaded preset: Model Mobile.", "已加载预设：模型 / 手机。", "プリセットを読み込みました: モデル / モバイル。", "프리셋 로드됨: 모델 / 모바일." } },
        { "Guide", new[] { "Guide", "教程", "ガイド", "가이드" } },
        { "Usage Guide", new[] { "Usage Guide", "使用教程", "使い方ガイド", "사용 가이드" } },
        { "Guide Intro", new[] { "This guide explains the recommended workflow for batch texture compression.", "本教程说明批量贴图压缩工具的推荐使用流程。", "このガイドでは、テクスチャ一括圧縮ツールの推奨ワークフローを説明します。", "이 가이드는 텍스처 일괄 압축 도구의 권장 사용 흐름을 설명합니다." } },
        { "Guide Basic Workflow", new[] { "Basic Workflow", "基本流程", "基本手順", "기본 흐름" } },
        { "Guide Basic Workflow Body", new[] { "1. Add one or more target folders inside Assets.\n2. Click Scan All.\n3. Choose a Map or Model preset for PC or Mobile.\n4. Adjust filters and import settings if needed.\n5. Keep Dry Run enabled and run a preview.\n6. If the preview is correct, disable Dry Run and apply.", "1. 添加一个或多个 Assets 内的目标文件夹。\n2. 点击“扫描全部”。\n3. 按地图或模型选择电脑/手机预设。\n4. 按需要调整过滤和导入设置。\n5. 保持“仅预览 / 不应用”开启并先运行预览。\n6. 预览确认无误后，关闭仅预览并应用。", "1. Assets 内の対象フォルダーを 1 つ以上追加します。\n2. “すべてスキャン”をクリックします。\n3. マップまたはモデルの PC / モバイルプリセットを選びます。\n4. 必要に応じてフィルターとインポート設定を調整します。\n5. ドライランを有効にしたままプレビューします。\n6. 内容を確認したら、ドライランを無効にして適用します。", "1. Assets 안의 대상 폴더를 하나 이상 추가합니다.\n2. 모두 스캔을 클릭합니다.\n3. 맵 또는 모델의 PC / 모바일 프리셋을 선택합니다.\n4. 필요하면 필터와 임포트 설정을 조정합니다.\n5. 드라이 런을 켠 상태로 먼저 미리보기를 실행합니다.\n6. 미리보기가 맞으면 드라이 런을 끄고 적용합니다." } },
        { "Guide Presets", new[] { "Preset Categories", "预设分类", "プリセット分類", "프리셋 분류" } },
        { "Guide Presets Body", new[] { "Map PC keeps good quality for world textures.\nMap Mobile reduces texture size for Quest or Android builds.\nModel PC keeps more detail for close-up model textures.\nModel Mobile balances model clarity and mobile size.", "地图 / 电脑：地图贴图质量优先。\n地图 / 手机：降低贴图尺寸，适合 Quest 或 Android。\n模型 / 电脑：保留模型近看细节。\n模型 / 手机：兼顾模型清晰度和手机端体积。", "マップ / PC: ワールド用テクスチャの品質を優先します。\nマップ / モバイル: Quest / Android 向けにサイズを抑えます。\nモデル / PC: 近くで見るモデルの細部を残します。\nモデル / モバイル: モデルの見やすさと容量を両立します。", "맵 / PC: 월드 텍스처 품질을 우선합니다.\n맵 / 모바일: Quest 또는 Android용으로 크기를 줄입니다.\n모델 / PC: 가까이 보이는 모델 디테일을 더 유지합니다.\n모델 / 모바일: 모델 선명도와 모바일 용량을 균형 있게 맞춥니다." } },
        { "Guide Filters", new[] { "Filters", "过滤", "フィルター", "필터" } },
        { "Guide Filters Body", new[] { "Use filters to avoid changing sensitive textures.\nSkip Normal Maps protects lighting details.\nSkip Sprites / UI protects interface textures.\nExclude Path Keywords can skip folders or files such as UI, Normal, or NoCompress.", "使用过滤可以避免误改敏感贴图。\n跳过法线贴图可保护光照细节。\n跳过 Sprite / UI 可保护界面贴图。\n排除路径关键字可跳过 UI、Normal、NoCompress 等路径。", "フィルターを使うと、重要なテクスチャの誤変更を避けられます。\n法線マップのスキップはライティングの細部を守ります。\nSprite / UI のスキップは UI テクスチャを守ります。\n除外パスキーワードで UI、Normal、NoCompress などを除外できます。", "필터를 사용하면 민감한 텍스처가 잘못 변경되는 것을 줄일 수 있습니다.\n노멀 맵 건너뛰기는 조명 디테일을 보호합니다.\n스프라이트 / UI 건너뛰기는 UI 텍스처를 보호합니다.\n제외 경로 키워드로 UI, Normal, NoCompress 같은 경로를 제외할 수 있습니다." } },
        { "Guide Dry Run", new[] { "Preview First", "先预览", "先にプレビュー", "먼저 미리보기" } },
        { "Guide Dry Run Body", new[] { "Dry Run does not modify assets. It lists which textures will be processed, which will be skipped, and what settings will change. Use it before every large batch.", "仅预览不会修改资源。它会列出哪些贴图会被处理、哪些会被跳过，以及设置会如何变化。每次大批量处理前都建议先预览。", "ドライランではアセットは変更されません。処理対象、スキップ対象、変更予定の設定を確認できます。大量処理の前には必ず使うことをおすすめします。", "드라이 런은 에셋을 수정하지 않습니다. 처리될 텍스처, 건너뛸 텍스처, 변경될 설정을 보여줍니다. 대량 처리 전에는 항상 먼저 확인하는 것이 좋습니다." } },
        { "Guide Apply", new[] { "Apply Changes", "应用更改", "変更を適用", "변경 적용" } },
        { "Guide Apply Body", new[] { "After checking the preview, disable Dry Run and click Apply to Ready Textures. A progress bar appears and can be canceled. Already processed textures stay changed.", "确认预览后，关闭仅预览并点击“应用到可处理贴图”。处理时会显示进度条，可以取消；已经处理过的贴图不会自动回滚。", "プレビュー確認後、ドライランを無効にして“処理対象テクスチャに適用”をクリックします。進行バーからキャンセルできますが、処理済みのテクスチャは自動では戻りません。", "미리보기를 확인한 뒤 드라이 런을 끄고 처리 가능 텍스처에 적용을 클릭합니다. 진행 표시줄에서 취소할 수 있지만 이미 처리된 텍스처는 자동으로 되돌아가지 않습니다." } },
        { "Guide Risks", new[] { "Risks", "使用风险", "リスク", "위험 안내" } },
        { "Guide Risks Body", new[] { "Batch compression changes texture importer settings and .meta files. Back up the project or use version control before applying. Very small Max Size values can blur textures. Wrong settings on normal maps, UI, or important text images can visibly damage quality.", "批量压缩会修改贴图导入设置和 .meta 文件。应用前请备份项目或使用版本管理。过低的最大尺寸会让贴图变糊；法线贴图、UI、文字图片如果设置错误，画质会明显受损。", "一括圧縮はテクスチャのインポート設定と .meta ファイルを変更します。適用前にバックアップまたはバージョン管理を使ってください。最大サイズが小さすぎるとぼやけます。法線マップ、UI、文字画像の設定ミスは品質低下につながります。", "일괄 압축은 텍스처 임포트 설정과 .meta 파일을 변경합니다. 적용 전에 프로젝트를 백업하거나 버전 관리를 사용하세요. 최대 크기가 너무 작으면 텍스처가 흐려질 수 있습니다. 노멀 맵, UI, 글자 이미지에 잘못 적용하면 품질이 크게 떨어질 수 있습니다." } }
    };

    // Shared language state keeps the main tool window and the guide window synchronized.
    private static ToolLanguage activeLanguage = ToolLanguage.English;

    // Folder list and scroll positions used by the editor UI.
    private readonly List<string> folderPaths = new List<string> { "Assets" };
    private string pendingFolderPath = "Assets";
    private Vector2 mainScrollPos;
    private Vector2 folderScrollPos;
    private Vector2 textureScrollPos;
    private readonly List<TextureItem> textures = new List<TextureItem>();

    // Default importer settings applied to every ready texture.
    private int newMaxSize = 512;
    private TextureImporterCompression compression = TextureImporterCompression.Compressed;
    private bool useCrunch = false;
    private int crunchQuality = 50;

    // Optional platform overrides for VRChat PC and Quest/mobile style builds.
    private bool overrideStandalone = false;
    private int standaloneMaxSize = 1024;
    private TextureImporterFormat standaloneFormat = TextureImporterFormat.Automatic;

    private bool overrideAndroid = false;
    private int androidMaxSize = 512;
    private TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6;

    // Filtering state controls which scanned textures are allowed into the apply step.
    private bool skipNormalMaps = false;
    private bool skipSprites = false;
    private bool skipSmallerThanTarget = false;
    private bool restrictToExtensions = false;
    private bool includePng = true;
    private bool includeJpg = true;
    private bool includeTga = true;
    private string excludePathKeywords = string.Empty;

    private bool dryRunMode = true;
    private bool showFilters = true;
    private bool showPlatformOverrides = true;
    private ToolLanguage language = ToolLanguage.English;

    // Cached metadata for one scanned texture. This keeps drawing, preview, and apply logic consistent.
    private class TextureItem
    {
        public Texture2D Texture;
        public string Path;
        public string Extension;
        public int Width;
        public int Height;
        public int MaxSize;
        public TextureImporterCompression Compression;
        public bool Crunch;
        public int CrunchQuality;
        public TextureImporterType TextureType;
        public bool WillProcess;
        public string SkipReason;
    }

    [MenuItem("Tools/Texture Batch Compressor")]
    public static void ShowWindow()
    {
        TextureBatchCompressor window = GetWindow<TextureBatchCompressor>(Translate(activeLanguage, "Texture Compressor"));
        window.minSize = new Vector2(760, 640);
    }

    // Load saved settings when Unity creates or reloads the editor window.
    private void OnEnable()
    {
        minSize = new Vector2(760, 640);
        LoadSettings(PrefPrefix);
        activeLanguage = language;
    }

    // Persist settings when the window closes or Unity recompiles editor scripts.
    private void OnDisable()
    {
        SaveSettings(PrefPrefix);
    }

    // Resolve a localized UI string for the current tool language.
    private string Text(string key)
    {
        return Translate(language, key);
    }

    // Resolve a localized string for any language, used by both the main window and guide window.
    private static string Translate(ToolLanguage targetLanguage, string key)
    {
        int languageIndex = Mathf.Clamp((int)targetLanguage, 0, LanguageLabels.Length - 1);
        string[] values;
        if (TextTable.TryGetValue(key, out values) && values.Length > languageIndex)
        {
            return values[languageIndex];
        }

        return key;
    }

    // Format a localized string with runtime values such as counts, paths, and texture settings.
    private string TextFormat(string key, params object[] args)
    {
        return string.Format(Text(key), args);
    }

    // Main editor UI entry point. Individual sections are split into Draw* methods below.
    private void OnGUI()
    {
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);
        EditorGUI.BeginChangeCheck();

        DrawLanguageSection();
        titleContent = new GUIContent(Text("Texture Compressor"));

        GUILayout.Label(Text("Batch Texture Compression Tool"), EditorStyles.boldLabel);
        DrawScanSummary();
        DrawFolderSection();

        EditorGUILayout.Space();
        DrawPresetSection();

        EditorGUILayout.Space();
        DrawFilterSection();

        EditorGUILayout.Space();
        DrawCompressionSection();

        EditorGUILayout.Space();
        DrawPlatformOverrideSection();

        EditorGUILayout.Space();
        DrawActionSection();

        EditorGUILayout.Space();
        DrawTextureList();

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings(PrefPrefix);
            RefreshTexturePlan();
        }
        EditorGUILayout.EndScrollView();
    }

    // Top bar with the guide button and language selector.
    private void DrawLanguageSection()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(Text("Guide"), GUILayout.Width(90)))
        {
            TextureBatchCompressorGuideWindow.Open(language);
        }
        GUILayout.Label(Text("Language"), GUILayout.Width(70));
        ToolLanguage previousLanguage = language;
        language = (ToolLanguage)EditorGUILayout.Popup((int)language, LanguageLabels, GUILayout.Width(130));
        activeLanguage = language;
        if (previousLanguage != language)
        {
            TextureBatchCompressorGuideWindow.RepaintOpenWindow();
        }
        EditorGUILayout.EndHorizontal();
    }

    // Compact status line showing scan count, ready-to-process count, and skipped count.
    private void DrawScanSummary()
    {
        int readyCount = GetReadyCount();
        int skippedCount = textures.Count - readyCount;
        EditorGUILayout.HelpBox(TextFormat("Scanned: {0} | Ready: {1} | Skipped: {2}", textures.Count, readyCount, skippedCount), MessageType.Info);
    }

    // Folder management UI. Users can add, browse, remove, and scan multiple Assets folders.
    private void DrawFolderSection()
    {
        GUILayout.Label(Text("Target Folders"), EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        int removeIndex = -1;
        folderScrollPos = EditorGUILayout.BeginScrollView(folderScrollPos, GUILayout.MinHeight(70), GUILayout.MaxHeight(130));
        for (int i = 0; i < folderPaths.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            folderPaths[i] = EditorGUILayout.TextField(TextFormat("Folder {0}", i + 1), folderPaths[i]);

            if (GUILayout.Button(Text("Browse"), GUILayout.Width(80)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel(Text("Select Folder"), Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    SetFolderFromAbsolutePath(i, selectedPath);
                }
            }

            GUI.enabled = folderPaths.Count > 1;
            if (GUILayout.Button(Text("Remove"), GUILayout.Width(80)))
            {
                removeIndex = i;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (removeIndex >= 0)
        {
            folderPaths.RemoveAt(removeIndex);
        }

        EditorGUILayout.BeginHorizontal();
        pendingFolderPath = EditorGUILayout.TextField(Text("Add Folder:"), pendingFolderPath);
        if (GUILayout.Button(Text("Add"), GUILayout.Width(80)))
        {
            AddFolderPath(pendingFolderPath, true);
        }
        if (GUILayout.Button(Text("Browse Add"), GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel(Text("Add Folder"), Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                AddFolderFromAbsolutePath(selectedPath);
            }
        }
        if (GUILayout.Button(Text("Scan All"), GUILayout.Width(90)))
        {
            ScanTextures();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    // Preset UI grouped by asset type and target platform.
    private void DrawPresetSection()
    {
        GUILayout.Label(Text("Presets"), EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(Text("Map"), EditorStyles.miniBoldLabel, GUILayout.Width(70));
        if (GUILayout.Button(Text("PC"), GUILayout.Height(24)))
        {
            ApplyMapPcPreset();
        }
        if (GUILayout.Button(Text("Mobile"), GUILayout.Height(24)))
        {
            ApplyMapMobilePreset();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(Text("Model"), EditorStyles.miniBoldLabel, GUILayout.Width(70));
        if (GUILayout.Button(Text("PC"), GUILayout.Height(24)))
        {
            ApplyModelPcPreset();
        }
        if (GUILayout.Button(Text("Mobile"), GUILayout.Height(24)))
        {
            ApplyModelMobilePreset();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    // Filter controls that decide which scanned textures are skipped before applying changes.
    private void DrawFilterSection()
    {
        showFilters = EditorGUILayout.Foldout(showFilters, Text("Filters"), true);
        if (!showFilters)
        {
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        skipNormalMaps = EditorGUILayout.ToggleLeft(Text("Skip Normal Maps"), skipNormalMaps);
        skipSprites = EditorGUILayout.ToggleLeft(Text("Skip Sprites / UI"), skipSprites);
        skipSmallerThanTarget = EditorGUILayout.ToggleLeft(Text("Skip Textures Already Under Target Size"), skipSmallerThanTarget);
        restrictToExtensions = EditorGUILayout.ToggleLeft(Text("Only Process Selected Extensions"), restrictToExtensions);

        if (restrictToExtensions)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            includePng = EditorGUILayout.ToggleLeft("PNG", includePng, GUILayout.Width(80));
            includeJpg = EditorGUILayout.ToggleLeft("JPG / JPEG", includeJpg, GUILayout.Width(120));
            includeTga = EditorGUILayout.ToggleLeft("TGA", includeTga, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
        }

        excludePathKeywords = EditorGUILayout.TextField(Text("Exclude Path Keywords"), excludePathKeywords);
        EditorGUILayout.EndVertical();
    }

    // Default texture importer controls shared by every processed texture.
    private void DrawCompressionSection()
    {
        GUILayout.Label(Text("Default Import Settings"), EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        newMaxSize = EditorGUILayout.IntPopup(Text("Max Size"), newMaxSize, MaxSizeLabels, MaxSizeValues);
        compression = (TextureImporterCompression)EditorGUILayout.EnumPopup(Text("Compression Mode"), compression);
        useCrunch = EditorGUILayout.Toggle(Text("Use Crunch Compression"), useCrunch);
        if (useCrunch)
        {
            crunchQuality = EditorGUILayout.IntSlider(Text("Crunch Quality"), crunchQuality, 0, 100);
        }
        EditorGUILayout.EndVertical();
    }

    // Optional per-platform override controls for Standalone and Android.
    private void DrawPlatformOverrideSection()
    {
        showPlatformOverrides = EditorGUILayout.Foldout(showPlatformOverrides, Text("Platform Overrides"), true);
        if (!showPlatformOverrides)
        {
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawPlatformFields("Standalone", ref overrideStandalone, ref standaloneMaxSize, ref standaloneFormat);
        EditorGUILayout.Space();
        DrawPlatformFields("Android", ref overrideAndroid, ref androidMaxSize, ref androidFormat);
        EditorGUILayout.EndVertical();
    }

    // Draws one platform override block and writes the values back through ref parameters.
    private void DrawPlatformFields(string platformName, ref bool overrideEnabled, ref int maxSize, ref TextureImporterFormat format)
    {
        overrideEnabled = EditorGUILayout.Toggle(TextFormat("Override {0}", platformName), overrideEnabled);
        if (!overrideEnabled)
        {
            return;
        }

        EditorGUI.indentLevel++;
        maxSize = EditorGUILayout.IntPopup(TextFormat("{0} Max Size", platformName), maxSize, MaxSizeLabels, MaxSizeValues);
        format = (TextureImporterFormat)EditorGUILayout.EnumPopup(TextFormat("{0} Format", platformName), format);
        EditorGUILayout.LabelField(Text("Compression and Crunch settings follow the default import settings."), EditorStyles.miniLabel);
        EditorGUI.indentLevel--;
    }

    // Preview/apply action controls. Dry Run prevents asset changes and only logs the plan.
    private void DrawActionSection()
    {
        GUILayout.Label(Text("Actions"), EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        dryRunMode = EditorGUILayout.Toggle(Text("Dry Run / Preview Only"), dryRunMode);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(Text("Refresh Preview"), GUILayout.Height(28)))
        {
            PreviewChanges();
        }

        GUI.enabled = textures.Count > 0;
        string actionLabel = dryRunMode ? Text("Run Dry Preview") : Text("Apply to Ready Textures");
        if (GUILayout.Button(actionLabel, GUILayout.Height(28)))
        {
            if (dryRunMode)
            {
                PreviewChanges();
            }
            else
            {
                ApplyCompression();
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // Draws the current scan result list with importer metadata and skip status.
    private void DrawTextureList()
    {
        GUILayout.Label(Text("Texture Preview"), EditorStyles.boldLabel);

        if (textures.Count == 0)
        {
            EditorGUILayout.HelpBox(Text("Scan a folder to list textures."), MessageType.None);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Text("Texture"), EditorStyles.miniBoldLabel, GUILayout.Width(190));
        EditorGUILayout.LabelField(Text("Size"), EditorStyles.miniBoldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField(Text("Max"), EditorStyles.miniBoldLabel, GUILayout.Width(60));
        EditorGUILayout.LabelField(Text("Compression"), EditorStyles.miniBoldLabel, GUILayout.Width(105));
        EditorGUILayout.LabelField(Text("Crunch"), EditorStyles.miniBoldLabel, GUILayout.Width(95));
        EditorGUILayout.LabelField(Text("Type"), EditorStyles.miniBoldLabel, GUILayout.Width(95));
        EditorGUILayout.LabelField(Text("Status"), EditorStyles.miniBoldLabel);
        EditorGUILayout.EndHorizontal();

        textureScrollPos = EditorGUILayout.BeginScrollView(textureScrollPos, GUILayout.Height(260));
        foreach (TextureItem item in textures)
        {
            DrawTextureRow(item);
        }
        EditorGUILayout.EndScrollView();
    }

    // Draw one scanned texture row in the preview list.
    private void DrawTextureRow(TextureItem item)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(item.Texture, typeof(Texture2D), false, GUILayout.Width(190));
        EditorGUILayout.LabelField($"{item.Width}x{item.Height}", GUILayout.Width(80));
        EditorGUILayout.LabelField(item.MaxSize.ToString(), GUILayout.Width(60));
        EditorGUILayout.LabelField(item.Compression.ToString(), GUILayout.Width(105));
        EditorGUILayout.LabelField(item.Crunch ? TextFormat("Yes ({0})", item.CrunchQuality) : Text("No"), GUILayout.Width(95));
        EditorGUILayout.LabelField(item.TextureType.ToString(), GUILayout.Width(95));
        EditorGUILayout.LabelField(item.WillProcess ? Text("Ready") : TextFormat("Skipped: {0}", item.SkipReason));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(item.Path, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    // Scan all validated target folders for Texture2D assets and cache unique results.
    private void ScanTextures()
    {
        string error;
        string[] validFolders;
        if (!TryNormalizeFolderPaths(out validFolders, out error))
        {
            EditorUtility.DisplayDialog(Text("Invalid Folder"), error, "OK");
            SetReport(TextFormat("Scan failed: {0}", error));
            return;
        }

        textures.Clear();
        HashSet<string> scannedGuids = new HashSet<string>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", validFolders);
        foreach (string guid in guids)
        {
            if (!scannedGuids.Add(guid))
            {
                continue;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureItem item = CreateTextureItem(path);
            if (item != null)
            {
                textures.Add(item);
            }
        }

        textures.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase));
        RefreshTexturePlan();
        SetReport(TextFormat("Scan complete. Found {0} unique texture(s) in {1}.", textures.Count, string.Join(", ", validFolders)));
    }

    // Create a cached texture row from an AssetDatabase path.
    private TextureItem CreateTextureItem(string path)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            return null;
        }

        TextureItem item = new TextureItem
        {
            Texture = texture,
            Path = path,
            Extension = Path.GetExtension(path).ToLowerInvariant()
        };

        UpdateTextureItem(item);
        return item;
    }

    // Re-read importer metadata and recalculate whether each texture should be processed.
    private void RefreshTexturePlan()
    {
        for (int i = textures.Count - 1; i >= 0; i--)
        {
            TextureItem item = textures[i];
            if (item == null || item.Texture == null)
            {
                textures.RemoveAt(i);
                continue;
            }

            UpdateTextureItem(item);
            item.WillProcess = ShouldProcess(item, out item.SkipReason);
        }
    }

    // Pull the current Unity importer state into a TextureItem.
    private void UpdateTextureItem(TextureItem item)
    {
        TextureImporter importer = GetImporter(item.Path);
        item.Width = item.Texture != null ? item.Texture.width : 0;
        item.Height = item.Texture != null ? item.Texture.height : 0;
        item.MaxSize = importer != null ? importer.maxTextureSize : 0;
        item.Compression = importer != null ? importer.textureCompression : TextureImporterCompression.Uncompressed;
        item.Crunch = importer != null && importer.crunchedCompression;
        item.CrunchQuality = importer != null ? importer.compressionQuality : 0;
        item.TextureType = importer != null ? importer.textureType : TextureImporterType.Default;
        item.Extension = Path.GetExtension(item.Path).ToLowerInvariant();
    }

    // Apply all skip rules to one texture and return a localized skip reason when it is excluded.
    private bool ShouldProcess(TextureItem item, out string reason)
    {
        reason = string.Empty;

        if (GetImporter(item.Path) == null)
        {
            reason = Text("missing importer");
            return false;
        }

        if (skipNormalMaps && item.TextureType == TextureImporterType.NormalMap)
        {
            reason = Text("normal map");
            return false;
        }

        if (skipSprites && item.TextureType == TextureImporterType.Sprite)
        {
            reason = Text("sprite / UI");
            return false;
        }

        if (restrictToExtensions && !IsAllowedExtension(item.Extension))
        {
            reason = Text("extension");
            return false;
        }

        if (MatchesExcludedKeyword(item.Path, out string keyword))
        {
            reason = TextFormat("excluded keyword: {0}", keyword);
            return false;
        }

        if (skipSmallerThanTarget && item.Width <= newMaxSize && item.Height <= newMaxSize)
        {
            reason = Text("already smaller than target");
            return false;
        }

        return true;
    }

    // Extension filter helper for PNG, JPG/JPEG, and TGA.
    private bool IsAllowedExtension(string extension)
    {
        if (!includePng && !includeJpg && !includeTga)
        {
            return false;
        }

        if (includePng && extension == ".png")
        {
            return true;
        }

        if (includeJpg && (extension == ".jpg" || extension == ".jpeg"))
        {
            return true;
        }

        if (includeTga && extension == ".tga")
        {
            return true;
        }

        return false;
    }

    // Checks comma, semicolon, or newline separated path keywords against an asset path.
    private bool MatchesExcludedKeyword(string path, out string matchedKeyword)
    {
        matchedKeyword = string.Empty;
        string[] keywords = excludePathKeywords.Split(KeywordSeparators, StringSplitOptions.RemoveEmptyEntries);
        foreach (string keyword in keywords)
        {
            string trimmed = keyword.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (path.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matchedKeyword = trimmed;
                return true;
            }
        }

        return false;
    }

    // Build and log a Dry Run report without modifying any import settings.
    private void PreviewChanges()
    {
        if (textures.Count == 0)
        {
            ScanTextures();
            return;
        }

        RefreshTexturePlan();
        List<string> lines = BuildPreviewLines();
        SetReport(lines);
    }

    // Creates the detailed preview report shown in the Unity Console.
    private List<string> BuildPreviewLines()
    {
        int readyCount = GetReadyCount();
        int skippedCount = textures.Count - readyCount;

        List<string> lines = new List<string>
        {
            TextFormat("Preview complete. Ready: {0}, skipped: {1}.", readyCount, skippedCount),
            TextFormat("Target default settings: max {0}, {1}, crunch {2}.", newMaxSize, compression, Text(useCrunch ? "on" : "off"))
        };

        if (overrideStandalone)
        {
            lines.Add(TextFormat("Standalone override: max {0}, {1}.", standaloneMaxSize, standaloneFormat));
        }

        if (overrideAndroid)
        {
            lines.Add(TextFormat("Android override: max {0}, {1}.", androidMaxSize, androidFormat));
        }

        foreach (TextureItem item in textures)
        {
            if (item.WillProcess)
            {
                lines.Add(TextFormat("Will process: {0} | {1}", item.Path, BuildChangeSummary(item)));
            }
            else
            {
                lines.Add(TextFormat("Skipped: {0} | {1}", item.Path, item.SkipReason));
            }
        }

        return lines;
    }

    // Summarizes the importer changes that will be applied to a texture.
    private string BuildChangeSummary(TextureItem item)
    {
        string oldCrunchText = Text(item.Crunch ? "on" : "off");
        string newCrunchText = useCrunch ? Text("on") + $" ({crunchQuality})" : Text("off");
        string summary = TextFormat("current max {0}, {1}, crunch {2} -> max {3}, {4}, crunch {5}", item.MaxSize, item.Compression, oldCrunchText, newMaxSize, compression, newCrunchText);
        TextureImporter importer = GetImporter(item.Path);

        if (importer != null && overrideStandalone)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Standalone");
            summary += " | " + TextFormat("Standalone {0} -> max {1}, {2}", FormatPlatformSettings(settings), standaloneMaxSize, standaloneFormat);
        }

        if (importer != null && overrideAndroid)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Android");
            summary += " | " + TextFormat("Android {0} -> max {1}, {2}", FormatPlatformSettings(settings), androidMaxSize, androidFormat);
        }

        return summary;
    }

    // Summarizes the current platform override state for preview and apply logs.
    private string FormatPlatformSettings(TextureImporterPlatformSettings settings)
    {
        if (!settings.overridden)
        {
            return Text("not overridden");
        }

        return $"max {settings.maxTextureSize}, {settings.format}";
    }

    // Apply the planned importer settings to every ready texture, with progress and cancel support.
    private void ApplyCompression()
    {
        RefreshTexturePlan();
        List<TextureItem> readyItems = GetReadyItems();
        int skippedCount = textures.Count - readyItems.Count;

        if (readyItems.Count == 0)
        {
            string message = TextFormat("No textures are ready to process. Skipped: {0}.", skippedCount);
            SetReport(message);
            EditorUtility.DisplayDialog(Text("Nothing to Process"), message, "OK");
            return;
        }

        int processedCount = 0;
        int failedCount = 0;
        bool canceled = false;
        List<string> lines = new List<string>();

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < readyItems.Count; i++)
            {
                TextureItem item = readyItems[i];
                float progress = readyItems.Count == 0 ? 1f : (float)i / readyItems.Count;
                bool cancelRequested = EditorUtility.DisplayCancelableProgressBar(
                    Text("Applying Texture Compression"),
                    $"{Path.GetFileName(item.Path)} ({i + 1}/{readyItems.Count})",
                    progress);

                if (cancelRequested)
                {
                    canceled = true;
                    lines.Add(Text("Canceled by user."));
                    break;
                }

                try
                {
                    TextureImporter importer = GetImporter(item.Path);
                    if (importer == null)
                    {
                        failedCount++;
                        lines.Add(TextFormat("Failed: {0} | {1}", item.Path, Text("missing importer")));
                        continue;
                    }

                    string changeSummary = BuildChangeSummary(item);
                    Undo.RegisterCompleteObjectUndo(importer, Text("Applying Texture Compression"));
                    ApplyImporterSettings(importer);
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    processedCount++;
                    lines.Add(TextFormat("Processed: {0} | {1}", item.Path, changeSummary));
                }
                catch (Exception ex)
                {
                    failedCount++;
                    lines.Add(TextFormat("Failed: {0} | {1}", item.Path, ex.Message));
                    Debug.LogError("Texture Batch Compressor failed for " + item.Path + "\n" + ex);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        RefreshTexturePlan();

        string status = canceled ? Text("Canceled") : Text("Complete");
        string summary = TextFormat("{0}. Processed: {1}, skipped: {2}, failed: {3}.", status, processedCount, skippedCount, failedCount);
        lines.Insert(0, summary);
        SetReport(lines);
        EditorUtility.DisplayDialog(status, summary, "OK");
    }

    // Write the default importer settings and optional platform overrides to one TextureImporter.
    private void ApplyImporterSettings(TextureImporter importer)
    {
        importer.maxTextureSize = newMaxSize;
        importer.textureCompression = compression;
        importer.crunchedCompression = useCrunch;
        importer.compressionQuality = crunchQuality;

        ApplyPlatformSettings(importer, "Standalone", overrideStandalone, standaloneMaxSize, standaloneFormat);
        ApplyPlatformSettings(importer, "Android", overrideAndroid, androidMaxSize, androidFormat);
    }

    // Apply or clear one named Unity platform override.
    private void ApplyPlatformSettings(TextureImporter importer, string platformName, bool overrideEnabled, int maxSize, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platformName);
        settings.name = platformName;
        settings.overridden = overrideEnabled;

        if (overrideEnabled)
        {
            settings.maxTextureSize = maxSize;
            settings.format = format;
            settings.textureCompression = compression;
            settings.crunchedCompression = useCrunch;
            settings.compressionQuality = crunchQuality;
        }

        importer.SetPlatformTextureSettings(settings);
    }

    // Return only textures that passed every filter.
    private List<TextureItem> GetReadyItems()
    {
        List<TextureItem> readyItems = new List<TextureItem>();
        foreach (TextureItem item in textures)
        {
            if (item.WillProcess)
            {
                readyItems.Add(item);
            }
        }

        return readyItems;
    }

    // Count textures that are currently ready for processing.
    private int GetReadyCount()
    {
        int count = 0;
        foreach (TextureItem item in textures)
        {
            if (item.WillProcess)
            {
                count++;
            }
        }

        return count;
    }

    // Safe importer lookup wrapper used throughout scan, preview, and apply logic.
    private TextureImporter GetImporter(string path)
    {
        return AssetImporter.GetAtPath(path) as TextureImporter;
    }

    // Normalize, validate, and deduplicate every target folder before scanning.
    private bool TryNormalizeFolderPaths(out string[] validFolders, out string error)
    {
        EnsureFolderList();

        List<string> normalizedFolders = new List<string>();
        HashSet<string> uniqueFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < folderPaths.Count; i++)
        {
            string normalizedPath = NormalizeAssetPath(folderPaths[i].Trim());
            if (!TryValidateFolderPath(normalizedPath, out error))
            {
                validFolders = new string[0];
                error = TextFormat("Folder {0}: {1}", i + 1, error);
                return false;
            }

            if (uniqueFolders.Add(normalizedPath))
            {
                normalizedFolders.Add(normalizedPath);
            }
        }

        folderPaths.Clear();
        folderPaths.AddRange(normalizedFolders);

        validFolders = normalizedFolders.ToArray();
        error = string.Empty;
        return true;
    }

    // Validate one Assets-relative folder path and return a localized error message.
    private bool TryValidateFolderPath(string path, out string error)
    {
        if (string.IsNullOrEmpty(path))
        {
            error = Text("Folder path is empty.");
            return false;
        }

        if (!IsAssetFolderPath(path))
        {
            error = Text("Folder path must be inside the Assets directory.");
            return false;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            error = TextFormat("Folder does not exist: {0}", path);
            return false;
        }

        error = string.Empty;
        return true;
    }

    // Unity texture search must stay inside the Assets folder.
    private bool IsAssetFolderPath(string path)
    {
        return path.Equals("Assets", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
    }

    // Replace an existing folder list entry with a folder selected from the OS picker.
    private void SetFolderFromAbsolutePath(int index, string selectedPath)
    {
        string assetPath;
        string error;
        if (TryGetAssetFolderFromAbsolutePath(selectedPath, out assetPath, out error))
        {
            folderPaths[index] = assetPath;
            return;
        }

        EditorUtility.DisplayDialog(Text("Invalid Folder"), error, "OK");
    }

    // Add a folder selected from the OS picker to the target list.
    private void AddFolderFromAbsolutePath(string selectedPath)
    {
        string assetPath;
        string error;
        if (TryGetAssetFolderFromAbsolutePath(selectedPath, out assetPath, out error))
        {
            AddFolderPath(assetPath, true);
            return;
        }

        EditorUtility.DisplayDialog(Text("Invalid Folder"), error, "OK");
    }

    // Convert an absolute OS path into a Unity Assets-relative path.
    private bool TryGetAssetFolderFromAbsolutePath(string selectedPath, out string assetPath, out string error)
    {
        string normalizedSelectedPath = NormalizeAssetPath(selectedPath);
        string normalizedDataPath = NormalizeAssetPath(Application.dataPath);

        if (normalizedSelectedPath.Equals(normalizedDataPath, StringComparison.OrdinalIgnoreCase))
        {
            assetPath = "Assets";
            return TryValidateFolderPath(assetPath, out error);
        }

        if (normalizedSelectedPath.StartsWith(normalizedDataPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            assetPath = "Assets" + normalizedSelectedPath.Substring(normalizedDataPath.Length);
            return TryValidateFolderPath(assetPath, out error);
        }

        assetPath = string.Empty;
        error = Text("You must select a folder inside the Assets directory.");
        return false;
    }

    // Add a typed Assets-relative path, with validation and duplicate checks.
    private void AddFolderPath(string path, bool showDialog)
    {
        string normalizedPath = NormalizeAssetPath(path.Trim());
        string error;
        if (!TryValidateFolderPath(normalizedPath, out error))
        {
            if (showDialog)
            {
                EditorUtility.DisplayDialog(Text("Invalid Folder"), error, "OK");
            }

            return;
        }

        foreach (string existingPath in folderPaths)
        {
            if (normalizedPath.Equals(NormalizeAssetPath(existingPath.Trim()), StringComparison.OrdinalIgnoreCase))
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(Text("Folder Already Added"), TextFormat("{0} is already in the target list.", normalizedPath), "OK");
                }

                return;
            }
        }

        folderPaths.Add(normalizedPath);
        pendingFolderPath = normalizedPath;
        SetReport(TextFormat("Added folder: {0}", normalizedPath));
    }

    // Keep at least one target folder so the UI always has a valid row.
    private void EnsureFolderList()
    {
        if (folderPaths.Count == 0)
        {
            folderPaths.Add("Assets");
        }
    }

    // Use forward slashes because Unity asset paths are slash-normalized.
    private string NormalizeAssetPath(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
    }

    // Preset for world/map textures on PC builds.
    private void ApplyMapPcPreset()
    {
        newMaxSize = 1024;
        compression = TextureImporterCompression.CompressedHQ;
        useCrunch = false;
        crunchQuality = 80;
        overrideStandalone = true;
        standaloneMaxSize = 1024;
        standaloneFormat = TextureImporterFormat.BC7;
        overrideAndroid = false;
        restrictToExtensions = true;
        includePng = true;
        includeJpg = true;
        includeTga = true;
        skipSmallerThanTarget = false;
        SetReport(Text("Loaded preset: Map PC."));
    }

    // Preset for world/map textures on mobile or Quest builds.
    private void ApplyMapMobilePreset()
    {
        newMaxSize = 512;
        compression = TextureImporterCompression.Compressed;
        useCrunch = false;
        crunchQuality = 60;
        overrideStandalone = false;
        overrideAndroid = true;
        androidMaxSize = 512;
        androidFormat = TextureImporterFormat.ASTC_6x6;
        restrictToExtensions = true;
        includePng = true;
        includeJpg = true;
        includeTga = true;
        skipSmallerThanTarget = true;
        SetReport(Text("Loaded preset: Map Mobile."));
    }

    // Preset for model textures on PC builds, preserving more detail.
    private void ApplyModelPcPreset()
    {
        newMaxSize = 2048;
        compression = TextureImporterCompression.CompressedHQ;
        useCrunch = false;
        crunchQuality = 90;
        overrideStandalone = true;
        standaloneMaxSize = 2048;
        standaloneFormat = TextureImporterFormat.BC7;
        overrideAndroid = false;
        restrictToExtensions = true;
        includePng = true;
        includeJpg = true;
        includeTga = true;
        skipSmallerThanTarget = false;
        SetReport(Text("Loaded preset: Model PC."));
    }

    // Preset for model textures on mobile or Quest builds.
    private void ApplyModelMobilePreset()
    {
        newMaxSize = 1024;
        compression = TextureImporterCompression.Compressed;
        useCrunch = false;
        crunchQuality = 70;
        overrideStandalone = false;
        overrideAndroid = true;
        androidMaxSize = 1024;
        androidFormat = TextureImporterFormat.ASTC_6x6;
        restrictToExtensions = true;
        includePng = true;
        includeJpg = true;
        includeTga = true;
        skipSmallerThanTarget = true;
        SetReport(Text("Loaded preset: Model Mobile."));
    }

    // Persist the current UI state in EditorPrefs so it survives editor restarts.
    private void SaveSettings(string prefix)
    {
        EnsureFolderList();
        EditorPrefs.SetString(prefix + "FolderPaths", string.Join("\n", folderPaths.ToArray()));
        EditorPrefs.SetString(prefix + "FolderPath", folderPaths[0]);
        EditorPrefs.SetInt(prefix + "MaxSize", newMaxSize);
        EditorPrefs.SetInt(prefix + "Compression", (int)compression);
        EditorPrefs.SetBool(prefix + "UseCrunch", useCrunch);
        EditorPrefs.SetInt(prefix + "CrunchQuality", crunchQuality);

        EditorPrefs.SetBool(prefix + "OverrideStandalone", overrideStandalone);
        EditorPrefs.SetInt(prefix + "StandaloneMaxSize", standaloneMaxSize);
        EditorPrefs.SetInt(prefix + "StandaloneFormat", (int)standaloneFormat);

        EditorPrefs.SetBool(prefix + "OverrideAndroid", overrideAndroid);
        EditorPrefs.SetInt(prefix + "AndroidMaxSize", androidMaxSize);
        EditorPrefs.SetInt(prefix + "AndroidFormat", (int)androidFormat);

        EditorPrefs.SetBool(prefix + "SkipNormalMaps", skipNormalMaps);
        EditorPrefs.SetBool(prefix + "SkipSprites", skipSprites);
        EditorPrefs.SetBool(prefix + "SkipSmallerThanTarget", skipSmallerThanTarget);
        EditorPrefs.SetBool(prefix + "RestrictToExtensions", restrictToExtensions);
        EditorPrefs.SetBool(prefix + "IncludePng", includePng);
        EditorPrefs.SetBool(prefix + "IncludeJpg", includeJpg);
        EditorPrefs.SetBool(prefix + "IncludeTga", includeTga);
        EditorPrefs.SetString(prefix + "ExcludePathKeywords", excludePathKeywords);
        EditorPrefs.SetBool(prefix + "DryRunMode", dryRunMode);
        EditorPrefs.SetInt(prefix + "Language", (int)language);
    }

    // Restore saved UI state, falling back to the field defaults when no setting exists.
    private void LoadSettings(string prefix)
    {
        LoadFolderSettings(prefix);
        newMaxSize = EditorPrefs.GetInt(prefix + "MaxSize", newMaxSize);
        compression = (TextureImporterCompression)EditorPrefs.GetInt(prefix + "Compression", (int)compression);
        useCrunch = EditorPrefs.GetBool(prefix + "UseCrunch", useCrunch);
        crunchQuality = EditorPrefs.GetInt(prefix + "CrunchQuality", crunchQuality);

        overrideStandalone = EditorPrefs.GetBool(prefix + "OverrideStandalone", overrideStandalone);
        standaloneMaxSize = EditorPrefs.GetInt(prefix + "StandaloneMaxSize", standaloneMaxSize);
        standaloneFormat = (TextureImporterFormat)EditorPrefs.GetInt(prefix + "StandaloneFormat", (int)standaloneFormat);

        overrideAndroid = EditorPrefs.GetBool(prefix + "OverrideAndroid", overrideAndroid);
        androidMaxSize = EditorPrefs.GetInt(prefix + "AndroidMaxSize", androidMaxSize);
        androidFormat = (TextureImporterFormat)EditorPrefs.GetInt(prefix + "AndroidFormat", (int)androidFormat);

        skipNormalMaps = EditorPrefs.GetBool(prefix + "SkipNormalMaps", skipNormalMaps);
        skipSprites = EditorPrefs.GetBool(prefix + "SkipSprites", skipSprites);
        skipSmallerThanTarget = EditorPrefs.GetBool(prefix + "SkipSmallerThanTarget", skipSmallerThanTarget);
        restrictToExtensions = EditorPrefs.GetBool(prefix + "RestrictToExtensions", restrictToExtensions);
        includePng = EditorPrefs.GetBool(prefix + "IncludePng", includePng);
        includeJpg = EditorPrefs.GetBool(prefix + "IncludeJpg", includeJpg);
        includeTga = EditorPrefs.GetBool(prefix + "IncludeTga", includeTga);
        excludePathKeywords = EditorPrefs.GetString(prefix + "ExcludePathKeywords", excludePathKeywords);
        dryRunMode = EditorPrefs.GetBool(prefix + "DryRunMode", dryRunMode);
        language = (ToolLanguage)Mathf.Clamp(EditorPrefs.GetInt(prefix + "Language", (int)language), 0, LanguageLabels.Length - 1);
    }

    // Load the multi-folder setting while preserving compatibility with the old single-folder key.
    private void LoadFolderSettings(string prefix)
    {
        folderPaths.Clear();

        string savedFolderList = EditorPrefs.GetString(prefix + "FolderPaths", string.Empty);
        if (!string.IsNullOrEmpty(savedFolderList))
        {
            string[] savedFolders = savedFolderList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> uniqueFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string savedFolder in savedFolders)
            {
                string normalizedFolder = NormalizeAssetPath(savedFolder.Trim());
                if (!string.IsNullOrEmpty(normalizedFolder) && uniqueFolders.Add(normalizedFolder))
                {
                    folderPaths.Add(normalizedFolder);
                }
            }
        }
        else
        {
            folderPaths.Add(EditorPrefs.GetString(prefix + "FolderPath", "Assets"));
        }

        EnsureFolderList();
        pendingFolderPath = folderPaths[folderPaths.Count - 1];
    }

    // Reports are intentionally written to the Console so the main window stays compact.
    private void SetReport(string report)
    {
        Debug.Log("Texture Batch Compressor: " + report);
    }

    // Multi-line reports include full preview or apply details.
    private void SetReport(List<string> lines)
    {
        Debug.Log("Texture Batch Compressor\n" + string.Join("\n", lines.ToArray()));
    }

    // Separate help window that mirrors the current language selected in the main tool.
    private class TextureBatchCompressorGuideWindow : EditorWindow
    {
        private static TextureBatchCompressorGuideWindow openWindow;
        private Vector2 scrollPos;

        // Open or focus the guide window and sync it to the main tool language.
        public static void Open(ToolLanguage currentLanguage)
        {
            activeLanguage = currentLanguage;
            openWindow = GetWindow<TextureBatchCompressorGuideWindow>(Translate(activeLanguage, "Guide"));
            openWindow.minSize = new Vector2(520, 520);
            openWindow.Show();
            openWindow.Repaint();
        }

        // Repaint the guide when the main window language changes.
        public static void RepaintOpenWindow()
        {
            if (openWindow != null)
            {
                openWindow.Repaint();
            }
        }

        // Track the open guide instance so it can be repainted from the main window.
        private void OnEnable()
        {
            openWindow = this;
            minSize = new Vector2(520, 520);
        }

        // Clear the static instance when this guide window closes.
        private void OnDisable()
        {
            if (openWindow == this)
            {
                openWindow = null;
            }
        }

        // Draw the localized guide content.
        private void OnGUI()
        {
            ToolLanguage currentLanguage = activeLanguage;
            titleContent = new GUIContent(Translate(currentLanguage, "Guide"));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Label(Translate(currentLanguage, "Usage Guide"), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Translate(currentLanguage, "Guide Intro"), MessageType.Info);

            DrawGuideSection(currentLanguage, "Guide Basic Workflow", "Guide Basic Workflow Body");
            DrawGuideSection(currentLanguage, "Guide Presets", "Guide Presets Body");
            DrawGuideSection(currentLanguage, "Guide Filters", "Guide Filters Body");
            DrawGuideSection(currentLanguage, "Guide Dry Run", "Guide Dry Run Body");
            DrawGuideSection(currentLanguage, "Guide Apply", "Guide Apply Body");
            DrawGuideSection(currentLanguage, "Guide Risks", "Guide Risks Body");

            EditorGUILayout.EndScrollView();
        }

        // Draw a title/body section in the guide window.
        private void DrawGuideSection(ToolLanguage currentLanguage, string titleKey, string bodyKey)
        {
            EditorGUILayout.Space();
            GUILayout.Label(Translate(currentLanguage, titleKey), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Translate(currentLanguage, bodyKey), MessageType.None);
        }
    }
}
