namespace Arise.Module;

[SuppressMessage("", "SA1307")]
[SuppressMessage("", "SA1310")]
internal static unsafe partial class WindowsPInvoke
{
    // CsWin32 does not support ntdll.dll APIs...

    [StructLayout(LayoutKind.Sequential)]
    public struct PEB
    {
        public bool InheritedAddressSpace;

        public bool ReadImageFileExecOptions;

        public bool BeingDebugged;

        public byte BitField;

        public nint Mutant;

        public void* ImageBaseAddress;

        public void* Ldr;

        public void* ProcessParameters;

        public void* SubSystemData;

        public void* ProcessHeap;

        public void* FastPebLock;

        public void* IFEOKey;

        public void* AtlThunkSListPtr;

        public uint CrossProcessFlags;

        public void* UserSharedInfoPtr;

        public uint SystemReserved;

        public uint AtlThunkSListPtr32;

        public void* ApiSetMap;

        public uint TlsExpansionCounter;

        public void* TlsBitmap;

        public fixed uint TlsBitmapBits[2];

        public void* ReadOnlySharedMemoryBase;

        public void* SharedData;

        public void** ReadOnlyStaticServerData;

        public void* AnsiCodePageData;

        public void* OemCodePageData;

        public void* UnicodeCaseTableData;

        public uint NumberOfProcessors;

        public uint NtGlobalFlag;

        public ulong CriticalSectionTimeout;

        public nuint HeapSegmentReserve;

        public nuint HeapSegmentCommit;

        public nuint HeapDeCommitTotalFreeThreshold;

        public nuint HeapDeCommitFreeBlockThreshold;

        public uint NumberOfHeaps;

        public uint MaximumNumberOfHeaps;

        public void** ProcessHeaps;

        public void* GdiSharedHandleTable;

        public void* ProcessStarterHelper;

        public uint GdiDCAttributeList;

        public void* LoaderLock;

        public uint OSMajorVersion;

        public uint OSMinorVersion;

        public ushort OSBuildNumber;

        public ushort OSCSDVersion;

        public uint OSPlatformId;

        public uint ImageSubsystem;

        public uint ImageSubsystemMajorVersion;

        public uint ImageSubsystemMinorVersion;

        public nuint ImageProcessAffinityMask;

        public fixed uint GdiHandleBuffer[60];

        public void* PostProcessInitRoutine;

        public void* TlsExpansionBitmap;

        public fixed uint TlsExpansionBitmapBits[32];

        public uint SessionId;

        public ulong AppCompatFlags;

        public ulong AppCompatFlagsUser;

        public void* pShimData;

        public void* AppCompatInfo;

        public UNICODE_STRING CSDVersion;

        public void* ActivationContextData;

        public void* ProcessAssemblyStorageMap;

        public void* SystemDefaultActivationContextData;

        public void* SystemAssemblyStorageMap;

        public nuint MinimumStackCommit;

        public void* FlsCallback;

        public LIST_ENTRY FlsListHead;

        public void* FlsBitmap;

        public fixed uint FlsBitmapBits[4];

        public uint FlsHighIndex;

        public void* WerRegistrationData;

        public void* WerShipAssertPtr;

        public void* pContextData;

        public void* pImageHeaderHash;

        public uint TracingFlags;

        public ulong CsrServerReadOnlySharedMemoryBase;

        public uint TppWorkerpListLock;

        public LIST_ENTRY TppWorkerpList;

        // TODO: Add more fields so we can access NtGlobalFlag2.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING
    {
        public ushort Length;

        public ushort MaximumLength;

        public char* Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LIST_ENTRY
    {
        public LIST_ENTRY* Flink;

        public LIST_ENTRY* Blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_KERNEL_DEBUGGER_INFORMATION
    {
        public bool DebuggerEnabled;

        public bool DebuggerNotPresent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES
    {
        public uint Length;

        public nint RootDirectory;

        public UNICODE_STRING* ObjectName;

        public uint Attributes;

        public void* SecurityDescriptor;

        public void* SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_TYPE_INFORMATION
    {
        public UNICODE_STRING TypeName;

        public uint TotalNumberOfObjects;

        public uint TotalNumberOfHandles;

        public uint TotalPagedPoolUsage;

        public uint TotalNonPagedPoolUsage;

        public uint TotalNamePoolUsage;

        public uint TotalHandleTableUsage;

        public uint HighWaterNumberOfObjects;

        public uint HighWaterNumberOfHandles;

        public uint HighWaterPagedPoolUsage;

        public uint HighWaterNonPagedPoolUsage;

        public uint HighWaterNamePoolUsage;

        public uint HighWaterHandleTableUsage;

        public uint InvalidAttributes;

        public GENERIC_MAPPING GenericMapping;

        public uint ValidAccessMask;

        public bool SecurityRequired;

        public bool MaintainHandleCount;

        public byte TypeIndex;

        public sbyte Reserved;

        public uint PoolType;

        public uint DefaultPagedPoolCharge;

        public uint DefaultNonPagedPoolCharge;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GENERIC_MAPPING
    {
        public uint GenericRead;

        public uint GenericWrite;

        public uint GenericExecute;

        public uint GenericAll;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KUSER_SHARED_DATA
    {
        public uint TickCountLowDeprecated;

        public uint TickCountMultiplier;

        public KSYSTEM_TIME InterruptTime;

        public KSYSTEM_TIME SystemTime;

        public KSYSTEM_TIME TimeZoneBias;

        public ushort ImageNumberLow;

        public ushort ImageNumberHigh;

        public fixed char NtSystemRoot[260];

        public uint MaxStackTraceDepth;

        public uint CryptoExponent;

        public uint TimeZoneId;

        public uint LargePageMinimum;

        public uint AitSamplingValue;

        public uint AppCompatFlag;

        public ulong RNGSeedVersion;

        public uint GlobalValidationRunlevel;

        public int TimeZoneBiasStamp;

        public uint NtBuildNumber;

        public int NtProductType;

        public bool ProductTypeIsValid;

        public bool Reserved0;

        public ushort NativeProcessorArchitecture;

        public uint NtMajorVersion;

        public uint NtMinorVersion;

        public fixed bool ProcessorFeatures[64];

        public uint Reserved1;

        public uint Reserved3;

        public uint TimeSlip;

        public int AlternativeArchitecture;

        public uint BootId;

        public long SystemExpirationDate;

        public ulong SuiteMask;

        public byte KdDebuggerEnabled;

        // TODO: Add more known fields beyond this point.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KSYSTEM_TIME
    {
        public uint LowPart;

        public int High1Time;

        public int High2Time;
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

    public const int STATUS_SUCCESS = 0x00000000;

    public const int STATUS_DEBUGGER_INACTIVE = unchecked((int)0xc0000354);

    public static readonly KUSER_SHARED_DATA* UserSharedData = (KUSER_SHARED_DATA*)0x7ffe0000;

    public static void InitializeObjectAttributes(OBJECT_ATTRIBUTES* p, UNICODE_STRING* n, uint a, IntPtr r, void* s)
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

    [LibraryImport(NtDll, EntryPoint = "RtlGetCurrentPeb")]
    public static partial PEB* RtlGetCurrentPeb();

    [LibraryImport(NtDll, EntryPoint = "RtlAdjustPrivilege")]
    public static partial int RtlAdjustPrivilege(
        uint privilege,
        [MarshalAs(UnmanagedType.U1)] bool enable,
        [MarshalAs(UnmanagedType.U1)] bool currentThread,
        [MarshalAs(UnmanagedType.U1)] out bool enabled);
}
