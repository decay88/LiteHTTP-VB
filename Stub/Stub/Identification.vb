Namespace LiteHTTP.Classes

    Class Identification

        Public Shared Function getHardwareID() As String
            Dim tohash As String = identifier("Win32_Processor", "ProcessorId")
            tohash += "-" & identifier("Win32_BIOS", "SerialNumber")
            tohash += "-" & identifier("Win32_DiskDrive", "Signature")
            tohash += "-" & identifier("Win32_BaseBoard", "SerialNumber")
            tohash += "-" & identifier("Win32_VideoController", "Name")
            Return Misc.hash(tohash)
        End Function

        Private Shared Function identifier(ByVal wmiClass As String, ByVal wmiProperty As String) As String
            Dim result As String = ""
            Dim mc As Management.ManagementClass = New System.Management.ManagementClass(wmiClass)
            Dim moc As Management.ManagementObjectCollection = mc.GetInstances()
            For Each mo As Management.ManagementObject In moc
                If result = "" Then
                    Try
                        result = mo(wmiProperty).ToString()
                        Exit For
                    Catch
                    End Try
                End If
            Next

            Return result
        End Function

        Public Shared Function osName() As String
            Return (New Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName.Replace("Microsoft ", "") & " " + Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"))
        End Function
    End Class
End Namespace
