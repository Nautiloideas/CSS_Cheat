# CSS Cheat Overlay

## 概述

CSS Cheat Overlay 是一个用于在游戏窗口上显示信息的覆盖程序。该程序通过读取游戏进程的内存数据，获取玩家和敌人的信息，并在游戏窗口上实时显示这些信息。

## 功能

- **进程选择**：用户可以从当前运行的进程列表中选择目标游戏进程。
- **模块加载**：程序会加载并显示目标进程的所有模块信息。
- **内存读取**：通过读取目标进程的内存数据，获取玩家和敌人的位置、健康值等信息。
- **覆盖窗口**：在目标游戏窗口上显示实时信息，包括玩家和敌人的位置、健康值等。

## 文件说明

### MainForm.cs

- **功能**：主窗口，负责加载进程列表、选择目标进程、启动和停止覆盖窗口。
- **主要方法**：
  - `LoadProcesses()`：加载当前运行的进程列表。
  - `selectProcessButton_Click()`：选择目标进程并加载其模块信息。
  - `btnStartDrawing_Click()`：启动覆盖窗口。
  - `btnStopDrawing_Click()`：停止覆盖窗口。

### OverlayWindow.cs

- **功能**：覆盖窗口，负责在目标游戏窗口上显示实时信息。
- **主要方法**：
  - `StartOverlay()`：启动覆盖窗口并开始更新信息。
  - `UpdateOverlay()`：更新覆盖窗口上的信息。
  - `UpdateOverlayPosition()`：更新覆盖窗口的位置，使其始终位于目标窗口上方。
  - `GetPlayerData()`：读取玩家数据。
  - `GetPlayerHealth()`：读取玩家健康值。
  - `GetEnemyHealth(int enemyIndex)`：读取敌人健康值。

### WindowFinder.cs

- **功能**：查找目标进程的窗口句柄。
- **主要方法**：
  - `FindWindowByProcess(uint targetProcessId, string windowTitle)`：根据进程ID和窗口标题查找窗口句柄。

### Memory.cs

- **功能**：读取目标进程的内存数据。
- **主要方法**：
  - `OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId)`：打开目标进程并获取其句柄。
  - `GetModuleHandle(IntPtr processHandle, uint processId, string moduleName)`：获取目标进程中指定模块的句柄。
  - `GetAllModules(IntPtr processHandle, uint processId)`：获取目标进程的所有模块信息。
  - `ReadMemoryValue(IntPtr processHandle, IntPtr moduleBase, int offset, MemoryValueType valueType)`：读取目标进程内存中的值。

### ModuleListForm.cs

- **功能**：显示目标进程的模块列表。
- **主要方法**：
  - `LoadModules(List<ModuleInfo> modules)`：加载并显示模块列表。
  - `ClearModules()`：清空模块列表。

## 使用说明

1. 启动程序。
2. 在进程列表中选择目标游戏进程。
3. 点击“选择进程”按钮，加载目标进程的模块信息。
4. 点击“开始绘制”按钮，启动覆盖窗口。
5. 覆盖窗口将显示在目标游戏窗口上，并实时更新玩家和敌人的信息。
6. 点击“停止绘制”按钮，停止覆盖窗口。

## 注意事项

- 该程序需要以管理员权限运行，以便读取目标进程的内存数据。
- 请确保目标游戏进程正在运行，并且具有窗口句柄。

## 许可证

该项目使用 MIT 许可证。详细信息请参阅 LICENSE 文件。
