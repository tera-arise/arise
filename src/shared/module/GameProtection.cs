using static Arise.Module.WindowsPInvoke;

namespace Arise.Module;

internal static unsafe class GameProtection
{
    [ModuleInitializer]
    [SuppressMessage("", "CA2255")]
    public static void Initialize()
    {
        var culture = CultureInfo.InvariantCulture;

        if (DateTime.UtcNow - DateTime.Parse(GetIssueTime(), culture) > TimeSpan.Parse(GetValidDuration(), culture))
            Terminate();

        StartThread(() =>
        {
            while (!Environment.HasShutdownStarted)
            {
                if (DetectDebugger())
                    Terminate();

                Thread.Sleep(GetCheckInterval());
            }
        });
    }

    private static void StartThread(ThreadStart body)
    {
        new Thread(body)
        {
            IsBackground = true,
        }.Start();
    }

    [SuppressMessage("", "CA5394")]
    private static void Terminate()
    {
        StartThread(() =>
        {
            var (lower, upper) = GetExitDelayRange();

            Thread.Sleep(Random.Shared.Next(lower, upper));

            _ = NtTerminateProcess(NtCurrentProcess, GetExitStatus());
        });
    }

    [Obfuscation]
    private static string GetIssueTime()
    {
        return "xyz";
    }

    [Obfuscation]
    private static string GetValidDuration()
    {
        return "xyz";
    }

    [Obfuscation]
    private static int GetCheckInterval()
    {
        return 42;
    }

    [Obfuscation]
    private static (int Lower, int Upper) GetExitDelayRange()
    {
        return (42, 42);
    }

    [Obfuscation]
    private static int GetExitStatus()
    {
        return 42;
    }

    private static bool DetectDebugger()
    {
        // TODO: Add more advanced checks. In particular, we should do a checksum of TERA's code segment after we apply
        // our own modifications and disable Themida. We should also test for common Win32 hooks used to defeat
        // anti-debugging techniques.
        //
        // https://github.com/LordNoteworthy/al-khaser
        // https://anti-debug.checkpoint.com

        return
            CheckA() ||
            CheckB() ||
            CheckC() ||
            CheckD() ||
            CheckE() ||
            CheckF() ||
            CheckG() ||
            CheckH() ||
            CheckI() ||
            CheckJ() ||
            CheckK() ||
            CheckL() ||
            CheckM();
    }

    private static bool CheckA()
    {
        return Debugger.IsAttached || Debugger.IsLogging();
    }

    private static bool CheckB()
    {
        nint port;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugPort, &port, (uint)sizeof(nint), out _), port) != (STATUS_SUCCESS, 0);
    }

    private static bool CheckC()
    {
        nint handle;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugObjectHandle, &handle, (uint)sizeof(nint), out _), handle) is
            (STATUS_SUCCESS, not 0);
    }

    private static bool CheckD()
    {
        uint inherit;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugFlags, &inherit, sizeof(uint), out _), inherit) is
            (not STATUS_SUCCESS, 0);
    }

    private static bool CheckE()
    {
        return RtlGetCurrentPeb()->BeingDebugged;
    }

    private static bool CheckF()
    {
        const uint flags =
            FLG_HEAP_ENABLE_TAIL_CHECK |
            FLG_HEAP_ENABLE_FREE_CHECK |
            FLG_HEAP_VALIDATE_PARAMETERS |
            FLG_HEAP_VALIDATE_ALL |
            FLG_APPLICATION_VERIFIER |
            FLG_MONITOR_SILENT_PROCESS_EXIT |
            FLG_USER_STACK_TRACE_DB |
            FLG_ENABLE_SYSTEM_CRIT_BREAKS |
            FLG_ENABLE_CLOSE_EXCEPTIONS |
            FLG_HEAP_PAGE_ALLOCS |
            FLG_STOP_ON_UNHANDLED_EXCEPTION |
            FLG_ENABLE_HANDLE_EXCEPTIONS;

        return (RtlGetCurrentPeb()->NtGlobalFlag & flags) != 0;
    }

    private static bool CheckG()
    {
        const uint flags = HeapTracingEnabled | CritSecTracingEnabled | LibLoaderTracingEnabled;

        return (RtlGetCurrentPeb()->TracingFlags & flags) != 0;
    }

    private static bool CheckH()
    {
        return (UserSharedData->KdDebuggerEnabled & 0b10) != 0;
    }

    private static bool CheckI()
    {
        SYSTEM_KERNEL_DEBUGGER_INFORMATION skdi;

        return NtQuerySystemInformation(
            SystemKernelDebuggerInformation,
            &skdi,
            (uint)sizeof(SYSTEM_KERNEL_DEBUGGER_INFORMATION),
            out _) != STATUS_SUCCESS ||
            skdi.DebuggerEnabled ||
            !skdi.DebuggerNotPresent;
    }

    private static bool CheckJ()
    {
        OBJECT_ATTRIBUTES oa;

        InitializeObjectAttributes(&oa, null, 0, 0, null);

        if (NtCreateDebugObject(out var dbg, DEBUG_ALL_ACCESS, &oa, 0) != STATUS_SUCCESS)
            return true;

        try
        {
            OBJECT_TYPE_INFORMATION oti;

            return (NtQueryObject(
                dbg,
                ObjectTypeInformation,
                &oti,
                (uint)sizeof(OBJECT_TYPE_INFORMATION),
                out _), oti.TotalNumberOfObjects) != (STATUS_SUCCESS, 1);
        }
        finally
        {
            _ = NtClose(dbg);
        }
    }

    private static bool CheckK()
    {
        _ = RtlAdjustPrivilege(SE_DEBUG_PRIVILEGE, false, false, out var enabled);

        return enabled;
    }

    private static bool CheckL()
    {
        return NtSystemDebugControl(SysDbgQuerySpecialCalls, null, 0, null, 0, out _) != STATUS_DEBUGGER_INACTIVE;
    }

    private static bool CheckM()
    {
        bool hide;

        return NtSetInformationThread(NtCurrentThread, ThreadHideFromDebugger, null, 0) != STATUS_SUCCESS &&
            (NtQueryInformationThread(NtCurrentThread, ThreadHideFromDebugger, &hide, sizeof(byte), out _), hide) !=
            (STATUS_SUCCESS, false);
    }
}
