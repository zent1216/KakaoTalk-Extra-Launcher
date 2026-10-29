// 카카오톡 추가 실행기
// 일반 카카오톡은 Windows에서 그대로 실행하고, 두 번째 카카오톡을 Sandboxie-Plus 상자 Kakao1에서 시작한다.
// Sandboxie-Plus 명령행(Start.exe, SbieIni.exe)만 호출하며 KakaoTalk.exe나 인증 기능은 건드리지 않는다.
// C# 5 (.NET Framework csc) 문법만 사용한다.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("카카오톡 추가 실행기")]
[assembly: System.Reflection.AssemblyProduct("KakaoTalk Extra Launcher")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]

namespace KakaoExtra
{
    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try { SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(244, 245, 248);
        public static readonly Color Card = Color.White;
        public static readonly Color Border = Color.FromArgb(226, 228, 234);
        public static readonly Color Text = Color.FromArgb(28, 31, 38);
        public static readonly Color Muted = Color.FromArgb(110, 116, 130);
        public static readonly Color Header = Color.FromArgb(34, 38, 66);
        public static readonly Color HeaderSub = Color.FromArgb(178, 186, 226);
        public static readonly Color Accent = Color.FromArgb(84, 96, 222);
        public static readonly Color AccentHover = Color.FromArgb(70, 82, 206);
        public static readonly Color AccentDown = Color.FromArgb(56, 66, 176);
        public static readonly Color Disabled = Color.FromArgb(178, 183, 198);
        public static readonly Color Ok = Color.FromArgb(32, 164, 96);
        public static readonly Color Warn = Color.FromArgb(226, 150, 20);
        public static readonly Color Bad = Color.FromArgb(220, 64, 64);
        public static readonly Color Idle = Color.FromArgb(180, 185, 196);
    }

    // ---------- 그리기 컨트롤 ----------

    sealed class RoundButton : Control
    {
        bool hover, down;
        public bool Primary = true;

        public RoundButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { down = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);

            Color fill, fg, border;
            if (Primary)
            {
                fill = !Enabled ? Theme.Disabled : down ? Theme.AccentDown : hover ? Theme.AccentHover : Theme.Accent;
                fg = Color.White;
                border = fill;
            }
            else
            {
                fill = down ? Color.FromArgb(232, 234, 240) : hover ? Color.FromArgb(242, 243, 247) : Color.White;
                fg = Enabled ? Theme.Text : Theme.Muted;
                border = Theme.Border;
            }

