Imports System.Net
Imports System.Net.Sockets
Imports System.IO

Public Class Form1

    Public fps As Integer = 100
    Public render As Integer = 1
    Public tick As Integer = 0

    Public width_ As Integer
    Public height_ As Integer
    Public grid As Integer = 5

    Public this As Player
    Public players As New List(Of Player)
    Public dead_players As New List(Of DeadPlayer)
    Public bullets As New List(Of Bullet)
    Public dead_bullets As New List(Of DeadBullet)

    Public grid_pen As New Pen(Color.FromArgb(195, 195, 195), 1)
    Public background As Color = Color.FromArgb(205, 205, 205)
    Public blue_player_brush As New SolidBrush(Color.FromArgb(0, 175, 225))
    Public blue_player_pen As New Pen(Color.FromArgb(0, 135, 165), 3)
    Public red_player_brush As New SolidBrush(Color.FromArgb(240, 75, 85))
    Public red_player_pen As New Pen(Color.FromArgb(205, 65, 70), 3)
    Public cannon_brush As New SolidBrush(Color.FromArgb(153, 153, 153))
    Public cannon_pen As New Pen(Color.FromArgb(114, 114, 114), 3) With {.LineJoin = Drawing2D.LineJoin.Round}
    Public border_pen As New Pen(grid_pen.Color, 3)
    Public gui_brush As New SolidBrush(Color.FromArgb(25, Color.Black))
    Public minimap As Rectangle

    Public connection As Socket
    Public server As String
    Public stream As NetworkStream
    Public reader As StreamReader
    Public writer As StreamWriter

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        server = InputBox("Which server would you like to connect to?", "Connect", "127.0.0.1")
        Try
            connection = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            connection.Connect(New IPEndPoint(IPAddress.Parse(server), 35656))
            stream = New NetworkStream(connection)
            reader = New StreamReader(stream)
            writer = New StreamWriter(stream)
            If reader.ReadLine = "online" Then
                Reply("wrd", writer)
            Else
                Close()
            End If
        Catch ex As Exception
            Close()
        End Try

        width_ = canvas.Width
        height_ = canvas.Height
        minimap = New Rectangle(width_ - 70, height_ - 70, 70, 70)
        canvas.Image = New Bitmap(width_, height_)
        game.Interval = 1000 / fps
        game.Start()
    End Sub

    Public Sub Reply(s As String, n As StreamWriter)
        Try
            n.WriteLine(s)
            n.Flush()
        Catch ex As Exception
            Me.Close()
        End Try
    End Sub

    Private Sub game_Tick(sender As Object, e As EventArgs) Handles game.Tick
        Try
            Dim g As Graphics = Graphics.FromImage(canvas.Image)
            Dim cc As New Point(width_ / 2, height_ / 2)
            Dim mp As New Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y)
            Dim pt As Single = Math.Atan2(mp.Y - cc.Y, mp.X - cc.X) / (Math.PI / 180)
            Dim world_size As Integer = 4000
            Dim base_size As Integer = 350
            Dim this As Player = New Player(0, 0, 0, 0, 0)
            Dim this_ As Player = New Player(0, 0, 0, 0, 0)
            players.Clear()
            dead_players.Clear()
            bullets.Clear()
            dead_bullets.Clear()
            Reply("wrd", writer)

            Dim world As String = reader.ReadLine()
            Dim objects As List(Of String) = world.Split("|").ToList

            For Each o As String In objects
                If o = "" Then Continue For
                Dim pp As New Dictionary(Of String, String)
                Dim cont As String = o.Remove(0, 3).Remove(o.Length - 4, 1)
                For Each x As String In cont.Split(",")
                    pp.Add(x.Split("=")(0), x.Split("=")(1))
                Next
                If o.StartsWith("pb") Then
                    this = New Player(pp("x"), pp("y"), 0, 0, pp("t"))
                    If pp("a") = "0" Then
                        Me.Close()
                    End If
                    world_size = pp("w")
                    base_size = pp("bs")
                ElseIf o.StartsWith("pt") Then
                    this_ = New Player(pp("x"), pp("y"), pp("s"), pp("d"), pp("t"))
                    players.Add(this_)
                ElseIf o.StartsWith("pp") Then
                    players.Add(New Player(pp("x"), pp("y"), pp("s"), pp("d"), pp("t")))
                ElseIf o.StartsWith("pd") Then
                    dead_players.Add(New DeadPlayer(pp("x"), pp("y"), pp("s"), pp("d"), pp("t"), pp("a")))
                ElseIf o.StartsWith("bb") Then
                    bullets.Add(New Bullet(pp("x"), pp("y"), pp("s"), pp("t")))
                ElseIf o.StartsWith("bd") Then
                    dead_bullets.Add(New DeadBullet(pp("x"), pp("y"), pp("s"), pp("t"), pp("a")))
                End If
            Next

            Dim px As Single = Math.Floor(this.X - width_ / 2)
            Dim py As Single = Math.Floor(this.Y - height_ / 2)
            Dim pos As New Point(px, py)
            g.Clear(background)

            g.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            If IsDivisible(tick, render) Then
                For i = 0 - (pos.Y / grid) Mod grid To (height_ / grid) - (pos.Y / grid) Mod grid Step grid
                    g.DrawLine(grid_pen, New Point(0, i * grid), New Point(width_, i * grid))
                Next
                For i = 0 - (pos.X / grid) Mod grid To (width_ / grid) - (pos.X / grid) Mod grid Step grid
                    g.DrawLine(grid_pen, New Point(i * grid, 0), New Point(i * grid, height_))
                Next
                g.DrawRectangle(border_pen, CSng(0 - world_size / 2 - pos.X), CSng(0 - world_size / 2 - pos.Y), world_size, world_size)
                g.FillRectangle(New SolidBrush(Color.FromArgb(15, blue_player_brush.Color)), CSng(0 - world_size / 2 - pos.X), CSng(0 - world_size / 2 - pos.Y), base_size, world_size)
                g.FillRectangle(New SolidBrush(Color.FromArgb(15, red_player_brush.Color)), CSng(world_size / 2 - pos.X - base_size), CSng(0 - world_size / 2 - pos.Y), base_size, world_size)
            End If

            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            For Each b As DeadBullet In dead_bullets
                Dim prect As New Rectangle(b.X - pos.X - b.Size / 2, b.Y - pos.Y - b.Size / 2, b.Size, b.Size)
                If IsDivisible(tick, render) Then
                    If b.Team = 0 Then
                        g.FillEllipse(New SolidBrush(Color.FromArgb(b.Alpha, blue_player_brush.Color)), prect)
                        g.DrawEllipse(New Pen(Color.FromArgb(b.Alpha, blue_player_pen.Color), blue_player_pen.Width), prect)
                    Else
                        g.FillEllipse(New SolidBrush(Color.FromArgb(b.Alpha, red_player_brush.Color)), prect)
                        g.DrawEllipse(New Pen(Color.FromArgb(b.Alpha, red_player_pen.Color), blue_player_pen.Width), prect)
                    End If
                End If
            Next
            For Each b As Bullet In bullets
                Dim prect As New Rectangle(b.X - pos.X - b.Size / 2, b.Y - pos.Y - b.Size / 2, b.Size, b.Size)
                If IsDivisible(tick, render) Then
                    If b.Team = 0 Then
                        g.FillEllipse(blue_player_brush, prect)
                        g.DrawEllipse(blue_player_pen, prect)
                    Else
                        g.FillEllipse(red_player_brush, prect)
                        g.DrawEllipse(red_player_pen, prect)
                    End If
                End If
            Next
            For Each p As DeadPlayer In dead_players
                Dim prect As New Rectangle(Math.Ceiling(p.X - pos.X - p.Size / 2), Math.Ceiling(p.Y - pos.Y - p.Size / 2), p.Size, p.Size)
                If IsDivisible(tick, render) Then
                    Dim cannon_point As New Point(prect.Location.X + p.Size / 2, prect.Location.Y + p.Size / 2)
                    Dim point_array As New List(Of Point)
                    point_array.Add(New Point(cannon_point.X, cannon_point.Y + (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X, cannon_point.Y - (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X + (p.Size / 4 * 2.8), cannon_point.Y - (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X + (p.Size / 4 * 2.8), cannon_point.Y + (p.Size / 2) / 2))
                    Dim rot_array As New List(Of Point)
                    For Each point_ As Point In point_array
                        rot_array.Add(RotatePoint(point_, cannon_point, p.Direction))
                    Next
                    g.FillPolygon(New SolidBrush(Color.FromArgb(p.Alpha, cannon_brush.Color)), rot_array.ToArray)
                    g.DrawPolygon(New Pen(Color.FromArgb(p.Alpha, cannon_pen.Color), cannon_pen.Width), rot_array.ToArray)
                    If p.Team = 0 Then
                        g.FillEllipse(New SolidBrush(Color.FromArgb(p.Alpha, blue_player_brush.Color)), prect)
                        g.DrawEllipse(New Pen(Color.FromArgb(p.Alpha, blue_player_pen.Color), blue_player_pen.Width), prect)
                    Else
                        g.FillEllipse(New SolidBrush(Color.FromArgb(p.Alpha, red_player_brush.Color)), prect)
                        g.DrawEllipse(New Pen(Color.FromArgb(p.Alpha, red_player_pen.Color), red_player_pen.Width), prect)
                    End If
                End If
            Next
            For Each p As Player In players
                Dim prect As New Rectangle(p.X - pos.X - p.Size / 2, p.Y - pos.Y - p.Size / 2, p.Size, p.Size)
                If this_ Is p Then prect.Location = New Point(cc.X - p.Size / 2, cc.Y - p.Size / 2)
                Dim hbox As New Rectangle(p.X - p.Size / 2, p.Y - p.Size / 2, p.Size, p.Size)
                If IsDivisible(tick, render) Then
                    Dim cannon_point As New Point(prect.Location.X + p.Size / 2, prect.Location.Y + p.Size / 2)
                    Dim point_array As New List(Of Point)
                    point_array.Add(New Point(cannon_point.X, cannon_point.Y + (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X, cannon_point.Y - (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X + (p.Size / 4 * 2.8), cannon_point.Y - (p.Size / 2) / 2))
                    point_array.Add(New Point(cannon_point.X + (p.Size / 4 * 2.8), cannon_point.Y + (p.Size / 2) / 2))
                    Dim rot_array As New List(Of Point)
                    For Each point_ As Point In point_array
                        rot_array.Add(RotatePoint(point_, cannon_point, p.Direction))
                    Next
                    g.FillPolygon(cannon_brush, rot_array.ToArray)
                    g.DrawPolygon(cannon_pen, rot_array.ToArray)
                    If p.Team = 0 Then
                        g.FillEllipse(blue_player_brush, prect)
                        g.DrawEllipse(blue_player_pen, prect)
                    Else
                        g.FillEllipse(red_player_brush, prect)
                        g.DrawEllipse(red_player_pen, prect)
                    End If
                End If
            Next
            g.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            g.FillRectangle(gui_brush, minimap)
            g.FillRectangle(New SolidBrush(Color.FromArgb(25, blue_player_brush.Color)), New Rectangle(minimap.Location, New Size((base_size / world_size) * minimap.Width, minimap.Height)))
            g.FillRectangle(New SolidBrush(Color.FromArgb(25, red_player_brush.Color)), New Rectangle(New Point((minimap.X + minimap.Width) - ((base_size / world_size) * minimap.Width), minimap.Y), New Size(base_size, minimap.Height)))
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            For Each p As Player In players
                If p.Team = 0 Then
                    g.FillEllipse(blue_player_brush, CSng(minimap.X + (((p.X + (world_size / 2)) / world_size) * (minimap.Width - 4))), CSng(minimap.Y + (((p.Y + (world_size / 2)) / world_size) * (minimap.Height - 4))), 3, 3)
                Else
                    g.FillEllipse(red_player_brush, CSng(minimap.X + (((p.X + (world_size / 2)) / world_size) * (minimap.Width - 4))), CSng(minimap.Y + (((p.Y + (world_size / 2)) / world_size) * (minimap.Height - 4))), 3, 3)
                End If
            Next
            For Each p As Bullet In bullets
                If p.Team = 0 Then
                    g.FillEllipse(blue_player_brush, CSng(minimap.X + (((p.X + (world_size / 2)) / world_size) * (minimap.Width - 4))), CSng(minimap.Y + (((p.Y + (world_size / 2)) / world_size) * (minimap.Height - 4))), 2, 2)
                Else
                    g.FillEllipse(red_player_brush, CSng(minimap.X + (((p.X + (world_size / 2)) / world_size) * (minimap.Width - 4))), CSng(minimap.Y + (((p.Y + (world_size / 2)) / world_size) * (minimap.Height - 4))), 2, 2)
                End If
            Next

            Reply("dir " & pt.ToString, writer)

            If IsDivisible(tick, render) Then canvas.Refresh()
            tick += 1
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Reply("dwn " & e.KeyCode.ToString, writer)
    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        Reply("kup " & e.KeyCode.ToString, writer)
    End Sub

    Public Shared Function RotatePoint(pointToRotate As Point, centerPoint As Point, angleInDegrees As Double) As Point
        Dim angleInRadians As Double = angleInDegrees * (Math.PI / 180)
        Dim cosTheta As Double = Math.Cos(angleInRadians)
        Dim sinTheta As Double = Math.Sin(angleInRadians)
        Return New Point(Math.Ceiling(cosTheta * (pointToRotate.X - centerPoint.X) - sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                         Math.Ceiling(sinTheta * (pointToRotate.X - centerPoint.X) + cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y))
    End Function

    Public Function IsDivisible(x As Integer, y As Integer) As Boolean
        Return (x Mod y) = 0
    End Function

    Private Sub Form1_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        If WindowState = FormWindowState.Minimized Then Exit Sub
        Try
            width_ = canvas.Width
            height_ = canvas.Height
            canvas.Image = New Bitmap(width_, height_)
            minimap = New Rectangle(width_ - 70, height_ - 70, 70, 70)
        Catch ex As Exception
            width_ = 630
            height_ = 420
            canvas.Image = New Bitmap(width_, height_)
            minimap = New Rectangle(width_ - 70, height_ - 70, 70, 70)
        End Try
    End Sub

    Private Sub canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles canvas.MouseDown
        Reply("sht", writer)
    End Sub

    Private Sub canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles canvas.MouseUp
        Reply("ces", writer)
    End Sub

    Class Player
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Direction As Single
        Public Team As Integer

        Public Sub New(x As Single, y As Single, s As Single, d As Single, t As Integer)
            Me.X = x
            Me.Y = y
            Size = s
            Direction = d
            Team = t
        End Sub
    End Class

    Class Bullet
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Team As Integer

        Public Sub New(x As Single, y As Single, s As Single, t As Integer)
            Me.X = x
            Me.Y = y
            Size = s
            Team = t
        End Sub
    End Class

    Class DeadBullet
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Team As Integer
        Public Alpha As Single

        Public Sub New(x As Single, y As Single, s As Single, t As Integer, a As Single)
            Me.X = x
            Me.Y = y
            Size = s
            Team = t
            Alpha = a
        End Sub
    End Class

    Class DeadPlayer
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Direction As Single
        Public Alpha As Single = 255
        Public Team As Integer

        Public Sub New(x As Single, y As Single, s As Single, d As Single, t As Integer, a As Single)
            Me.X = x
            Me.Y = y
            Size = s
            Direction = d
            Alpha = a
            Team = t
        End Sub
    End Class
End Class
