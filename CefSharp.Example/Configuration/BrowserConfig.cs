// Copyright © 2025 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Text.Json;

namespace CefSharp.Example.Configuration
{
    /// <summary>
    /// 浏览器配置模型
    /// </summary>
    public class BrowserConfig
    {
        /// <summary>
        /// 窗口配置
        /// </summary>
        public WindowConfig Window { get; set; } = new WindowConfig();

        /// <summary>
        /// 字体配置
        /// </summary>
        public FontConfig Font { get; set; } = new FontConfig();

        /// <summary>
        /// 浏览器外观配置
        /// </summary>
        public BrowserAppearance Appearance { get; set; } = new BrowserAppearance();

        /// <summary>
        /// CEF 持久化配置
        /// </summary>
        public PersistenceConfig Persistence { get; set; } = new PersistenceConfig();
    }

    /// <summary>
    /// 窗口配置
    /// </summary>
    public class WindowConfig
    {
        /// <summary>
        /// 窗口宽度
        /// </summary>
        public int Width { get; set; } = 2048;

        /// <summary>
        /// 窗口高度
        /// </summary>
        public int Height { get; set; } = 1024;

        /// <summary>
        /// 窗口启动状态：Normal, Maximized, Minimized
        /// </summary>
        public string WindowState { get; set; } = "Normal";

        /// <summary>
        /// 是否记住窗口位置和大小
        /// </summary>
        public bool RememberWindowState { get; set; } = true;
    }

    /// <summary>
    /// 字体配置
    /// </summary>
    public class FontConfig
    {
        /// <summary>
        /// 默认字体名称
        /// </summary>
        public string Family { get; set; } = "Microsoft YaHei";

        /// <summary>
        /// 默认字体大小
        /// </summary>
        public int Size { get; set; } = 16;

        /// <summary>
        /// 是否启用字体平滑
        /// </summary>
        public bool AntiAlias { get; set; } = true;
    }

    /// <summary>
    /// 浏览器外观配置
    /// </summary>
    public class BrowserAppearance
    {
        /// <summary>
        /// 背景颜色（十六进制，如 #1E1E1E）
        /// </summary>
        public string BackgroundColor { get; set; } = "#1E1E1E";

        /// <summary>
        /// 默认网页背景颜色（十六进制）
        /// </summary>
        public string DefaultPageBackgroundColor { get; set; } = "#1E1E1E";

        /// <summary>
        /// 默认文本颜色（十六进制）
        /// </summary>
        public string TextColor { get; set; } = "#FFFFFF";
    }

    /// <summary>
    /// CEF 持久化配置
    /// </summary>
    public class PersistenceConfig
    {
        /// <summary>
        /// 是否启用持久化缓存
        /// </summary>
        public bool EnableCache { get; set; } = true;

        /// <summary>
        /// 缓存目录路径（相对于程序目录）
        /// </summary>
        public string CachePath { get; set; } = "cache";

        /// <summary>
        /// 根缓存目录路径
        /// </summary>
        public string RootCachePath { get; set; } = "cache";

        /// <summary>
        /// 是否持久化 Session Cookies
        /// </summary>
        public bool PersistSessionCookies { get; set; } = true;

        /// <summary>
        /// 是否启用用户偏好持久化
        /// </summary>
        public bool PersistUserPreferences { get; set; } = true;

        /// <summary>
        /// 远程调试端口（0 表示禁用）
        /// </summary>
        public int RemoteDebuggingPort { get; set; } = 8088;
    }

    /// <summary>
    /// 配置管理器
    /// </summary>
    public static class ConfigManager
    {
        private const string ConfigFileName = "cefsharp.config.json";
        private static BrowserConfig? _currentConfig;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public static BrowserConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    lock (_lock)
                    {
                        if (_currentConfig == null)
                        {
                            _currentConfig = LoadOrCreateConfig();
                        }
                    }
                }
                return _currentConfig;
            }
        }

        /// <summary>
        /// 加载或创建默认配置文件
        /// </summary>
        private static BrowserConfig LoadOrCreateConfig()
        {
            try
            {
                var configPath = GetConfigPath();

                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<BrowserConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    });

                    if (config != null)
                    {
                        return config;
                    }
                }

                // 创建默认配置文件
                var defaultConfig = new BrowserConfig();
                SaveConfig(defaultConfig);

                return defaultConfig;
            }
            catch (Exception ex)
            {
                // 如果加载失败，返回默认配置
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
                return new BrowserConfig();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static bool SaveConfig(BrowserConfig config)
        {
            try
            {
                var configPath = GetConfigPath();
                var directory = Path.GetDirectoryName(configPath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, json);

                _currentConfig = config;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        private static string GetConfigPath()
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDirectory = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(exeDirectory, ConfigFileName);
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void ReloadConfig()
        {
            lock (_lock)
            {
                _currentConfig = null;
            }
        }

        /// <summary>
        /// 将颜色字符串转换为 ARGB 值
        /// </summary>
        public static uint ColorToArgb(string colorHex)
        {
            try
            {
                var color = System.Drawing.ColorTranslator.FromHtml(colorHex);
                return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
            }
            catch
            {
                return 0xFF1E1E1E; // 默认深灰色
            }
        }
    }
}
