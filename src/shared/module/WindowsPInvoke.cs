namespace Arise.Module;

[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal static unsafe partial class WindowsPInvoke
{
    // CsWin32 does not support ntdll.dll APIs...

    [StructLayout(LayoutKind.Explicit)]
    public struct KUSER_SHARED_DATA
    {
        [FieldOffset(0x2d4)]
        public byte KdDebuggerEnabled;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES
    {
        public uint Length;

        public nint RootDirectory;

        public void* ObjectName;

        public uint Attributes;

        public void* SecurityDescriptor;

        public void* SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct OBJECT_TYPE_INFORMATION
    {
        [FieldOffset(0x10)]
        public uint TotalNumberOfObjects;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PEB
    {
        [FieldOffset(0x2)]
        public bool BeingDebugged;

        [FieldOffset(0xbc)]
        public uint NtGlobalFlag;

        [FieldOffset(0x378)]
        public uint TracingFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SYSTEM_KERNEL_DEBUGGER_INFORMATION
    {
        [FieldOffset(0x0)]
        public bool DebuggerEnabled;

        [FieldOffset(0x1)]
        public bool DebuggerNotPresent;
    }

    private const string NtDll = "ntdll.dll";

    public const nint NtCurrentProcess = -1;

    public const nint NtCurrentThread = -1;

    public const int ObjectTypeInformation = 2;

    public const int ProcessDebugPort = 7;

    public const int ProcessDebugObjectHandle = 30;

    public const int ProcessDebugFlags = 31;

    public const int ThreadHideFromDebugger = 17;

    public const int SystemKernelDebuggerInformation = 35;

    public const int SysDbgQuerySpecialCalls = 6;

    public const int HeapTracingEnabled = 0b001;

    public const int CritSecTracingEnabled = 0b010;

    public const int LibLoaderTracingEnabled = 0b100;

    public const uint DEBUG_ALL_ACCESS = 0b111110000000000001111;

    public const uint FLG_HEAP_ENABLE_TAIL_CHECK = 0b0000000000000000000000000010000;

    public const uint FLG_HEAP_ENABLE_FREE_CHECK = 0b0000000000000000000000000100000;

    public const uint FLG_HEAP_VALIDATE_PARAMETERS = 0b0000000000000000000000001000000;

    public const uint FLG_HEAP_VALIDATE_ALL = 0b0000000000000000000000010000000;

    public const uint FLG_APPLICATION_VERIFIER = 0b0000000000000000000000100000000;

    public const uint FLG_MONITOR_SILENT_PROCESS_EXIT = 0b0000000000000000000001000000000;

    public const uint FLG_USER_STACK_TRACE_DB = 0b0000000000000000001000000000000;

    public const uint FLG_ENABLE_SYSTEM_CRIT_BREAKS = 0b0000000000100000000000000000000;

    public const uint FLG_ENABLE_CLOSE_EXCEPTIONS = 0b0000000010000000000000000000000;

    public const uint FLG_HEAP_PAGE_ALLOCS = 0b0000010000000000000000000000000;

    public const uint FLG_STOP_ON_UNHANDLED_EXCEPTION = 0b0100000000000000000000000000000;

    public const uint FLG_ENABLE_HANDLE_EXCEPTIONS = 0b1000000000000000000000000000000;

    public const uint SE_DEBUG_PRIVILEGE = 20;

    public const int STATUS_DEBUGGER_INACTIVE = unchecked((int)0xc0000354);

    public const int STATUS_SUCCESS = 0x00000000;

    public static readonly KUSER_SHARED_DATA* UserSharedData = (KUSER_SHARED_DATA*)0x7ffe0000;

    public static void InitializeObjectAttributes(OBJECT_ATTRIBUTES* p, void* n, uint a, IntPtr r, void* s)
    {
        p->Length = (uint)sizeof(OBJECT_ATTRIBUTES);
        p->RootDirectory = r;
        p->Attributes = a;
        p->ObjectName = n;
        p->SecurityDescriptor = s;
        p->SecurityQualityOfService = null;
    }

    [LibraryImport(NtDll, EntryPoint = "NtClose")]
    public static partial int NtClose(nint objectHandle);

    [LibraryImport(NtDll, EntryPoint = "NtCreateDebugObject")]
    public static partial int NtCreateDebugObject(
        out nint debugObjectHandle, uint desiredAccess, OBJECT_ATTRIBUTES* objectAttributes, uint killProcessOnExit);

    [LibraryImport(NtDll, EntryPoint = "NtQueryInformationProcess")]
    public static partial int NtQueryInformationProcess(
        nint processHandle,
        int processInformationClass,
        void* processInformation,
        uint processInformationLength,
        out uint returnLength);

    [LibraryImport(NtDll, EntryPoint = "NtQueryInformationThread")]
    public static partial int NtQueryInformationThread(
        nint threadHandle,
        int threadInformationClass,
        void* threadInformation,
        uint threadInformationLength,
        out uint returnLength);

    [LibraryImport(NtDll, EntryPoint = "NtQueryObject")]
    public static partial int NtQueryObject(
        nint handle,
        int objectInformationClass,
        void* objectInformation,
        uint objectInformationLength,
        out uint returnLength);

    [LibraryImport(NtDll, EntryPoint = "NtQuerySystemInformation")]
    public static partial int NtQuerySystemInformation(
        int systemInformationClass, void* systemInformation, uint systemInformationLength, out uint returnLength);

    [LibraryImport(NtDll, EntryPoint = "NtSetInformationThread")]
    public static partial int NtSetInformationThread(
        nint threadHandle, int threadInformationClass, void* threadInformation, uint threadInformationLength);

    [LibraryImport(NtDll, EntryPoint = "NtSystemDebugControl")]
    public static partial int NtSystemDebugControl(
        int command,
        void* inputBuffer,
        uint inputBufferLength,
        void* outputBuffer,
        uint outputBufferLength,
        out uint returnLength);

    [LibraryImport(NtDll, EntryPoint = "NtTerminateProcess")]
    public static partial int NtTerminateProcess(nint processHandle, int exitStatus);

    [LibraryImport(NtDll, EntryPoint = "RtlAdjustPrivilege")]
    public static partial int RtlAdjustPrivilege(
        uint privilege,
        [MarshalAs(UnmanagedType.U1)] bool enable,
        [MarshalAs(UnmanagedType.U1)] bool currentThread,
        [MarshalAs(UnmanagedType.U1)] out bool enabled);

    [LibraryImport(NtDll, EntryPoint = "RtlGetCurrentPeb")]
    public static partial PEB* RtlGetCurrentPeb();
}
