param(
    [Parameter(Mandatory = $true)]
    [string]$Account
)

# Windows only grants "Log on as a service" automatically when you configure a service
# through the Services MMC console. Services created programmatically (like this MSI's
# ServiceInstall) don't get that right for free, so a custom account fails to log on the
# first time. This script grants SeServiceLogonRight to the account before the service
# is started.

$ErrorActionPreference = 'Stop'

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class LsaRights
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public int Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
    private static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int AccessMask, out IntPtr PolicyHandle);

    [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
    private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, byte[] AccountSid, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

    [DllImport("advapi32.dll")]
    private static extern int LsaClose(IntPtr ObjectHandle);

    [DllImport("advapi32.dll")]
    private static extern int LsaNtStatusToWinError(uint status);

    public static void AddPrivilege(byte[] sid, string privilege)
    {
        LSA_OBJECT_ATTRIBUTES oa = new LSA_OBJECT_ATTRIBUTES();
        LSA_UNICODE_STRING system = new LSA_UNICODE_STRING();
        IntPtr policyHandle;

        // POLICY_CREATE_ACCOUNT | POLICY_LOOKUP_NAMES
        uint status = LsaOpenPolicy(ref system, ref oa, 0x00000800 | 0x00000010, out policyHandle);
        if (status != 0)
            throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(status));

        LSA_UNICODE_STRING[] rights = new LSA_UNICODE_STRING[1];
        rights[0] = new LSA_UNICODE_STRING();
        rights[0].Buffer = Marshal.StringToHGlobalUni(privilege);
        rights[0].Length = (ushort)(privilege.Length * 2);
        rights[0].MaximumLength = (ushort)((privilege.Length + 1) * 2);

        status = LsaAddAccountRights(policyHandle, sid, rights, 1);

        Marshal.FreeHGlobal(rights[0].Buffer);
        LsaClose(policyHandle);

        if (status != 0)
            throw new System.ComponentModel.Win32Exception(LsaNtStatusToWinError(status));
    }
}
"@

$ntAccount = New-Object System.Security.Principal.NTAccount($Account)
$sidObj = $ntAccount.Translate([System.Security.Principal.SecurityIdentifier])
$sidBytes = New-Object byte[] ($sidObj.BinaryLength)
$sidObj.GetBinaryForm($sidBytes, 0)

[LsaRights]::AddPrivilege($sidBytes, "SeServiceLogonRight")

Write-Output "Granted 'Log on as a service' to $Account"
