Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form1
    Dim xold As Integer
    Dim yold As Integer

    Dim x1 As Integer
    Dim y1 As Integer
    Dim x2 As Integer
    Dim y2 As Integer

    Dim blackpen As New Drawing.Pen(Color.Yellow)

    Dim counts1x As String()
    Dim counts1y As String()

    Sub draw()
        Dim g As Graphics
        blackpen.Color = Color.Cyan
        g = PictureBox1.CreateGraphics

        g.DrawLine(blackpen, x1, y1, x2, y2)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        OpenFileDialog1.Filter = "GravoStyle HPGL Spooler files (*.spl)|*.spl|All files (*.*)|*.*"

        Dim result As DialogResult = OpenFileDialog1.ShowDialog()

        If result = Windows.Forms.DialogResult.OK Then
            Dim path As String = OpenFileDialog1.FileName
            Dim text As String = File.ReadAllText(path)
            Dim first As String = System.IO.File.ReadAllLines(path)(0)
            If first.ToString.Contains("IN;PA;") Then
                RichTextBox1.Text = text.ToString
                ProgressBar1.Value = 0
                ProgressBar2.Value = 0
                RichTextBox2.Text = Nothing
                RichTextBox3.Text = Nothing
                PictureBox1.Image = Nothing
                Convert()
            End If

        End If
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

        Dim bmp As Bitmap = New Drawing.Bitmap(PictureBox1.Width, PictureBox1.Height)

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

                Dim newstr = RichTextBox2.Lines(i).Replace(matches(1).ToString, (matches(1).ToString / 40).ToString.Replace(",", ".")).Replace(matches(0).ToString, (matches(0).ToString / 40)).Replace(",", ".").ToString

                RichTextBox3.AppendText(newstr & vbNewLine)

                Dim lineclean As String = newstr.Replace("G0 X", "").Replace("Y", "")
                Dim counts As String() = lineclean.Split(New Char() {" "c})

                counts1x = counts(0).Split(New Char() {"."c})
                counts1y = counts(1).Split(New Char() {"."c})

                x1 = xold
                y1 = yold
                x2 = counts1x(0)
                y2 = counts1y(0)

                xold = x2
                yold = y2

                draw()
            Else
                RichTextBox3.AppendText(RichTextBox2.Lines(i).ToString & vbNewLine)
            End If
            ProgressBar2.Value = i
        Next

        ' PictureBox1.Image.Save("myimage.jpg", Drawing.Imaging.ImageFormat.Jpeg)

        Dim s As Size = PictureBox1.Size
        Dim memoryImage = New Bitmap(s.Width, s.Height)
        Dim memoryGraphics As Graphics = Graphics.FromImage(memoryImage)
        Dim ScreenPos As Point = Me.PictureBox1.PointToScreen(New Point(0, 0))
        memoryGraphics.CopyFromScreen(ScreenPos.X, ScreenPos.Y, 0, 0, s)
        PictureBox1.Image = memoryImage

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

                Dim first As String = System.IO.File.ReadAllLines(arg)(0)
                If first.ToString.Contains("IN;PA;") Then
                    RichTextBox1.Text = text.ToString
                    ProgressBar1.Value = 0
                    ProgressBar2.Value = 0
                    RichTextBox2.Text = Nothing
                    RichTextBox3.Text = Nothing
                    PictureBox1.Image = Nothing
                    Convert()
                End If

            End If
        Next arg
    End Sub

    Private Sub FileSystemWatcher1_Created(sender As Object, e As FileSystemEventArgs) Handles FileSystemWatcher1.Created
        RichTextBox1.Text = Nothing

        ProgressBar1.Value = 0
        ProgressBar2.Value = 0
        RichTextBox2.Text = Nothing
        RichTextBox3.Text = Nothing
        PictureBox1.Image = Nothing

        System.Threading.Thread.Sleep(500) : Application.DoEvents()
        Try

            Dim first As String = System.IO.File.ReadAllLines(e.FullPath)(0)

            If e.FullPath.Contains(".SPL") Then
Read_Again:
                Try
                    Dim text As String = File.ReadAllText(e.FullPath)

                    If first.ToString.Contains("IN;PA;") Then
                        RichTextBox1.Text = text.ToString
                    End If
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
        Catch

        End Try

    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged
        ProgressBar1.Maximum = RichTextBox1.Lines.Count
    End Sub

    Private Sub RichTextBox2_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox2.TextChanged

        ProgressBar2.Maximum = RichTextBox2.Lines.Count
    End Sub

End Class