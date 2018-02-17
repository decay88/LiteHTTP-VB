Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Security.Cryptography

Namespace LiteHTTP.Classes

    Class Communication

        Public Shared Function makeRequest(ByVal url As String, ByVal parameters As String) As String
            Try
                Dim result As String = Nothing
                Dim param As Byte() = Encoding.UTF8.GetBytes(parameters)
                Dim req As HttpWebRequest = WebRequest.Create(url)
                req.Method = "POST"
                req.UserAgent = "E9BC3BD76216AFA560BFB5ACAF5731A3"
                req.ContentType = "application/x-www-form-urlencoded"
                req.ContentLength = param.Length
                Dim st As Stream = req.GetRequestStream()
                st.Write(param, 0, param.Length)
                st.Close()
                st.Dispose()
                Dim resp As WebResponse = req.GetResponse()
                Dim sr As StreamReader = New StreamReader(resp.GetResponseStream())
                result = sr.ReadToEnd()
                sr.Close()
                sr.Dispose()
                resp.Close()
                Return result
            Catch
                Return "rqf"
            End Try
        End Function

        Public Shared Function encrypt(ByVal input As String) As String
            Try
                Dim key As String = Settings.edkey
                Dim rj As RijndaelManaged = New RijndaelManaged()
                rj.Padding = PaddingMode.Zeros
                rj.Mode = CipherMode.CBC
                rj.KeySize = 256
                rj.BlockSize = 256
                Dim ky As Byte() = Encoding.ASCII.GetBytes(key)
                Dim inp As Byte() = Encoding.ASCII.GetBytes(input)
                Dim res As Byte()
                Dim enc As ICryptoTransform = rj.CreateEncryptor(ky, ky)
                Using ms As MemoryStream = New MemoryStream()
                    Using cs As CryptoStream = New CryptoStream(ms, enc, CryptoStreamMode.Write)
                        cs.Write(inp, 0, inp.Length)
                        cs.FlushFinalBlock()
                        cs.Close()
                        cs.Dispose()
                    End Using

                    res = ms.ToArray()
                    ms.Close()
                    ms.Dispose()
                End Using

                Return Convert.ToBase64String(res).Replace("+", "~")
            Catch
                Return Nothing
            End Try
        End Function

        Public Shared Function decrypt(ByVal input As String) As String
            Try
                Dim key As String = Settings.edkey
                Dim rj As RijndaelManaged = New RijndaelManaged()
                rj.Padding = PaddingMode.Zeros
                rj.Mode = CipherMode.CBC
                rj.KeySize = 256
                rj.BlockSize = 256
                Dim ky As Byte() = Encoding.ASCII.GetBytes(key)
                Dim inp As Byte() = Convert.FromBase64String(input)
                Dim res As Byte() = New Byte(inp.Length - 1) {}
                Dim dec As ICryptoTransform = rj.CreateDecryptor(ky, ky)
                Using ms As MemoryStream = New MemoryStream(inp)
                    Using cs As CryptoStream = New CryptoStream(ms, dec, CryptoStreamMode.Read)
                        cs.Read(res, 0, res.Length)
                        cs.Close()
                        cs.Dispose()
                    End Using

                    ms.Close()
                    ms.Dispose()
                End Using

                Return Encoding.UTF8.GetString(res).Trim().Replace(vbNullChar, "")
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function
    End Class
End Namespace

