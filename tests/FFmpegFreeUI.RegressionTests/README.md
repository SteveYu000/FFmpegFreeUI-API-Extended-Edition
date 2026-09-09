# Queue and Preset Regression Checks

Run on Windows with .NET 10 and the application's existing LakeUI reference available:

```powershell
dotnet run --project tests/FFmpegFreeUI.RegressionTests/FFmpegFreeUI.RegressionTests.csproj
```

Include real media generation, preset trimming, and FFprobe verification:

```powershell
dotnet run --project tests/FFmpegFreeUI.RegressionTests/FFmpegFreeUI.RegressionTests.csproj -- --ffmpeg C:/Tools/ffmpeg/bin/ffmpeg.exe
```

The executable serves as its own short-lived stdout/stderr fixture. It covers preset
copy isolation, failed file replacement, queue ordering and lookup, cache restoration,
task cancellation and reset, multi-selection, numeric progress boundaries, and scheduler
recovery. No test framework package is required. Test data lives in temporary folders
or the test build output; the application's working settings and presets are not used.

The final measurements compare 1,000 legacy JSON copies with the batch clone factory
after warmup. Timings are diagnostic, not pass/fail thresholds.

## Ownership

- `编码队列_v6.vb`: queue membership, immutable task IDs, lookup, scheduling and output reservations.
- `编码任务_v6.vb`: execution lifecycle, processes, stage data and logs.
- `编码进度_v6.vb`: FFmpeg output parsing and display formatting.
- `预设存储_v6.vb`: preset copying, migration and persistence.

Queue membership changes must go through queue methods. `队列` is a read-only snapshot;
`获取队列快照` returns a detached list. A terminal status does not imply cleanup has
finished: reset, removal and reorder also require `正在执行 = False`. Process output
streams finish before the next stage starts, and process startup shares the task state
lock with cancellation. Queue events may arrive on background threads. Queue membership
and task state changes request a coalesced update on the next UI turn; only progress
and log output wait for the configured refresh timer.
