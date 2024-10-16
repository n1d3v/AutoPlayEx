using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoPlayEx_Forms
{
    public partial class Dialog : Form
    {
        public Dialog()
        {
            InitializeComponent();
            Console.WriteLine("Form1 Constructor: Initializing Form...");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ShowMUIDialog();
        }

        private void ShowMUIDialog()
        {
            string muiFilePath = @"C:\Windows\System32\en-US\shell7.dll.mui";
            Console.WriteLine($"ShowMUIDialog: Loading MUI file from path: {muiFilePath}");

            IntPtr hMUI = LoadLibraryEx(muiFilePath, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE);
            if (hMUI == IntPtr.Zero)
            {
                Console.WriteLine("ShowMUIDialog: Failed to load MUI file.");
                MessageBox.Show("Failed to load MUI file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Console.WriteLine("ShowMUIDialog: MUI file loaded successfully.");
            Console.WriteLine($"ShowMUIDialog: Attempting to create dialog with resource ID: 1121");

            try
            {
                DialogBox(hMUI, 1121);
            }
            finally
            {
                Console.WriteLine("ShowMUIDialog: Freeing MUI library...");
                FreeLibrary(hMUI);
                Console.WriteLine("ShowMUIDialog: MUI library freed.");
            }
        }

        private void DialogBox(IntPtr hModule, int resourceId)
        {
            Console.WriteLine("DialogBox: Creating dialog...");
            IntPtr hDialog = CreateDialog(hModule, resourceId);
            if (hDialog != IntPtr.Zero)
            {
                Console.WriteLine("DialogBox: Dialog created successfully.");
                ShowWindow(hDialog, 1);
                MSG msg;
                while (GetMessage(out msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                Console.WriteLine("DialogBox: Message loop ended.");
            }
            else
            {
                Console.WriteLine("DialogBox: Failed to create dialog.");
            }
        }

        private IntPtr CreateDialog(IntPtr hModule, int resourceId)
        {
            Console.WriteLine($"CreateDialog: Calling CreateDialogParam with resource ID: {resourceId}");
            IntPtr dialog = CreateDialogParam(hModule, (IntPtr)resourceId, IntPtr.Zero, DialogProc, IntPtr.Zero);
            if (dialog == IntPtr.Zero)
            {
                Console.WriteLine($"CreateDialog: CreateDialogParam returned null for resource ID: {resourceId}. Error: {Marshal.GetLastWin32Error()}");
            }
            else
            {
                Console.WriteLine($"CreateDialog: Dialog handle obtained: {dialog}");
            }
            return dialog;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CreateDialogParam(IntPtr hInstance, IntPtr lpTemplateName, IntPtr hWndParent, DialogProcDelegate lpDialogFunc, IntPtr dwInitParam);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        static extern void TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern void DispatchMessage(ref MSG lpMsg);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll")]
        static extern bool FreeLibrary(IntPtr hModule);

        [Flags]
        enum LoadLibraryFlags : uint
        {
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point pt;
        }

        delegate IntPtr DialogProcDelegate(IntPtr hDlg, uint message, IntPtr wParam, IntPtr lParam);

        private IntPtr DialogProc(IntPtr hDlg, uint message, IntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case 0x0110:
                    return IntPtr.Zero;

                case 0x0101:
                    if (wParam.ToInt32() == 1 || wParam.ToInt32() == 2)
                    {
                        EndDialog(hDlg, wParam.ToInt32());
                        return IntPtr.Zero;
                    }
                    break;

                case 0x0010:
                    Environment.Exit(0);
                    return IntPtr.Zero;

                default:
                    break;
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        static extern void EndDialog(IntPtr hDlg, int nResult);
    }
}
