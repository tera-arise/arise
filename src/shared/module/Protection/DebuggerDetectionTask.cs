using static Arise.Bridge.WindowsPInvoke;

namespace Arise.Bridge.Protection;

internal sealed unsafe class DebuggerDetectionTask : GameProtectionTask
{
    protected override bool Check()
    {
        // TODO: There are other checks we can do that might be partially redundant with these.
        return
            CheckManagedDebugger() ||
            CheckProcessDebugPort() ||
            CheckProcessDebugObjectHandle() ||
            CheckProcessDebugFlags() ||
            CheckPebBeingDebugged() ||
            CheckPebNtGlobalFlag() ||
            CheckPebTracingFlags() ||
            CheckUserSharedDataKdDebuggerEnabled() ||
            CheckSystemKernelDebuggerInfo() ||
            CheckNtCreateAndQueryDebugObject() ||
            CheckNtSystemDebugControl() ||
            CheckThreadHideFromDebugger();
    }

    private static bool CheckManagedDebugger()
    {
        return Debugger.IsAttached || Debugger.IsLogging();
    }

    private static bool CheckProcessDebugPort()
    {
        nint port;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugPort, &port, (uint)sizeof(nint), out _), port) != (STATUS_SUCCESS, 0);
    }

    private static bool CheckProcessDebugObjectHandle()
    {
        nint handle;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugObjectHandle, &handle, (uint)sizeof(nint), out _), handle) is
            (STATUS_SUCCESS, not 0);
    }

    private static bool CheckProcessDebugFlags()
    {
        uint inherit;

        return (NtQueryInformationProcess(
            NtCurrentProcess, ProcessDebugFlags, &inherit, sizeof(uint), out _), inherit) is
            (not STATUS_SUCCESS, 0);
    }

    private static bool CheckPebBeingDebugged()
    {
        return RtlGetCurrentPeb()->BeingDebugged;
    }

    private static bool CheckPebNtGlobalFlag()
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

    private static bool CheckPebTracingFlags()
    {
        const uint flags = HeapTracingEnabled | CritSecTracingEnabled | LibLoaderTracingEnabled;

        return (RtlGetCurrentPeb()->TracingFlags & flags) != 0;
    }

    private static bool CheckUserSharedDataKdDebuggerEnabled()
    {
        return (UserSharedData->KdDebuggerEnabled & 0b10) != 0;
    }

    private static bool CheckSystemKernelDebuggerInfo()
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

    private static bool CheckNtCreateAndQueryDebugObject()
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

    private static bool CheckNtSystemDebugControl()
    {
        return NtSystemDebugControl(SysDbgQuerySpecialCalls, null, 0, null, 0, out _) != STATUS_DEBUGGER_INACTIVE;
    }

    private static bool CheckThreadHideFromDebugger()
    {
        bool hide;

        return NtSetInformationThread(NtCurrentThread, ThreadHideFromDebugger, null, 0) != STATUS_SUCCESS &&
            (NtQueryInformationThread(NtCurrentThread, ThreadHideFromDebugger, &hide, sizeof(byte), out _), hide) !=
            (STATUS_SUCCESS, false);
    }
}
