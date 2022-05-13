Imports System.Net.Mail
Imports System.Net.Sockets


Public Class Form1
    Dim tcpClient As TcpClient
    Dim lostCounter As Integer = 0

    'prevent altering the dropdown menu
    Private Sub ComboBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ComboBox1.KeyPress
        e.Handled = True
    End Sub

    'test mail button
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        sendMail()
    End Sub

    'send mail routine
    Private Sub sendMail()
        'check for required fields
        If TextBox1.Text.Length = 0 Or TextBox4.Text.Length = 0 Or TextBox5.Text.Length = 0 Or TextBox6.Text.Length = 0 Or TextBox7.Text.Length = 0 Then
            MessageBox.Show("All fields are mandatory (+ your email)!")
            Return
        End If



        Dim Mail As New MailMessage
        Dim SMTP As New SmtpClient(TextBox4.Text)

        Mail.Subject = TextBox2.Text
        Mail.From = New MailAddress(TextBox1.Text)
        SMTP.Credentials = New System.Net.NetworkCredential(TextBox5.Text, TextBox6.Text)

        Mail.To.Add(TextBox1.Text)
        Mail.IsBodyHtml = True
        Mail.Body = TextBox3.Text

        'enable ssl option
        If ComboBox1.SelectedItem = "SSL/TLS" Then
            SMTP.EnableSsl = True
        Else
            SMTP.EnableSsl = False
        End If

        SMTP.Port = TextBox7.Text

        Try
            SMTP.Send(Mail)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    'enable watching
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'reset lost counter
        lostCounter = 0

        'check for required fields
        If TextBox1.Text.Length = 0 Or TextBox4.Text.Length = 0 Or TextBox5.Text.Length = 0 Or TextBox6.Text.Length = 0 Or TextBox7.Text.Length = 0 Then
            MessageBox.Show("Check entered email fields!")
            Return
        End If

        If Button1.Text = "Watch star" Then
            tcpClient = New TcpClient

            Try
                'default PHD2 server address and port
                tcpClient.Connect("127.0.0.1", 4400)
            Catch ex As Exception
                MessageBox.Show("PHD2 not running?")
                Return
            End Try


            'watch interval
            Timer1.Interval = 5000

            Timer1.Start()


            Button1.Text = "Stop watching"
            Label13.Text = "Waiting..."
        Else
            'close connection and stop timer
            tcpClient.Close()
            Timer1.Stop()
            Button1.Text = "Watch star"
            Label13.Text = "Idle"
        End If
    End Sub


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim networkStream As NetworkStream = tcpClient.GetStream()
        If networkStream.CanWrite And networkStream.CanRead Then

            ' Send appstate request
            Dim sendBytes As [Byte]() = System.Text.Encoding.ASCII.GetBytes("{""method"":""get_app_state"", ""id"": 1}" & vbCrLf)
            networkStream.Write(sendBytes, 0, sendBytes.Length)

            ' Read response
            Dim bytes(tcpClient.ReceiveBufferSize) As Byte
            Try
                networkStream.Read(bytes, 0, CInt(tcpClient.ReceiveBufferSize))
            Catch ex As Exception
                MessageBox.Show("PHD2 not running?")
                Button1_Click(Nothing, Nothing)
                Return
            End Try


            Dim response As String = System.Text.Encoding.ASCII.GetString(bytes)

            'check message if they contain contains...
            If response.IndexOf("StarLost") > -1 Then
                lostCounter += 1
                Label13.Text = "Star lost!!!"

                If lostCounter = 5 Then
                    lostCounter = 0
                    'stop watching
                    Button1_Click(Nothing, Nothing)

                    'send mail
                    sendMail()
                End If


            ElseIf response.IndexOf("GuideStep") > -1 Then
                Label13.Text = "Guding"
                lostCounter = 0
            Else
                lostCounter = 0
                Label13.Text = "Doing something else..."
            End If


        End If
    End Sub

    Private Sub TabControl1_Leave(sender As Object, e As EventArgs) Handles TabControl1.Leave
        My.Settings.Save()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label14.Text = "App version " & Application.ProductVersion
    End Sub
End Class
