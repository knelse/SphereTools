using System.Diagnostics;
using System.Runtime.InteropServices;

public static class FileUtil
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RM_UNIQUE_PROCESS
    {
        public readonly int dwProcessId;
        private readonly System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private const int RmRebootReasonNone = 0;
    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RM_PROCESS_INFO
    {
        public readonly RM_UNIQUE_PROCESS Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
        private readonly string strAppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
        private readonly string strServiceShortName;

        private readonly uint AppStatus;
        private readonly uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)] private readonly bool bRestartable;
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(uint pSessionHandle,
        uint nFiles,
        string[] rgsFilenames,
        uint nApplications,
        [In] RM_UNIQUE_PROCESS[] rgApplications,
        uint nServices,
        string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint pSessionHandle);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(uint dwSessionHandle,
        out uint pnProcInfoNeeded,
        ref uint pnProcInfo,
        [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
        ref uint lpdwRebootReasons);

    public static List<Process> WhoIsLocking(string path)
    {
        var key = Guid.NewGuid().ToString();
        var processes = new List<Process>();

        var res = RmStartSession(out var handle, 0, key);

        if (res != 0)
            throw new Exception("Could not begin restart session.  Unable to determine file locker.");

        try
        {
            const int ERROR_MORE_DATA = 234;
            uint pnProcInfo = 0,
                lpdwRebootReasons = RmRebootReasonNone;

            var resources = new[] { path }; // Just checking on one resource.

            res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null!, 0, null!);

            if (res != 0)
                throw new Exception("Could not register resource.");

            //Note: there's a race condition here -- the first call to RmGetList() returns
            //      the total number of process. However, when we call RmGetList() again to get
            //      the actual processes this number may have increased.
            res = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null!, ref lpdwRebootReasons);

            if (res == ERROR_MORE_DATA)
            {
                // Create an array to store the process results
                var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                // Get the list
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);

                if (res == 0)
                {
                    processes = new List<Process>((int)pnProcInfo);

                    // Enumerate all of the results and add them to the 
                    // list to be returned
                    for (var i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                        }
                        // catch the error -- in case the process is no longer running
                        catch (ArgumentException)
                        {
                        }
                    }
                }
                else
                    throw new Exception("Could not list processes locking resource.");
            }
            else if (res != 0)
                throw new Exception("Could not list processes locking resource. Failed to get size of result.");
        }
        finally
        {
            RmEndSession(handle);
        }

        return processes;
    }

}

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: getfilelocks <file_path>");
            Environment.Exit(1);
        }
        
        var processes = FileUtil.WhoIsLocking(args[0]);
        foreach (var process in processes)
        {
            Console.WriteLine(process.Id + " -> " + process.ProcessName);
        }
    }
}