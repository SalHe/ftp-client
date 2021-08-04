// Decompiled with JetBrains decompiler
// Type: System.Net.FtpStatusCode
// Assembly: System.Net.Requests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 3B8E85B0-0B32-4B8F-AF28-0FB1C8A75131
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.7\System.Net.Requests.dll

namespace FTPClient.WhyFTP
{
    public enum FTPStatusCode
    {
        Undefined = 0,
        RestartMarker = 110, // 0x0000006E
        ServiceTemporarilyNotAvailable = 120, // 0x00000078
        DataAlreadyOpen = 125, // 0x0000007D
        OpeningData = 150, // 0x00000096
        CommandOK = 200, // 0x000000C8
        CommandExtraneous = 202, // 0x000000CA
        DirectoryStatus = 212, // 0x000000D4
        FileStatus = 213, // 0x000000D5
        SystemType = 215, // 0x000000D7
        SendUserCommand = 220, // 0x000000DC
        ClosingControl = 221, // 0x000000DD
        ClosingData = 226, // 0x000000E2
        EnteringPassive = 227, // 0x000000E3
        LoggedInProceed = 230, // 0x000000E6
        ServerWantsSecureSession = 234, // 0x000000EA
        FileActionOK = 250, // 0x000000FA
        PathnameCreated = 257, // 0x00000101
        SendPasswordCommand = 331, // 0x0000014B
        NeedLoginAccount = 332, // 0x0000014C
        FileCommandPending = 350, // 0x0000015E
        ServiceNotAvailable = 421, // 0x000001A5
        CantOpenData = 425, // 0x000001A9
        ConnectionClosed = 426, // 0x000001AA
        ActionNotTakenFileUnavailableOrBusy = 450, // 0x000001C2
        ActionAbortedLocalProcessingError = 451, // 0x000001C3
        ActionNotTakenInsufficientSpace = 452, // 0x000001C4
        CommandSyntaxError = 500, // 0x000001F4
        ArgumentSyntaxError = 501, // 0x000001F5
        CommandNotImplemented = 502, // 0x000001F6
        BadCommandSequence = 503, // 0x000001F7
        NotLoggedIn = 530, // 0x00000212
        AccountNeeded = 532, // 0x00000214
        ActionNotTakenFileUnavailable = 550, // 0x00000226
        ActionAbortedUnknownPageType = 551, // 0x00000227
        FileActionAborted = 552, // 0x00000228
        ActionNotTakenFilenameNotAllowed = 553, // 0x00000229
    }
}