# 《寒霜庇护所》 Unity 项目配置指南

## 环境要求

- **Unity Hub**: 3.x+
- **Unity Editor**: 2022.3 LTS
- **构建平台**: Android (API 28+) / iOS (14+)

## 打开项目

1. 启动 **Unity Hub**
2. 点击 **Open** → **Add project from disk**
3. 选择 `/Assets` 的上级目录（本项目根目录）
4. 等待 Unity 导入资源（首次约 3-5 分钟）

## 首次配置（自动）

打开项目后，Unity 菜单栏:

1. **FrostShelter → Create All Configs**: 一键创建所有 ScriptableObject 配置文件
2. 检查 `Assets/_Project/Resources/Configs/` 目录是否生成了 `.asset` 文件
3. 打开 `Assets/_Project/Scenes/Main.unity`

## 场景结构

```
Main.unity
├── Main Camera (正交俯视视角)
├── Directional Light (冷色光)
├── GameBootstrap (启动器，挂载GameBootstrap.cs)
├── MainCanvas
│   └── PanelRoot (所有UI面板的父节点)
└── EventSystem
```

## 运行

1. 点击 **Play** 按钮
2. GameBootstrap 自动初始化所有服务
3. Console 中看到 `[FrostShelter] Game initialized` 即成功

## 构建

1. **File → Build Settings**
2. 选择 Android / iOS
3. **Player Settings**:
   - Company Name: FrostShelter Studio
   - Product Name: 寒霜庇护所
   - Package Name: com.frostshelter.game

## 代码模块清单 (99 files / 8770 lines)

| 模块 | 路径 | 说明 |
|------|------|------|
| Core | `Scripts/Core/` | GameManager, ServiceLocator, EventDispatcher |
| Save | `Scripts/SaveSystem/` | SaveData, SaveManager, AES-128加密 |
| Time | `Scripts/TimeEngine/` | 在线tick, 离线收益 |
| Resource | `Scripts/Resource/` | 资源CRUD, 游商 |
| Temperature | `Scripts/Temperature/` | 温度梯度, 暴风雪 |
| Building | `Scripts/Building/` | 13种建筑 |
| Survivor | `Scripts/Survivor/` | 幸存者生命周期 |
| Hero | `Scripts/Hero/` | 英雄养成 |
| Battle | `Scripts/Battle/` | 双战斗系统 |
| Exploration | `Scripts/Exploration/` | Roguelike六边形地图 |
| TechTree | `Scripts/TechTree/` | 3列科技树 |
| Events | `Scripts/Events/` | 20+随机事件 |
| Story | `Scripts/Story/` | 3章多结局 |
| UI | `Scripts/UI/` | 完整UI框架 |
| DataTable | `Scripts/DataTable/` | CSV配置加载 |
| Editor | `Scripts/Editor/` | 编辑器工具 |