            using (var path = Shapes.Round(r, Height * 0.24f))
            using (var b = new SolidBrush(fill))
            using (var p = new Pen(border))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    sealed class Card : Panel
    {
        public float Radius = 10;

        public Card()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Card;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Shapes.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Radius))
            using (var b = new SolidBrush(Theme.Card))
            using (var p = new Pen(Theme.Border))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }
        }
    }

    sealed class Canvas : Panel
    {
        public Canvas()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
    }

    sealed class Dot : Control
    {
        Color color = Theme.Idle;
        public Color DotColor { get { return color; } set { color = value; Invalidate(); } }

        public Dot()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Theme.Card;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(color))
                e.Graphics.FillEllipse(b, 0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
        }
    }

    // ---------- 경로 탐색 ----------

    static class Locator
    {
        public static string FindSandboxieStart(out string source)
        {
            string dir = ReadValue(RegistryHive.LocalMachine, RegistryView.Registry64,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie-Plus_is1", "InstallLocation");
            if (Check(dir, "Start.exe")) { source = "설치 정보에서 찾음"; return Path.Combine(dir, "Start.exe"); }

            string svc = ReadValue(RegistryHive.LocalMachine, RegistryView.Registry64,
                @"SYSTEM\CurrentControlSet\Services\SbieSvc", "ImagePath");
            string svcExe = CleanExePath(svc);
            if (svcExe != null && Check(Path.GetDirectoryName(svcExe), "Start.exe"))
            {
                source = "Sandboxie 서비스 위치에서 찾음";
                return Path.Combine(Path.GetDirectoryName(svcExe), "Start.exe");
            }

            foreach (var root in ProgramRoots())
            {
                foreach (var name in new[] { "Sandboxie-Plus", "Sandboxie" })
                {
                    string d = Path.Combine(root, name);
                    if (Check(d, "Start.exe")) { source = "기본 설치 폴더에서 찾음"; return Path.Combine(d, "Start.exe"); }
                }
            }
            source = null;
            return null;
        }

        public static string FindKakaoTalk(out string source)
        {
            foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
            {
                foreach (var view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
                {
                    string icon = ReadValue(hive, view, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\KakaoTalk", "DisplayIcon");
                    string exe = CleanExePath(icon);
                    if (exe != null && File.Exists(exe)) { source = "설치 정보에서 찾음"; return exe; }

                    string loc = ReadValue(hive, view, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\KakaoTalk", "InstallLocation");
                    if (Check(loc, "KakaoTalk.exe")) { source = "설치 정보에서 찾음"; return Path.Combine(loc, "KakaoTalk.exe"); }
                }
            }

            foreach (var p in Process.GetProcessesByName("KakaoTalk"))
            {
                try
                {
                    string f = p.MainModule.FileName;
                    if (File.Exists(f)) { source = "실행 중인 카카오톡에서 찾음"; return f; }
                }
                catch { }
                finally { p.Dispose(); }
            }

            var roots = new List<string>(ProgramRoots());
            roots.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            foreach (var root in roots)
            {
                string d = Path.Combine(root, @"Kakao\KakaoTalk");
                if (Check(d, "KakaoTalk.exe")) { source = "기본 설치 폴더에서 찾음"; return Path.Combine(d, "KakaoTalk.exe"); }
            }
            source = null;
            return null;
        }

        static IEnumerable<string> ProgramRoots()
        {
            foreach (var v in new[] { "ProgramW6432", "ProgramFiles", "ProgramFiles(x86)" })
            {
                string r = Environment.GetEnvironmentVariable(v);
                if (!string.IsNullOrEmpty(r)) yield return r;
            }
        }

        static bool Check(string dir, string file)
        {
            return !string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir.Trim().Trim('"'), file));
        }

        static string ReadValue(RegistryHive hive, RegistryView view, string key, string name)
        {
            try
            {
                using (var root = RegistryKey.OpenBaseKey(hive, view))
                using (var k = root.OpenSubKey(key))
                {
                    if (k == null) return null;
                    var v = k.GetValue(name) as string;
                    return v == null ? null : Environment.ExpandEnvironmentVariables(v.Trim());
                }
            }
            catch { return null; }
        }

        // "C:\...\x.exe",0  /  "C:\...\x.exe" -args  /  C:\...\x.exe  →  C:\...\x.exe
        static string CleanExePath(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            raw = raw.Trim();
            if (raw.StartsWith("\""))
            {
                int end = raw.IndexOf('"', 1);
                return end > 1 ? raw.Substring(1, end - 1) : null;
            }
            int exe = raw.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            return exe > 0 ? raw.Substring(0, exe + 4) : null;
        }
    }

    // ---------- Sandboxie 연동 ----------

    static class Sbie
    {
        public const string Box = "Kakao1";

        public static int Run(string exe, string args, int timeoutMs, out string stdout)
        {
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var p = Process.Start(psi))
            {
                var outTask = p.StandardOutput.ReadToEndAsync();
                p.StandardError.ReadToEndAsync();
                if (!p.WaitForExit(timeoutMs)) { stdout = ""; return -1; }
                stdout = outTask.Result;
                return p.ExitCode;
            }
        }

        static string IniTool(string startExe)
        {
            return Path.Combine(Path.GetDirectoryName(startExe), "SbieIni.exe");
        }

        public static bool BoxExists(string startExe)
        {
            string output;
            Run(IniTool(startExe), "query " + Box + " Enabled", 10000, out output);
            return output.Trim().Length > 0;
        }

        public static string CreateBox(string startExe)
        {
            string ini = IniTool(startExe);
            if (!File.Exists(ini)) return "SbieIni.exe를 찾을 수 없습니다 (" + ini + ").";
            string output;
            int code = Run(ini, "set " + Box + " Enabled y", 10000, out output);
            if (code != 0) return string.Format("SbieIni.exe가 오류 코드 {0}로 끝났습니다.", code);
            Run(startExe, "/reload", 10000, out output);
            return BoxExists(startExe) ? null : "설정을 쓴 뒤에도 상자가 확인되지 않습니다.";
        }

        // Start.exe /listpids 출력: 첫 줄은 개수, 이후 한 줄에 PID 하나
        public static HashSet<int> BoxPids(string startExe)
        {
            var set = new HashSet<int>();
            if (string.IsNullOrEmpty(startExe) || !File.Exists(startExe)) return set;
            try
            {
                string output;
                if (Run(startExe, "/box:" + Box + " /listpids", 8000, out output) != 0) return set;
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < lines.Length; i++)
                {
                    int pid;
                    if (int.TryParse(lines[i].Trim(), out pid)) set.Add(pid);
                }
            }
            catch { }
            return set;
        }

        public static int Launch(string startExe, string kakaoExe)
        {
            // 출력 리다이렉트를 하지 않는다: 자식 프로세스가 파이프를 물고 있으면 대기가 끝나지 않는다.
            var psi = new ProcessStartInfo(startExe,
                string.Format("/box:{0} /silent \"{1}\"", Box, kakaoExe))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(kakaoExe)
            };
            using (var p = Process.Start(psi))
            {
                return p.WaitForExit(15000) ? p.ExitCode : 0;
            }
        }
    }

    static class Windows
    {
        delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc proc, IntPtr lParam);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int pid);
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int L, T, R, B; }
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT r);

        public static bool HasVisibleWindow(ICollection<int> pids)
        {
            if (pids.Count == 0) return false;
            bool found = false;
            EnumWindows(delegate(IntPtr h, IntPtr l)
            {
                int pid;
                GetWindowThreadProcessId(h, out pid);
                RECT r;
                if (pids.Contains(pid) && IsWindowVisible(h) && GetWindowRect(h, out r) && r.R - r.L >= 100 && r.B - r.T >= 100)
                {
                    found = true;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        public static HashSet<int> KakaoPids()
        {
            var set = new HashSet<int>();
            foreach (var p in Process.GetProcessesByName("KakaoTalk")) { set.Add(p.Id); p.Dispose(); }
            return set;
        }
    }

    sealed class Snapshot
    {
        public List<int> Normal = new List<int>();
        public List<int> Extra = new List<int>();
        public string SbieText;
        public Color SbieColor;
    }

    // ---------- 메인 창 ----------

    sealed class MainForm : Form
    {
        readonly float scale;
        readonly Font fontBase, fontSmall, fontTitle, fontButton;
        readonly string settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KakaoExtraLauncher", "settings.ini");

        string startExe, kakaoExe, startSource, kakaoSource;
        Dot dotNormal, dotExtra, dotSbie;
        Label valNormal, valExtra, valSbie, capStart, capKakao, pathStart, pathKakao;
        RoundButton btnLaunch;
        TextBox log;
        readonly Timer timer = new Timer();
        bool busy, refreshing;
        Snapshot last;

        int S(float v) { return (int)Math.Round(v * scale); }

        public MainForm()
        {
            using (var g = CreateGraphics()) scale = g.DpiX / 96f;
            fontBase = new Font("Malgun Gothic", 9f);
            fontSmall = new Font("Malgun Gothic", 8.25f);
            fontTitle = new Font("Malgun Gothic", 13f, FontStyle.Bold);
            fontButton = new Font("Malgun Gothic", 10.5f, FontStyle.Bold);

            Text = "카카오톡 추가 실행기";
            Font = fontBase;
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(480), S(616));
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            BuildUi();
            LoadPaths();

            timer.Interval = 2500;
            timer.Tick += async delegate { await RefreshStatus(); };
            Shown += async delegate { timer.Start(); await RefreshStatus(); };
            FormClosing += delegate { timer.Stop(); SavePaths(); };
        }

        void BuildUi()
        {
            var header = new Canvas { Location = Point.Empty, Size = new Size(S(480), S(84)), BackColor = Theme.Header };
            header.Paint += (s, e) =>
            {
                IconArt.Draw(e.Graphics, S(20), S(18), S(48));
                TextRenderer.DrawText(e.Graphics, "카카오톡 추가 실행기", fontTitle, new Point(S(80), S(17)), Color.White);
                TextRenderer.DrawText(e.Graphics, "Sandboxie 상자 " + Sbie.Box + "에서 두 번째 카카오톡을 엽니다", fontSmall,
                    new Point(S(82), S(48)), Theme.HeaderSub);
            };
            Controls.Add(header);

            Controls.Add(Caption("상태", 96));
            var status = new Card { Location = new Point(S(20), S(118)), Size = new Size(S(440), S(114)), Radius = S(10) };
            AddStatusRow(status, 0, "일반 카카오톡", out dotNormal, out valNormal);
            AddStatusRow(status, 1, "추가 카카오톡 (" + Sbie.Box + ")", out dotExtra, out valExtra);
            AddStatusRow(status, 2, "Sandboxie-Plus", out dotSbie, out valSbie);
            Controls.Add(status);

            btnLaunch = new RoundButton
            {
                Text = "추가 계정 실행",
                Font = fontButton,
                Location = new Point(S(20), S(246)),
                Size = new Size(S(440), S(50))
            };
            btnLaunch.Click += async delegate { await LaunchExtra(); };
            Controls.Add(btnLaunch);

            Controls.Add(Caption("경로", 310));
            var paths = new Card { Location = new Point(S(20), S(332)), Size = new Size(S(440), S(128)), Radius = S(10) };
            AddPathBlock(paths, 0, out capStart, out pathStart, delegate { BrowseStart(); });
            AddPathBlock(paths, 1, out capKakao, out pathKakao, delegate { BrowseKakao(); });
            Controls.Add(paths);

            Controls.Add(Caption("기록", 474));
            var logCard = new Card { Location = new Point(S(20), S(496)), Size = new Size(S(440), S(104)), Radius = S(10) };
            log = new TextBox
            {
                Location = new Point(S(12), S(10)),
                Size = new Size(S(416), S(84)),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Card,
                ForeColor = Theme.Muted,
                ScrollBars = ScrollBars.Vertical,
                Font = fontSmall,
                TabStop = false
            };
            logCard.Controls.Add(log);
            Controls.Add(logCard);
        }

        Label Caption(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Malgun Gothic", 8.25f, FontStyle.Bold),
                ForeColor = Theme.Muted,
                BackColor = Theme.Bg,
                Location = new Point(S(22), S(y)),
                Size = new Size(S(200), S(20)),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        void AddStatusRow(Card card, int i, string name, out Dot dot, out Label value)
        {
            int y = 14 + i * 32;
            dot = new Dot { Location = new Point(S(18), S(y + 5)), Size = new Size(S(10), S(10)) };
            var label = new Label
            {
                Text = name, ForeColor = Theme.Text, BackColor = Theme.Card,
                Location = new Point(S(38), S(y)), Size = new Size(S(170), S(20)), TextAlign = ContentAlignment.MiddleLeft
            };
            value = new Label
            {
                Text = "확인 중…", ForeColor = Theme.Muted, BackColor = Theme.Card,
                Location = new Point(S(200), S(y)), Size = new Size(S(224), S(20)), TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(dot);
            card.Controls.Add(label);
            card.Controls.Add(value);
        }

        void AddPathBlock(Card card, int i, out Label caption, out Label path, EventHandler onChange)
        {
            int y = i * 58;
            caption = new Label
            {
                Font = fontSmall, ForeColor = Theme.Muted, BackColor = Theme.Card,
                Location = new Point(S(16), S(y + 12)), Size = new Size(S(330), S(18)), TextAlign = ContentAlignment.MiddleLeft
            };
            path = new Label
            {
                ForeColor = Theme.Text, BackColor = Theme.Card, AutoEllipsis = true,
                Location = new Point(S(16), S(y + 31)), Size = new Size(S(332), S(20)), TextAlign = ContentAlignment.MiddleLeft
            };
            var btn = new RoundButton
            {
                Primary = false, Text = "찾기", Font = fontSmall,
                Location = new Point(S(360), S(y + 19)), Size = new Size(S(64), S(30))
            };
            btn.Click += onChange;
            card.Controls.Add(caption);
            card.Controls.Add(path);
            card.Controls.Add(btn);
        }

        // ---------- 경로 ----------

        void LoadPaths()
        {
            string savedStart = null, savedKakao = null;
            try
            {
                if (File.Exists(settingsPath))
                {
                    foreach (var line in File.ReadAllLines(settingsPath, Encoding.UTF8))
                    {
                        int i = line.IndexOf('=');
                        if (i <= 0) continue;
                        string k = line.Substring(0, i), v = line.Substring(i + 1);
                        if (k == "start") savedStart = v;
                        else if (k == "kakao") savedKakao = v;
                    }
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(savedStart) && File.Exists(savedStart)) { startExe = savedStart; startSource = "저장된 경로"; }
            else startExe = Locator.FindSandboxieStart(out startSource);

            if (!string.IsNullOrEmpty(savedKakao) && File.Exists(savedKakao)) { kakaoExe = savedKakao; kakaoSource = "저장된 경로"; }
            else kakaoExe = Locator.FindKakaoTalk(out kakaoSource);

            ShowPaths();
            Log(startExe != null
                ? "Sandboxie-Plus: " + startExe + " (" + startSource + ")"
                : "Sandboxie-Plus Start.exe를 자동으로 찾지 못했습니다. '찾기'로 지정하세요.");
            Log(kakaoExe != null
                ? "카카오톡: " + kakaoExe + " (" + kakaoSource + ")"
                : "KakaoTalk.exe를 자동으로 찾지 못했습니다. '찾기'로 지정하세요.");
            SavePaths();
        }

        void ShowPaths()
        {
            capStart.Text = "Sandboxie-Plus · Start.exe" + (startSource != null ? "  ·  " + startSource : "");
            capKakao.Text = "카카오톡 · KakaoTalk.exe" + (kakaoSource != null ? "  ·  " + kakaoSource : "");
            pathStart.Text = startExe ?? "찾지 못함 — '찾기'를 눌러 지정하세요";
            pathKakao.Text = kakaoExe ?? "찾지 못함 — '찾기'를 눌러 지정하세요";
            pathStart.ForeColor = startExe != null ? Theme.Text : Theme.Bad;
            pathKakao.ForeColor = kakaoExe != null ? Theme.Text : Theme.Bad;
        }

        void SavePaths()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                File.WriteAllLines(settingsPath, new[] { "start=" + (startExe ?? ""), "kakao=" + (kakaoExe ?? "") }, Encoding.UTF8);
            }
            catch { }
        }

        void BrowseStart()
        {
            string f = Pick("Sandboxie-Plus의 Start.exe 선택", "Start.exe|Start.exe|실행 파일 (*.exe)|*.exe", startExe);
            if (f == null) return;
            startExe = f;
            startSource = "직접 지정";
            ShowPaths();
            SavePaths();
            Log("Sandboxie-Plus 경로를 지정했습니다: " + f);
        }

        void BrowseKakao()
        {
            string f = Pick("공식 KakaoTalk.exe 선택", "KakaoTalk.exe|KakaoTalk.exe|실행 파일 (*.exe)|*.exe", kakaoExe);
            if (f == null) return;
            kakaoExe = f;
            kakaoSource = "직접 지정";
            ShowPaths();
            SavePaths();
            Log("카카오톡 경로를 지정했습니다: " + f);
        }

        string Pick(string title, string filter, string current)
        {
            using (var dlg = new OpenFileDialog { Title = title, Filter = filter })
            {
                if (current != null && File.Exists(current)) dlg.InitialDirectory = Path.GetDirectoryName(current);
                return dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
            }
        }

        // ---------- 상태 ----------

        async Task RefreshStatus()
        {
            if (refreshing) return;
            refreshing = true;
            try
            {
                string start = startExe;
                var snap = await Task.Run(() => TakeSnapshot(start));
                ApplySnapshot(snap);
            }
            finally { refreshing = false; }
        }

        static Snapshot TakeSnapshot(string start)
        {
            var snap = new Snapshot();
            var all = Windows.KakaoPids();
            var box = Sbie.BoxPids(start);
            foreach (int pid in all)
            {
                if (box.Contains(pid)) snap.Extra.Add(pid);
                else snap.Normal.Add(pid);
            }

            try
            {
                using (var sc = new ServiceController("SbieSvc"))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        string ver = null;
                        if (start != null && File.Exists(start))
                        {
                            string sandman = Path.Combine(Path.GetDirectoryName(start), "SandMan.exe");
                            try { ver = FileVersionInfo.GetVersionInfo(File.Exists(sandman) ? sandman : start).ProductVersion; } catch { }
                        }
                        snap.SbieText = "정상 작동" + (string.IsNullOrEmpty(ver) ? "" : " · v" + ver);
                        snap.SbieColor = Theme.Ok;
                    }
                    else
                    {
                        snap.SbieText = "서비스가 멈춰 있음";
                        snap.SbieColor = Theme.Bad;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                snap.SbieText = "설치되지 않음";
                snap.SbieColor = Theme.Bad;
            }
            return snap;
        }

        void ApplySnapshot(Snapshot s)
        {
            last = s;
            SetRow(dotNormal, valNormal, s.Normal.Count > 0 ? Theme.Ok : Theme.Warn,
                s.Normal.Count > 0 ? "실행 중" : "실행 안 됨 · 먼저 실행하세요");
            SetRow(dotExtra, valExtra, s.Extra.Count > 0 ? Theme.Ok : Theme.Idle,
                s.Extra.Count > 0 ? "실행 중" : "대기");
            SetRow(dotSbie, valSbie, s.SbieColor, s.SbieText);
            if (!busy) btnLaunch.Text = s.Extra.Count > 0 ? "추가 카카오톡 창 열기" : "추가 계정 실행";
        }

        static void SetRow(Dot dot, Label value, Color color, string text)
        {
            dot.DotColor = color;
            value.Text = text;
            value.ForeColor = color == Theme.Idle ? Theme.Muted : color == Theme.Ok ? Theme.Text : color;
        }

        // ---------- 실행 ----------

        async Task LaunchExtra()
        {
            if (busy) return;
            if (startExe == null || !File.Exists(startExe)) { Log("실패: Sandboxie-Plus Start.exe 경로가 올바르지 않습니다."); return; }
            if (kakaoExe == null || !File.Exists(kakaoExe)) { Log("실패: KakaoTalk.exe 경로가 올바르지 않습니다."); return; }

            busy = true;
            btnLaunch.Enabled = false;
            btnLaunch.Text = "실행 중…";
            try
            {
                if (last != null && last.Normal.Count == 0)
                    Log("안내: 일반 카카오톡이 실행 중이 아닙니다. 첫 번째 계정을 먼저 여는 것을 권장합니다.");

                string start = startExe, kakao = kakaoExe;
                string boxError = await Task.Run(() => Sbie.BoxExists(start) ? null : CreateBoxLogged(start));
                if (boxError != null)
                {
                    Log("실패: " + Sbie.Box + " 상자를 만들지 못했습니다 — " + boxError);
                    Log("Sandboxie 설정이 암호로 보호돼 있거나 서비스가 멈춰 있을 수 있습니다. Sandboxie-Plus에서 " + Sbie.Box + " 상자를 직접 만든 뒤 다시 시도하세요.");
                    return;
                }

                var before = await Task.Run(() => Sbie.BoxPids(start));
                var allBefore = Windows.KakaoPids();
                bool wasRunning = false;
                foreach (int pid in allBefore) if (before.Contains(pid)) wasRunning = true;
                if (wasRunning) Log(Sbie.Box + " 상자에서 카카오톡이 이미 실행 중입니다. 기존 창을 다시 엽니다.");

                int code = await Task.Run(() => Sbie.Launch(start, kakao));
                if (code != 0)
                {
                    Log(string.Format("실패: Sandboxie Start.exe가 오류 코드 {0}로 끝났습니다. Sandboxie-Plus 창에서 메시지를 확인하세요.", code));
                    return;
                }
                Log("실행 요청을 보냈습니다. 카카오톡 창이 뜨는지 확인하는 중…");

                await WaitForWindow(start, allBefore, wasRunning);
            }
            catch (Exception ex)
            {
                Log("실패: " + ex.Message);
            }
            finally
            {
                busy = false;
                btnLaunch.Enabled = true;
            }
            await RefreshStatus();
        }

        string CreateBoxLogged(string start)
        {
            BeginInvoke(new Action(() => Log(Sbie.Box + " 상자가 없어 새로 만듭니다.")));
            string err = Sbie.CreateBox(start);
            if (err == null) BeginInvoke(new Action(() => Log(Sbie.Box + " 상자를 만들었습니다.")));
            return err;
        }

        async Task<bool> WaitForWindow(string start, HashSet<int> allBefore, bool wasRunning)
        {
            var sw = Stopwatch.StartNew();
            var inBox = new List<int>();
            while (sw.Elapsed.TotalSeconds < 25)
            {
                await Task.Delay(800);
                var box = await Task.Run(() => Sbie.BoxPids(start));
                inBox.Clear();
                foreach (int pid in Windows.KakaoPids()) if (box.Contains(pid)) inBox.Add(pid);

                if (Windows.HasVisibleWindow(inBox))
                {
                    bool isNew = false;
                    foreach (int pid in inBox) if (!allBefore.Contains(pid)) isNew = true;
                    Log(isNew || !wasRunning
                        ? "성공: " + Sbie.Box + " 상자의 카카오톡 창이 열렸습니다. 두 번째 계정으로 로그인하세요."
                        : "성공: " + Sbie.Box + " 상자의 기존 카카오톡 창이 표시됐습니다.");
                    return true;
                }
            }

            if (inBox.Count > 0)
                Log("확인 불가: " + Sbie.Box + " 상자에서 카카오톡(PID " + string.Join(", ", inBox) + ")은 실행 중이지만 25초 안에 창을 찾지 못했습니다. 작업 표시줄 트레이 아이콘을 확인하세요.");
            else
                Log("실패: " + Sbie.Box + " 상자에서 카카오톡 프로세스가 확인되지 않습니다. Sandboxie-Plus 창에서 " + Sbie.Box + " 상자를 확인하세요.");
            return false;
        }

        void Log(string message)
        {
            if (log.TextLength > 0) log.AppendText(Environment.NewLine);
            log.AppendText(string.Format("{0:HH:mm:ss}  {1}", DateTime.Now, message));
        }
    }
}
