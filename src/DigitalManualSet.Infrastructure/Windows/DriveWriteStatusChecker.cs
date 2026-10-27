using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using DigitalManualSet.Core.Models;

namespace DigitalManualSet.Infrastructure.Windows;

/// <summary>
/// Determines whether a drive supports write operations using Windows device I/O.
/// </summary>
internal static class DriveWriteStatusChecker
{
    /// <summary>
    /// Specifies read sharing for the native device handle.
    /// </summary>
    private const uint FileShareRead = 0x00000001;

    /// <summary>
    /// Specifies write sharing for the native device handle.
    /// </summary>
    private const uint FileShareWrite = 0x00000002;

    /// <summary>
    /// Specifies that an existing device should be opened.
    /// </summary>
    private const uint OpenExisting = 3;

    /// <summary>
    /// Identifies the Windows control code used to query whether a disk is writable.
    /// </summary>
    private const uint IoctlDiskIsWritable = 0x00070024;

    /// <summary>
    /// Identifies the Windows error returned for write-protected media.
    /// </summary>
    private const int ErrorWriteProtect = 19;

    /// <summary>
    /// Gets the write status of the drive at the specified root path.
    /// </summary>
    /// <param name="rootPath">The root path of the drive to check.</param>
    /// <returns>The detected write status of the drive.</returns>
    public static DriveWriteStatus GetWriteStatus(string rootPath)
    {
        var volumePath = $@"\\.\{rootPath.TrimEnd('\\')}";

        using var handle = CreateFile(
            volumePath,
            0,
            FileShareRead | FileShareWrite,
            IntPtr.Zero,
            OpenExisting,
            0,
            IntPtr.Zero);

        if (handle.IsInvalid)
        {
            return DriveWriteStatus.Unknown;
        }

        var result = DeviceIoControl(
            handle,
            IoctlDiskIsWritable,
            IntPtr.Zero,
            0,
            IntPtr.Zero,
            0,
            out _,
            IntPtr.Zero);

        if (result)
        {
            return DriveWriteStatus.Writable;
        }

        var error = Marshal.GetLastWin32Error();

        return error == ErrorWriteProtect
            ? DriveWriteStatus.WriteProtected
            : DriveWriteStatus.Unknown;
    }

    /// <summary>
    /// Opens a handle to an existing Windows device.
    /// </summary>
    /// <param name="lpFileName">The name of the device to open.</param>
    /// <param name="dwDesiredAccess">The requested access mode.</param>
    /// <param name="dwShareMode">The sharing mode for the device.</param>
    /// <param name="lpSecurityAttributes">Optional security attributes.</param>
    /// <param name="dwCreationDisposition">The action to take when the device exists or does not exist.</param>
    /// <param name="dwFlagsAndAttributes">The device flags and attributes.</param>
    /// <param name="hTemplateFile">An optional template file handle.</param>
    /// <returns>A safe handle for the opened device.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    /// <summary>
    /// Sends a control code to a Windows device.
    /// </summary>
    /// <param name="hDevice">The handle to the device.</param>
    /// <param name="dwIoControlCode">The control code to send.</param>
    /// <param name="lpInBuffer">The input buffer.</param>
    /// <param name="nInBufferSize">The size of the input buffer.</param>
    /// <param name="lpOutBuffer">The output buffer.</param>
    /// <param name="nOutBufferSize">The size of the output buffer.</param>
    /// <param name="lpBytesReturned">The number of bytes returned by the device.</param>
    /// <param name="lpOverlapped">Optional overlapped I/O data.</param>
    /// <returns><see langword="true"/> when the device operation succeeds; otherwise, <see langword="false"/>.</returns>
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);
}