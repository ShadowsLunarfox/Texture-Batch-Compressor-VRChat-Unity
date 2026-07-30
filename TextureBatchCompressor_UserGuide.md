# Texture Batch Compressor User Guide / 使用教程 / 使い方ガイド / 사용 가이드

## Languages

- [English](#english)
- [中文](#中文)
- [日本語](#日本語)
- [한국어](#한국어)

---

## English

### Tool Location

Open from the Unity menu:

```text
Tools > Texture Batch Compressor
```

Texture Batch Compressor is a Unity Editor tool for batch editing texture import compression settings. It is useful for VRChat world projects that need fast optimization for map textures, model textures, and PC / Mobile platform overrides.

The `Guide` button at the top of the tool opens the built-in guide window. The guide follows the currently selected UI language.

### Basic Workflow

1. Open the tool window.
2. Choose the UI language at the top: `English`, `中文`, `日本語`, or `한국어`.
3. Add one or more folders inside the Unity `Assets` directory.
4. Click `Scan All`.
5. Choose a preset:
   - `Map > PC`: map textures, PC quality first.
   - `Map > Mobile`: map textures, mobile size first.
   - `Model > PC`: model textures, keeps more close-up detail.
   - `Model > Mobile`: model textures, balances clarity and mobile size.
6. Adjust `Filters`, `Default Import Settings`, and `Platform Overrides` if needed.
7. Keep `Dry Run / Preview Only` enabled and click `Run Dry Preview`.
8. Check which textures will be processed or skipped.
9. Disable `Dry Run / Preview Only`.
10. Click `Apply to Ready Textures`.

### Multiple Folders

`Target Folders` supports multiple folders. Every folder must be inside the Unity project's `Assets` directory.

When scanning, the tool removes duplicates automatically. The same texture will only be processed once even if multiple target folders include it.

### Filters

`Skip Normal Maps`: skips normal maps to avoid damaging lighting and surface detail.

`Skip Sprites / UI`: skips Sprite or UI textures to prevent icons, buttons, and text textures from becoming blurry.

`Skip Textures Already Under Target Size`: skips textures whose width and height are already less than or equal to the target size.

`Only Process Selected Extensions`: only processes selected file types. Supported types are `PNG`, `JPG / JPEG`, and `TGA`.

`Exclude Path Keywords`: skips textures whose path contains a keyword. Separate multiple keywords with commas, semicolons, or new lines.

Example:

```text
NoCompress, UI, Normal
```

### Settings

`Max Size`: limits the maximum imported texture size. Smaller values reduce memory and download size, but also reduce detail.

`Compression Mode`: Unity's texture compression quality setting.

`Use Crunch Compression`: applies Crunch compression. This can reduce size further, but may increase import time and reduce quality.

`Platform Overrides`: sets platform-specific texture size and format for `Standalone` and `Android`. VRChat PC usually uses `Standalone`; Quest / Mobile usually uses `Android`.

### Risks

Batch compression changes Unity texture importer settings and `.meta` files. Back up the project or use version control before applying large batches.

Very low `Max Size` values can make materials blurry, especially large walls, floors, posters, text, UI, faces, and clothing details.

Wrong settings on normal maps can break lighting, bumps, and highlights. Use `Skip Normal Maps` unless you are intentionally processing them.

Mobile / Android formats are not always suitable for PC, and PC formats are not always suitable for Quest. Test each target platform before publishing.

Crunch compression can reduce file size, but important textures should be tested first with Dry Run and a small batch.

Unity Undo is registered, but texture reimport and platform override changes may not always be fully restored by Undo in every Unity version. Version control or project backups are the safest rollback method.

When processing many textures, Unity may pause for a while. The progress bar can cancel the operation, but textures already processed before cancellation will remain changed.

---

## 中文

### 工具位置

在 Unity 菜单栏打开：

```text
Tools > Texture Batch Compressor
```

Texture Batch Compressor 是一个 Unity 编辑器批量贴图压缩工具，适合 VRChat 世界项目中快速调整地图贴图、模型贴图，以及 PC / Mobile 平台覆盖设置。

工具窗口顶部的 `Guide / 教程` 按钮可以打开内置教程窗口，教程内容会跟随当前语言选项切换。

### 基本使用流程

1. 打开工具窗口。
2. 在窗口顶部选择界面语言：`English`、`中文`、`日本語`、`한국어`。
3. 在 `Target Folders` 中添加一个或多个 `Assets` 目录内的文件夹。
4. 点击 `Scan All` 扫描所有目标文件夹。
5. 选择一个预设：
   - `Map > PC`：地图贴图，电脑端质量优先。
   - `Map > Mobile`：地图贴图，手机端体积优先。
   - `Model > PC`：模型贴图，电脑端保留更多近看细节。
   - `Model > Mobile`：模型贴图，手机端兼顾清晰度和体积。
6. 按需要调整 `Filters`、`Default Import Settings` 和 `Platform Overrides`。
7. 保持 `Dry Run / Preview Only` 勾选，点击 `Run Dry Preview` 先预览。
8. 检查哪些贴图会被处理，哪些贴图会被跳过。
9. 取消勾选 `Dry Run / Preview Only`。
10. 点击 `Apply to Ready Textures` 开始批量应用。

### 多文件夹说明

`Target Folders` 支持添加多个文件夹。每个文件夹必须位于 Unity 项目的 `Assets` 目录内。

扫描时工具会自动去重。同一张贴图即使被多个目标路径覆盖，也只会处理一次。

### 过滤选项

`Skip Normal Maps`：跳过法线贴图，避免错误压缩导致光照和表面细节异常。

`Skip Sprites / UI`：跳过 Sprite 或 UI 贴图，避免图标、按钮、文字贴图变糊。

`Skip Textures Already Under Target Size`：跳过宽高都已经小于或等于目标尺寸的贴图。

`Only Process Selected Extensions`：只处理选中的文件类型，目前支持 `PNG`、`JPG / JPEG`、`TGA`。

`Exclude Path Keywords`：输入路径关键字来排除贴图。多个关键字可用英文逗号、分号或换行分隔。

示例：

```text
NoCompress, UI, Normal
```

### 设置说明

`Max Size`：限制贴图导入后的最大尺寸。数值越小，内存和下载体积越小，但画面细节也越少。

`Compression Mode`：Unity 的贴图压缩质量设置。

`Use Crunch Compression`：进一步压缩贴图体积，可能增加导入时间，也可能降低画质。

`Platform Overrides`：为 `Standalone` 和 `Android` 设置平台专用尺寸和格式。VRChat PC 通常对应 `Standalone`，Quest / Mobile 通常对应 `Android`。

### 风险说明

批量压缩会修改 Unity 贴图导入设置和 `.meta` 文件。建议在操作前备份项目，或确保项目已经使用 Git / Plastic SCM / Unity Version Control 等版本管理。

过低的 `Max Size` 会让材质变糊，尤其是大型墙面、地面、海报、文字、UI、角色脸部和衣服细节。

错误压缩法线贴图可能导致光照、凹凸和高光表现异常。除非你明确要处理法线贴图，否则建议启用 `Skip Normal Maps`。

Mobile / Android 格式不一定适合 PC 平台，PC 格式也不一定适合 Quest。发布前应分别测试目标平台。

Crunch 压缩可以减小体积，但重要贴图建议先用 Dry Run 预览，再小范围测试。

工具注册了 Unity Undo，但贴图重新导入和平台覆盖设置在不同 Unity 版本中可能无法完全依赖 Undo 恢复。最可靠的回滚方式仍然是版本管理或项目备份。

处理大量贴图时 Unity 可能短时间卡顿。进度条支持取消，但已经处理过的贴图不会自动回滚。

---

## 日本語

### ツールの場所

Unity メニューから開きます：

```text
Tools > Texture Batch Compressor
```

Texture Batch Compressor は、Unity Editor でテクスチャのインポート圧縮設定を一括変更するためのツールです。VRChat ワールド制作で、マップ用テクスチャ、モデル用テクスチャ、PC / Mobile 向けのプラットフォーム上書きを素早く調整したい場合に便利です。

ツール上部の `Guide / ガイド` ボタンを押すと、内蔵ガイドウィンドウを開けます。ガイド内容は現在選択中の UI 言語に合わせて切り替わります。

### 基本手順

1. ツールウィンドウを開きます。
2. 上部で UI 言語を選択します：`English`、`中文`、`日本語`、`한국어`。
3. `Target Folders` に、Unity の `Assets` 内のフォルダーを 1 つ以上追加します。
4. `Scan All` をクリックして対象フォルダーをスキャンします。
5. プリセットを選択します：
   - `Map > PC`：マップ用テクスチャ。PC 向けの品質を優先します。
   - `Map > Mobile`：マップ用テクスチャ。モバイル向けの容量削減を優先します。
   - `Model > PC`：モデル用テクスチャ。近くで見たときの細部を多めに残します。
   - `Model > Mobile`：モデル用テクスチャ。見やすさと容量を両立します。
6. 必要に応じて `Filters`、`Default Import Settings`、`Platform Overrides` を調整します。
7. `Dry Run / Preview Only` を有効にしたまま、`Run Dry Preview` で先に確認します。
8. 処理されるテクスチャとスキップされるテクスチャを確認します。
9. `Dry Run / Preview Only` を無効にします。
10. `Apply to Ready Textures` をクリックして一括適用します。

### 複数フォルダー

`Target Folders` には複数のフォルダーを追加できます。各フォルダーは Unity プロジェクトの `Assets` 内にある必要があります。

スキャン時には重複が自動で除外されます。同じテクスチャが複数の対象フォルダーに含まれていても、処理は 1 回だけです。

### フィルター

`Skip Normal Maps`：法線マップをスキップし、ライティングや表面の細部が壊れるのを避けます。

`Skip Sprites / UI`：Sprite または UI テクスチャをスキップし、アイコン、ボタン、文字テクスチャのぼやけを防ぎます。

`Skip Textures Already Under Target Size`：幅と高さがすでに目標サイズ以下のテクスチャをスキップします。

`Only Process Selected Extensions`：選択したファイル形式のみ処理します。対応形式は `PNG`、`JPG / JPEG`、`TGA` です。

`Exclude Path Keywords`：パスに含まれるキーワードでテクスチャを除外します。複数のキーワードはカンマ、セミコロン、改行で区切れます。

例：

```text
NoCompress, UI, Normal
```

### 設定

`Max Size`：インポート後の最大テクスチャサイズを制限します。小さいほどメモリ使用量やダウンロードサイズは下がりますが、細部も減ります。

`Compression Mode`：Unity のテクスチャ圧縮品質設定です。

`Use Crunch Compression`：Crunch 圧縮を使います。サイズをさらに減らせますが、インポート時間が増えたり画質が下がったりする場合があります。

`Platform Overrides`：`Standalone` と `Android` にプラットフォーム専用のサイズと形式を設定します。VRChat PC は通常 `Standalone`、Quest / Mobile は通常 `Android` を使います。

### リスク

一括圧縮は Unity のテクスチャインポート設定と `.meta` ファイルを変更します。大量に適用する前に、バックアップまたはバージョン管理を使うことをおすすめします。

`Max Size` が小さすぎると、特に大きな壁、床、ポスター、文字、UI、顔、服の細部がぼやけます。

法線マップに誤った設定を適用すると、ライティング、凹凸、ハイライトが不自然になる場合があります。意図して処理する場合以外は `Skip Normal Maps` を有効にしてください。

Mobile / Android 向け形式が PC に適しているとは限らず、PC 向け形式が Quest に適しているとも限りません。公開前に対象プラットフォームごとに確認してください。

Crunch 圧縮はサイズ削減に役立ちますが、重要なテクスチャは Dry Run と小規模テストで確認してください。

ツールは Unity Undo を登録しますが、テクスチャの再インポートやプラットフォーム上書きは Unity バージョンによって完全には戻らない場合があります。最も安全な回復方法はバージョン管理またはプロジェクトのバックアップです。

大量のテクスチャを処理すると Unity が一時的に止まることがあります。進行バーからキャンセルできますが、キャンセル前に処理済みのテクスチャは自動では戻りません。

---

## 한국어

### 도구 위치

Unity 메뉴에서 엽니다:

```text
Tools > Texture Batch Compressor
```

Texture Batch Compressor는 Unity Editor에서 텍스처 임포트 압축 설정을 일괄 변경하는 도구입니다. VRChat 월드 프로젝트에서 맵 텍스처, 모델 텍스처, PC / Mobile 플랫폼 오버라이드를 빠르게 조정할 때 유용합니다.

도구 상단의 `Guide / 가이드` 버튼을 누르면 내장 가이드 창을 열 수 있습니다. 가이드 내용은 현재 선택한 UI 언어에 맞춰 바뀝니다.

### 기본 흐름

1. 도구 창을 엽니다.
2. 상단에서 UI 언어를 선택합니다: `English`, `中文`, `日本語`, `한국어`.
3. `Target Folders`에 Unity `Assets` 안의 폴더를 하나 이상 추가합니다.
4. `Scan All`을 클릭해 모든 대상 폴더를 스캔합니다.
5. 프리셋을 선택합니다:
   - `Map > PC`: 맵 텍스처, PC 품질 우선.
   - `Map > Mobile`: 맵 텍스처, 모바일 용량 우선.
   - `Model > PC`: 모델 텍스처, 가까이 보이는 디테일을 더 유지.
   - `Model > Mobile`: 모델 텍스처, 선명도와 모바일 용량의 균형.
6. 필요에 따라 `Filters`, `Default Import Settings`, `Platform Overrides`를 조정합니다.
7. `Dry Run / Preview Only`를 켠 상태로 `Run Dry Preview`를 먼저 실행합니다.
8. 어떤 텍스처가 처리되고 어떤 텍스처가 건너뛰어지는지 확인합니다.
9. `Dry Run / Preview Only`를 끕니다.
10. `Apply to Ready Textures`를 클릭해 일괄 적용합니다.

### 여러 폴더

`Target Folders`는 여러 폴더를 지원합니다. 모든 폴더는 Unity 프로젝트의 `Assets` 디렉터리 안에 있어야 합니다.

스캔할 때 도구가 중복을 자동으로 제거합니다. 같은 텍스처가 여러 대상 폴더에 포함되어 있어도 한 번만 처리됩니다.

### 필터

`Skip Normal Maps`: 노멀 맵을 건너뛰어 조명과 표면 디테일이 손상되는 것을 피합니다.

`Skip Sprites / UI`: Sprite 또는 UI 텍스처를 건너뛰어 아이콘, 버튼, 글자 텍스처가 흐려지는 것을 방지합니다.

`Skip Textures Already Under Target Size`: 너비와 높이가 이미 목표 크기 이하인 텍스처를 건너뜁니다.

`Only Process Selected Extensions`: 선택한 파일 형식만 처리합니다. 지원 형식은 `PNG`, `JPG / JPEG`, `TGA`입니다.

`Exclude Path Keywords`: 경로에 포함된 키워드로 텍스처를 제외합니다. 여러 키워드는 쉼표, 세미콜론 또는 줄바꿈으로 구분할 수 있습니다.

예시:

```text
NoCompress, UI, Normal
```

### 설정

`Max Size`: 임포트 후 최대 텍스처 크기를 제한합니다. 값이 작을수록 메모리와 다운로드 크기는 줄지만 디테일도 줄어듭니다.

`Compression Mode`: Unity 텍스처 압축 품질 설정입니다.

`Use Crunch Compression`: Crunch 압축을 적용합니다. 크기를 더 줄일 수 있지만 임포트 시간이 길어지거나 품질이 낮아질 수 있습니다.

`Platform Overrides`: `Standalone`과 `Android`에 플랫폼 전용 크기와 포맷을 설정합니다. VRChat PC는 보통 `Standalone`, Quest / Mobile은 보통 `Android`를 사용합니다.

### 위험 안내

일괄 압축은 Unity 텍스처 임포트 설정과 `.meta` 파일을 변경합니다. 대량 적용 전에는 프로젝트를 백업하거나 버전 관리를 사용하는 것이 좋습니다.

`Max Size`가 너무 낮으면 큰 벽, 바닥, 포스터, 글자, UI, 얼굴, 의상 디테일이 흐려질 수 있습니다.

노멀 맵에 잘못된 설정을 적용하면 조명, 굴곡, 하이라이트가 이상하게 보일 수 있습니다. 의도적으로 처리하는 경우가 아니라면 `Skip Normal Maps`를 켜는 것이 좋습니다.

Mobile / Android 형식이 PC에 항상 적합한 것은 아니며, PC 형식이 Quest에 항상 적합한 것도 아닙니다. 업로드 전에 대상 플랫폼별로 확인하세요.

Crunch 압축은 크기를 줄이는 데 도움이 되지만, 중요한 텍스처는 Dry Run과 소규모 테스트로 먼저 확인하세요.

도구는 Unity Undo를 등록하지만, 텍스처 재임포트와 플랫폼 오버라이드 변경은 Unity 버전에 따라 Undo로 완전히 복구되지 않을 수 있습니다. 가장 안전한 복구 방법은 버전 관리 또는 프로젝트 백업입니다.

많은 텍스처를 처리하면 Unity가 잠시 멈출 수 있습니다. 진행 표시줄에서 취소할 수 있지만, 취소 전에 이미 처리된 텍스처는 자동으로 되돌아가지 않습니다.
