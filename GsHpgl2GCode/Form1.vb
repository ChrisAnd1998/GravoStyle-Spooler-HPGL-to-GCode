Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        OpenFileDialog1.Filter = "GravoStyle HPGL Spooler files (*.spl)|*.spl|All files (*.*)|*.*"

        Dim result As DialogResult = OpenFileDialog1.ShowDialog()

        If result = Windows.Forms.DialogResult.OK Then
            Dim path As String = OpenFileDialog1.FileName
            Dim text As String = File.ReadAllText(path)
            RichTextBox1.Text = text.ToString
            ProgressBar1.Value = 0
            ProgressBar2.Value = 0
            RichTextBox2.Text = Nothing
            RichTextBox3.Text = Nothing
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Convert()
    End Sub

    Sub Convert()
        Button3.Enabled = False
        ProgressBar1.Value = 0
        ProgressBar2.Value = 0

        RichTextBox2.Text = Nothing
        RichTextBox3.Text = Nothing

        RichTextBox2.AppendText("G0 Z0" & vbNewLine)

        For Each line In RichTextBox1.Lines

            If line.Contains("PU") AndAlso line.Contains(",") Then
                RichTextBox2.AppendText(line.Replace("PU", "G0 X").Replace(",", " Y").Replace(";", "") & vbNewLine)
            End If

            If line.Contains("PD") AndAlso line.Contains(",") Then
                RichTextBox2.AppendText(line.Replace("PD", "G0 X").Replace(",", " Y").Replace(";", "") & vbNewLine)
            End If

            If line.Contains("PU;") Then
                RichTextBox2.AppendText("G0 Z0" & vbNewLine)
            End If

            If line.Contains("PD;") Then
                RichTextBox2.AppendText("G0 Z" & TextBox1.Text & vbNewLine)
            End If

            ProgressBar1.Value = ProgressBar1.Value + 1

        Next

        System.Threading.Thread.Sleep(500) : Application.DoEvents()
        For i As Integer = 0 To RichTextBox2.Lines.Count - 1

            If RichTextBox2.Lines(i).Contains("X") Then
                Dim strRegex2 As String = "\d+"
                Dim myRegex2 As New Regex(strRegex2, RegexOptions.None)

                Dim matches As New ArrayList

                For Each myMatchx As Match In myRegex2.Matches(RichTextBox2.Lines(i).ToString)

                    If myMatchx.Success Then
                        If Not myMatchx.Value.ToString = "0" Then
                            matches.Add(myMatchx.Value.ToString)
                        End If
                    End If
                Next

                RichTextBox3.AppendText(RichTextBox2.Lines(i).Replace(matches(1).ToString, (matches(1).ToString / 40).ToString.Replace(",", ".")).Replace(matches(0).ToString, (matches(0).ToString / 40)).Replace(",", ".").ToString & vbNewLine)
            Else
                RichTextBox3.AppendText(RichTextBox2.Lines(i).ToString & vbNewLine)
            End If
            ProgressBar2.Value = i
        Next

        Button3.Enabled = True
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveFileDialog1.Filter = "Gcode Files (*.gcode*)|*.gcode"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
         Then
            My.Computer.FileSystem.WriteAllText _
            (SaveFileDialog1.FileName, RichTextBox3.Text, True)
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Shown
        For Each arg As String In Environment.GetCommandLineArgs()
            If arg.ToString.Contains(".SPL") Then
                Dim text As String = File.ReadAllText(arg)
                RichTextBox1.Text = text.ToString
                ProgressBar1.Value = 0
                ProgressBar2.Value = 0
                RichTextBox2.Text = Nothing
                RichTextBox3.Text = Nothing
                Convert()
            End If
        Next arg
    End Sub

    Private Sub FileSystemWatcher1_Created(sender As Object, e As FileSystemEventArgs) Handles FileSystemWatcher1.Created
        RichTextBox1.Text = Nothing

        ProgressBar1.Value = 0
        ProgressBar2.Value = 0
        RichTextBox2.Text = Nothing
        RichTextBox3.Text = Nothing

        System.Threading.Thread.Sleep(500) : Application.DoEvents()

        If e.FullPath.Contains(".SPL") Then
Read_Again:
            Try
                Dim text As String = File.ReadAllText(e.FullPath)
                RichTextBox1.Text = text.ToString
            Catch
                GoTo Read_Again
            End Try

        End If

        System.Threading.Thread.Sleep(500) : Application.DoEvents()

        Try
            My.Computer.FileSystem.DeleteFile(e.FullPath)
        Catch

        End Try

        Convert()

    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged
        ProgressBar1.Maximum = RichTextBox1.Lines.Count
    End Sub

    Private Sub RichTextBox2_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox2.TextChanged

        ProgressBar2.Maximum = RichTextBox2.Lines.Count
    End Sub

End Class