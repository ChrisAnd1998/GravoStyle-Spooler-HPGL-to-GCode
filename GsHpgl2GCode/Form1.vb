Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CompilerServices

Public Class Form1

    <DllImport("kernel32.dll")>
    Private Shared Function SetProcessWorkingSetSize(ByVal hProcess As IntPtr, ByVal dwMinimumWorkingSetSize As Int32, ByVal dwMaximumWorkingSetSize As Int32) As Int32
    End Function

    Dim xold As Integer
    Dim yold As Integer

    Dim x1 As Integer
    Dim y1 As Integer
    Dim x2 As Integer
    Dim y2 As Integer

    Dim blackpen As New Drawing.Pen(Color.Yellow)

    Dim newc As Boolean

    Dim counts1x As String()
    Dim counts1y As String()

    Dim startup As Boolean

    Dim virtual3 As New ArrayList

    Sub draw()
        Dim g As Graphics

        If newc = True Then
            blackpen.Color = Color.Red

            newc = False
        Else

            blackpen.Color = Color.Cyan
        End If
        g = PictureBox1.CreateGraphics : Application.DoEvents()

        g.DrawLine(blackpen, x1, y1, x2, y2) : Application.DoEvents()
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

                RichTextBox3.Text = Nothing
                PictureBox1.Image = Nothing
                Button2.Enabled = False
                Button3.Enabled = False

                Convert()
            End If

        End If
    End Sub

    Sub Convert()
        Me.Show()
        Me.TopMost = True
        Button3.Enabled = False
        ProgressBar1.Value = 0
        ProgressBar2.Value = 0

        RichTextBox3.Text = Nothing

        Dim virtual2 As New ArrayList
        Dim virtual As New ArrayList

        For Each linev As String In RichTextBox1.Lines
            virtual.Add(linev)
        Next

        For Each line In virtual
            ' MessageBox.Show(line)
            If line.Contains("!PZ") AndAlso line.Contains(",") Then
                Dim array As String() = line.Split(New Char() {";"c})
                For Each text4 As String In array
                    If text4.Contains("!PZ") Then
                        Me.TextBox1.Text = (text4.Replace("!PZ 0,", "") / 100.0).ToString().Replace(",", ".")
                    End If
                Next
                Exit For
            End If

        Next

        For Each line In virtual
            ' MessageBox.Show(line)
            If line.Contains("!LB") Then
                Label3.Text = line.Replace("!LB", "").Replace(";", "")
                Exit For
            End If
        Next

        For i As Integer = 0 To virtual.Count - 1

            If virtual(i).Contains("PU") AndAlso virtual(i).Contains(",") Then
                '  MessageBox.Show(virtual(i))
                virtual2.Add(virtual(i).Replace("PU", "G1 X").Replace(",", " Y").Replace(";", "")) : Application.DoEvents()
            End If

            If virtual(i).Contains("PD") AndAlso virtual(i).Contains(",") Then
                virtual2.Add(virtual(i).Replace("PD", "G1 X").Replace(",", " Y").Replace(";", "")) : Application.DoEvents()
            End If

            If virtual(i).Contains("PU;") Then
                virtual2.Add("G1 Z0") : Application.DoEvents()
            End If

            If virtual(i).Contains("PD;") Then
                Dim valx As Double = Val(TextBox1.Text) + Val(TextBox2.Text)
                virtual2.Add("G1 Z" & valx.ToString.Replace(",", ".")) : Application.DoEvents()
            End If

            ProgressBar1.Value = i : Application.DoEvents()

        Next

        virtual3.Clear()
        virtual3.Add("G1 Z0")
        virtual3.AddRange(virtual2)

        System.Threading.Thread.Sleep(500) : Application.DoEvents()

        Button2.Enabled = True

        ProgressBar1.Value = ProgressBar1.Maximum

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveFileDialog1.Filter = "Gcode Files (*.gcode*)|*.gcode"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            My.Computer.FileSystem.WriteAllText _
              (SaveFileDialog1.FileName, RichTextBox3.Text, True)
        End If

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Shown
        If startup = True Then
            Me.Hide()
            startup = False
        End If

        For Each arg As String In Environment.GetCommandLineArgs()
            If arg.ToString.Contains(".SPL") Then
                Dim text As String = File.ReadAllText(arg)

                Dim first As String = System.IO.File.ReadAllLines(arg)(0)
                If first.ToString.Contains("IN;PA;") Then
                    RichTextBox1.Text = text.ToString
                    ProgressBar1.Value = 0
                    ProgressBar2.Value = 0

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
        Me.WindowState = WindowState.Normal
        Me.BringToFront()
    End Sub

    Private Sub RichTextBox2_TextChanged(sender As Object, e As EventArgs)
        ' ProgressBar2.Maximum = virtual3.Count

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        If TextBox1.Text = "" Then
            TextBox1.Text = 12
        End If

        If TextBox2.Text = "" Then
            TextBox2.Text = 0
        End If

        If TextBox3.Text = "" Then
            TextBox3.Text = 500
        End If

        If TextBox4.Text = "" Then
            TextBox4.Text = 500
        End If

        Dim bmp As Bitmap = New Drawing.Bitmap(PictureBox1.Width, PictureBox1.Height)

        RichTextBox3.Clear()
        PictureBox1.Image = Nothing
        ProgressBar2.Maximum = virtual3.Count

        Button1.Enabled = False
        Button2.Enabled = False
        TextBox1.Enabled = False
        TextBox2.Enabled = False

        xold = 0
        yold = 0
        ' Dim virtual As New ArrayList
        ' For Each linev As String In RichTextBox2.Lines
        ' virtual.Add(linev)
        ' Next

        For i As Integer = 0 To virtual3.Count - 1

            If virtual3(i).Contains("X") Then

                Dim line = virtual3(i).Split(New Char() {"Y"c}) : Application.DoEvents()

                Dim linex = line(0) : Application.DoEvents()
                Dim liney = "Y" & line(1) : Application.DoEvents()

                Dim numberx = line(0).ToString.Replace("G1 X", "") : Application.DoEvents()
                Dim numbery = line(1).ToString.Replace("Y", "") : Application.DoEvents()

                Dim oXpos = numberx : Application.DoEvents()
                Dim oYpos = numbery : Application.DoEvents()
                Dim Xpos = numberx / 40 : Application.DoEvents()
                Dim Ypos = 490 - (numbery / 40) : Application.DoEvents()

                Console.WriteLine(Math.Round(Ypos, 3))

                Dim newxstr = linex.Replace(oXpos, Math.Round(Xpos, 3)).Replace(",", ".") : Application.DoEvents()
                Dim newystr = liney.Replace(oYpos, Math.Round(Ypos, 3)).Replace(",", ".") : Application.DoEvents()

                Dim newstr = (newxstr & " " & newystr).ToString.Replace("G1X", "G1 X") : Application.DoEvents()

                RichTextBox3.AppendText(newstr & " F" & TextBox3.Text & vbNewLine) : Application.DoEvents()

                Dim lineclean As String = newstr.Replace("G1 X", "").Replace("Y", "") : Application.DoEvents()
                Dim counts As String() = lineclean.Split(New Char() {" "c}) : Application.DoEvents()

                counts1x = counts(0).Split(New Char() {"."c}) : Application.DoEvents()
                counts1y = counts(1).Split(New Char() {"."c}) : Application.DoEvents()

                x1 = xold : Application.DoEvents()
                y1 = yold : Application.DoEvents()
                x2 = counts1x(0) : Application.DoEvents()
                y2 = counts1y(0) : Application.DoEvents()

                xold = x2 : Application.DoEvents()
                yold = y2 : Application.DoEvents()

                PictureBox1.Focus() : Application.DoEvents()

                draw()

                '  RichTextBox3.SelectionStart = Len(RichTextBox3.Text) : Application.DoEvents()

                'RichTextBox3.ScrollToCaret() : Application.DoEvents()
            Else
                Console.WriteLine(virtual3(i).ToString)
                newc = True

                Dim valx As Double = Val(TextBox1.Text) + Val(TextBox2.Text)

                If Not virtual3(i) = "G1 Z0" Then
                    RichTextBox3.AppendText("G1 Z" & valx.ToString.Replace(",", ".") & " F" & TextBox4.Text & vbNewLine)
                Else
                    RichTextBox3.AppendText(virtual3(i).ToString & " F" & TextBox4.Text & vbNewLine)
                End If

            End If
            ProgressBar2.Value = i : Application.DoEvents()
            Dim percentage As String = CType((ProgressBar2.Value / ProgressBar2.Maximum * 100), Integer).ToString & "%" : Application.DoEvents()
            Label2.Text = percentage : Application.DoEvents()

        Next

        Dim s As Size = PictureBox1.Size
        Dim memoryImage = New Bitmap(s.Width, s.Height)
        Dim memoryGraphics As Graphics = Graphics.FromImage(memoryImage)
        Dim ScreenPos As Point = Me.PictureBox1.PointToScreen(New Point(0, 0))
        memoryGraphics.CopyFromScreen(ScreenPos.X, ScreenPos.Y, 0, 0, s)
        PictureBox1.Image = memoryImage

        ' PictureBox1.Image.Save("myimage.jpg", Drawing.Imaging.ImageFormat.Jpeg)

        ProgressBar2.Value = ProgressBar2.Maximum
        Label2.Text = "100%"

        Button3.Enabled = True

        Button1.Enabled = True
        Button2.Enabled = True
        TextBox1.Enabled = True
        TextBox2.Enabled = True

        ClearMemory()

    End Sub

    Public Shared Function ClearMemory() As Int32
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
        Return SetProcessWorkingSetSize(Diagnostics.Process.GetCurrentProcess.Handle, -1, -1)
    End Function

    Private Sub NotifyIcon1_Click(sender As Object, e As EventArgs) Handles NotifyIcon1.Click
        Me.Show()
    End Sub

    Private Sub Form1_Load_1(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            For Each prog As Process In Process.GetProcessesByName("GsHpgl2GCode")
                If Not prog.Id = Process.GetCurrentProcess.Id Then
                    prog.Kill()
                End If
            Next
        Catch
        End Try

        startup = True
    End Sub

    Protected Overrides Sub OnClosing(ByVal e As System.ComponentModel.CancelEventArgs)
        e.Cancel = True
        Me.Hide()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        NotifyIcon1.Visible = False
        Me.Close()
        End
    End Sub

End Class