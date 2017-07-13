Imports System
Imports System.Net
Imports System.IO
Imports System.Net.Sockets
Imports System.Threading
Imports System.Drawing
Imports System.Windows.forms

Module Module1

    Public fps As Integer = 75
    Public tick As Integer = 0
    Public rand As New Random

    Public width_ As Integer
    Public height_ As Integer
    Public world_size As Single = 4000
    Public base_size As Single = 350
    Public ss_div As Single = 100
    Public kill_multiplier As Single = 1

    Public max_life As Integer = 100
    Public max_size As Integer = 100
    Public min_size As Integer = 60
    Public damage_on_hit As Integer = 5
    Public regen_per_tick As Integer = 1
    Public players As New List(Of Player)
    Public dead_players As New List(Of DeadPlayer)
    Public bullets As New List(Of Bullet)
    Public dead_bullets As New List(Of DeadBullet)
    Public shoot_tick As Integer = 10
    Public starting_score As Single = 100

    Public accel As Single = 0.25
    Public diss As Single = 25
    Public thrust As Single = 2
    Public max_vel As Single = 3
    Public bullet_speed As Single = 10
    Public collision As Single = 20
    Public knockback As Single = 10
    Public deflect As Single = 1

    Public listen_addr As IPAddress = IPAddress.Any
    Public port As UInt16 = 35656
    Public max_party As UInteger = 100
    Public time_out As Integer = 60000
    Public allow_multi As Boolean = True

    Public Sub Load()
        If File.Exists("server.ini") Then
            Dim settings As New Dictionary(Of String, String)
            For Each l As String In File.ReadAllLines("server.ini")
                If Not String.IsNullOrWhiteSpace(l) And Not l.StartsWith("//") Then
                    settings.Add(l.Split("=")(0), l.Split("=")(1))
                End If
            Next
            listen_addr = IPAddress.Parse(settings("address"))
            port = settings("port")
            time_out = settings("timeout")
            allow_multi = settings("allow_multi_connect")
            world_size = settings("world_size")
            base_size = settings("base_size")
            max_party = settings("maximum_party")
            max_size = settings("max_size")
            max_life = settings("max_life")
            max_vel = settings("max_vel")
            min_size = settings("size_addition")
            ss_div = settings("size_score_division")
            accel = settings("acceleration")
            diss = settings("dissipation")
            collision = settings("collision_division")
            damage_on_hit = settings("damage")
            knockback = settings("knockback_division")
            deflect = settings("bullet_deflect")
            shoot_tick = settings("shoot_interval")
            bullet_speed = settings("bullet_speed")
            kill_multiplier = settings("kill_multiplier")
            starting_score = settings("starting_score")
        Else
            File.Create("server.ini").Dispose()
            File.WriteAllLines("server.ini", {
                               "",
                               "// networking settings",
                               "address=" & listen_addr.ToString,
                               "port=" & port.ToString,
                               "timeout=" & time_out.ToString,
                               "allow_multi_connect=" & allow_multi.ToString,
                               "",
                               "// world settings",
                               "world_size=" & world_size.ToString,
                               "base_size=" & base_size.ToString,
                               "maximum_party=" & max_party.ToString,
                               "",
                               "// player maximums",
                               "max_size=" & max_size.ToString,
                               "max_life=" & max_life.ToString,
                               "max_vel=" & max_vel.ToString,
                               "",
                               "// player constants",
                               "size_addition=" & min_size.ToString,
                               "size_score_division=" & ss_div.ToString,
                               "acceleration=" & accel.ToString,
                               "dissipation=" & diss.ToString,
                               "collision_division=" & collision.ToString,
                               "",
                               "// bullet constants",
                               "damage=" & damage_on_hit.ToString,
                               "knockback_division=" & knockback.ToString,
                               "bullet_deflect=" & deflect.ToString,
                               "shoot_interval=" & shoot_tick.ToString,
                               "bullet_speed=" & bullet_speed.ToString,
                               "",
                               "kill_multiplier=" & kill_multiplier.ToString,
                               "starting_score=" & starting_score.ToString})
            Load()
        End If
    End Sub

    Sub Main()
        Load()
        Console.Title = "Server"

        Dim s As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        s.Bind(New IPEndPoint(listen_addr, port))
        s.Listen(10)
        Dim t As New Thread(
            New ThreadStart(
            Function()
                While True
                    Dim a As Socket = s.Accept()
                    Dim n As New Thread(
                    New ThreadStart(
                    Function()
                        Try
                            Dim st As New NetworkStream(a)
                            Using w As New StreamWriter(st), r As New StreamReader(st)
                                Thread.Sleep(rand.Next(100, 1000))
                                Dim addr As String = DirectCast(a.RemoteEndPoint, IPEndPoint).Address.ToString
                                Join(addr)
                                Dim this As New Player(rand.Next(0 - world_size / 2, world_size), rand.Next(0 - world_size / 2, world_size), addr, MakeTeam)
                                If this.Team = 0 Then this.X = rand.Next(0 - (world_size / 2), 0 - (world_size / 2) + base_size) Else this.X = rand.Next((world_size / 2) - base_size, world_size / 2)
                                If allow_multi = False Then
                                    For Each p As Player In players
                                        If p.Address.ToString = addr Then
                                            Reply("multi", w)
                                            Return False
                                        End If
                                    Next
                                End If
                                players.Add(this)
                                Reply("online", w)
                                Try
                                    While True
                                        Dim req As String = r.ReadLine()
                                        Dim args As String = ""
                                        If req.StartsWith("dwn") Or req.StartsWith("kup") Or req.StartsWith("dir") Then args = req.Split(" ")(1)
                                        Dim c As String = req.Split(" ")(0)
                                        Select Case c
                                            Case "dwn"
                                                this.KeyDown([Enum].Parse(GetType(Keys), args))
                                            Case "kup"
                                                this.KeyUp([Enum].Parse(GetType(Keys), args))
                                            Case "wrd"
                                                Reply(MakeWorldString(this), w)
                                            Case "sht"
                                                this.Shooting = True
                                            Case "ces"
                                                this.Shooting = False
                                            Case "dir"
                                                this.Direction = args
                                            Case "qui"
                                                Leave(addr)
                                                this.HP = -100
                                                a.Close()
                                                Return False
                                        End Select
                                    End While
                                Catch ex As Exception
                                    this.HP = -100
                                    Disconnect(addr)
                                End Try
                            End Using
                        Catch ex As Exception
                            Return False
                        End Try
                        Return True
                    End Function))
                    n.IsBackground = True
                    n.Start()
                End While
                Return True
            End Function))
        t.IsBackground = True
        t.Start()

        Dim i As New Thread(
            New ThreadStart(
            Function()
                While True
                    Iterate()
                    Thread.Sleep(1000 / fps)
                End While
                Return True
            End Function))
        i.IsBackground = True
        i.Start()

        While True
            Dim key = Console.ReadKey(True)
            If ((key.Modifiers And ConsoleModifiers.Control) <> 0) And key.Key = ConsoleKey.R Then
                Load()
            ElseIf ((key.Modifiers And ConsoleModifiers.Control) <> 0) And key.Key = ConsoleKey.N Then
                Dim add As New Player(rand.Next(0 - world_size / 2, world_size), rand.Next(0 - world_size / 2, world_size), "0.0.0.0", MakeTeam)
                add.Score = rand.Next(starting_score, 3000)
                add.VelX = rand.Next(-10, 10)
                add.VelY = rand.Next(-10, 10)
                add.Shooting = True
                players.Add(add)
            End If
        End While
    End Sub

    Public Function MakeTeam() As Integer
        Dim blues As Integer = 0, reds As Integer = 0
        For Each p As Player In players
            If p.Team = 0 Then blues += 1 Else reds += 1
        Next
        If blues = reds Then Return rand.Next(0, 2)
        If blues > reds Then Return 1 Else Return 0
    End Function

    Public Function MakeWorldString(p As Player) As String
        Try
            Dim out As String = ""
            If p.HP > 0 Then out &= "pb{a=1," Else out &= "pb{a=0,"
            out &= "x=" & ((p.X - width_ / 2) + (p.VelX * thrust)).ToString & ",y=" & ((p.Y - height_ / 2) + (p.VelY * thrust)).ToString
            out &= ",w=" & world_size.ToString
            out &= ",t=" & p.Team.ToString & ",bs=" & base_size.ToString & "}|"
            For Each i As Player In players
                out &= If(i Is p, "pt{x=", "pp{x=") & i.X.ToString & ",y=" & i.Y.ToString & ",s=" & i.Size.ToString & ",d=" & i.Direction.ToString & ",t=" & i.Team.ToString & "}|"
            Next
            For Each i As DeadPlayer In dead_players
                out &= "pd{x=" & i.X.ToString & ",y=" & i.Y.ToString & ",s=" & i.Size.ToString & ",d=" & i.Direction.ToString & ",t=" & i.Team.ToString & ",a=" & i.Alpha.ToString & "}|"
            Next
            For Each i As Bullet In bullets
                out &= "bb{x=" & i.X.ToString & ",y=" & i.Y.ToString & ",s=" & i.Size.ToString & ",t=" & i.Sender.Team.ToString & "}|"
            Next
            For Each i As DeadBullet In dead_bullets
                out &= "bd{x=" & i.X.ToString & ",y=" & i.Y.ToString & ",s=" & i.Size.ToString & ",t=" & i.Team.ToString & ",a=" & i.Alpha.ToString & "}|"
            Next
            Return out
        Catch ex As Exception
            Return MakeWorldString(p)
        End Try
    End Function

    Public Sub Reply(s As String, n As StreamWriter)
        n.WriteLine(s)
        n.Flush()
    End Sub

    Public Sub Join(j As String)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(Now.ToString("[ hh:mm:ss ] "))
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(j)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine(" joined the game")
    End Sub

    Public Sub Leave(l As String)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(Now.ToString("[ hh:mm:ss ] "))
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(l)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine(" left the game")
    End Sub

    Public Sub Disconnect(t As String)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(Now.ToString("[ hh:mm:ss ] "))
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(t)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine(" disconnected")
    End Sub

    Public Sub Kill(k As String, v As String)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(Now.ToString("[ hh:mm:ss ] "))
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(k)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(" killed ")
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine(v)
    End Sub

    Public Sub Iterate()
        Try
            Dim garabage As New List(Of Object)
            For Each b As DeadBullet In dead_bullets
                b.Tick()
                If b.Expired = True Then garabage.Add(b)
            Next
            For Each b As Bullet In bullets
                If b.X < 0 - world_size / 2 + base_size And b.Sender.Team = 1 Then
                    b.VelX += deflect
                End If
                If b.X > world_size / 2 - base_size And b.Sender.Team = 0 Then
                    b.VelX -= deflect
                End If
                b.Tick()
                If b.Life < 0 Then
                    garabage.Add(b)
                    dead_bullets.Add(New DeadBullet(b))
                End If
            Next
            For Each p As DeadPlayer In dead_players
                p.Tick()
                If p.Expired = True Then garabage.Add(p)
            Next
            For Each p As Player In players
                If p.X < 0 - world_size / 2 + base_size And p.Team = 1 Then
                    p.X = 0 - world_size / 2 + base_size
                End If
                If p.X > world_size / 2 - base_size And p.Team = 0 Then
                    p.X = world_size / 2 - base_size
                End If
                Dim hbox As New Rectangle(p.X - p.Size / 2, p.Y - p.Size / 2, p.Size, p.Size)
                For Each b As Bullet In bullets
                    If Not b.Sender.Team = p.Team Then
                        If hbox.Contains(New Point(b.X, b.Y)) Then
                            b.Life = 0
                            p.VelX += b.VelX / knockback
                            p.VelY += b.VelY / knockback
                            p.HP -= 5
                            If p.HP < 0 Then
                                b.Sender.Score += p.Score * kill_multiplier
                                Kill(b.Sender.Address.ToString, p.Address.ToString)
                            End If
                        End If
                    End If
                Next
                For Each o As Player In players
                    If Not o Is p Then
                        Dim ohbox As New Rectangle(o.X - o.Size / 2, o.Y - o.Size / 2, o.Size, o.Size)
                        If ohbox.IntersectsWith(hbox) Then
                            o.VelX += 0 - p.VelX / collision
                            o.VelY += 0 - p.VelY / collision
                        End If
                    End If
                Next
                If p.Address.ToString = "0.0.0.0" Then
                    p.Direction += 1
                    If rand.Next(0, 2) = 0 And Not p.Keys(Keys.S) = True Then p.KeyDown(Keys.W)
                    If rand.Next(0, 2) = 0 And Not p.Keys(Keys.D) = True Then p.KeyDown(Keys.A)
                    If rand.Next(0, 2) = 0 And Not p.Keys(Keys.W) = True Then p.KeyDown(Keys.S)
                    If rand.Next(0, 2) = 0 And Not p.Keys(Keys.A) = True Then p.KeyDown(Keys.D)
                    If Not rand.Next(0, 5) = 4 Then
                        If rand.Next(0, 2) = 0 Then p.KeyUp(Keys.W)
                        If rand.Next(0, 2) = 0 Then p.KeyUp(Keys.A)
                        If rand.Next(0, 2) = 0 Then p.KeyUp(Keys.S)
                        If rand.Next(0, 2) = 0 Then p.KeyUp(Keys.D)
                    End If
                    For Each r As Player In players
                        If Not r.Team = p.Team Then
                            Dim x As Point = New Point(r.X, r.Y)
                            If p.X > x.X Then
                                p.KeyDown(Keys.A)
                                p.KeyUp(Keys.D)
                            End If
                            If p.X < x.X Then
                                p.KeyDown(Keys.D)
                                p.KeyUp(Keys.A)
                            End If
                            If p.Y < x.Y Then
                                p.KeyDown(Keys.S)
                                p.KeyUp(Keys.W)
                            End If
                            If p.Y > x.Y Then
                                p.KeyDown(Keys.W)
                                p.KeyUp(Keys.S)
                            End If
                            Dim rad As Integer = 250
                            If New Rectangle(r.X - rad, r.Y - rad, rad * 2, rad * 2).Contains(New Point(p.X, p.Y)) Then p.Shooting = True Else p.Shooting = False
                        End If
                    Next
                    If p.HP < max_life / 2 Then
                        If p.Team = 0 Then
                            p.KeyDown(Keys.A)
                            p.KeyUp(Keys.D)
                        Else
                            p.KeyDown(Keys.D)
                            p.KeyUp(Keys.A)
                        End If
                        If rand.Next(0, 5) = 0 Then p.Shooting = True Else p.Shooting = False
                    End If
                End If
                p.Tick()
                If p.HP < 0 Then
                    garabage.Add(p)
                    dead_players.Add(New DeadPlayer(p))
                End If
            Next
            For Each o As Object In garabage
                If TypeOf o Is Bullet Then
                    bullets.Remove(o)
                ElseIf TypeOf o Is Player Then
                    players.Remove(o)
                ElseIf TypeOf o Is DeadBullet Then
                    dead_bullets.Remove(o)
                ElseIf TypeOf o Is DeadPlayer Then
                    dead_players.Remove(o)
                End If
            Next
            tick += 1
        Catch ex As Exception
        End Try
    End Sub

    Public Function RotatePoint(pointToRotate As Point, centerPoint As Point, angleInDegrees As Double) As Point
        Dim angleInRadians As Double = angleInDegrees * (Math.PI / 180)
        Dim cosTheta As Double = Math.Cos(angleInRadians)
        Dim sinTheta As Double = Math.Sin(angleInRadians)
        Return New Point(cosTheta * (pointToRotate.X - centerPoint.X) - sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X,
                         sinTheta * (pointToRotate.X - centerPoint.X) + cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
    End Function

    Public Function IsDivisible(x As Integer, y As Integer) As Boolean
        Return (x Mod y) = 0
    End Function

    Class Player
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Score As Single
        Public HP As Integer
        Public Direction As Single
        Public Address As IPAddress
        Public Team As Integer '0 is blue, 1 is red
        Public Shooting As Boolean = False

        Public VelX As Single
        Public VelY As Single
        Public Keys(256) As Boolean

        Public Sub New(x As Single, y As Single, address As String, team As Integer)
            Me.X = x
            Me.Y = y
            Score = starting_score
            Size = min_size
            Direction = 0
            Me.Address = IPAddress.Parse(address)
            VelX = 0
            VelY = 0
            Keys = New Boolean(256) {}
            HP = max_life
            Me.Team = team
        End Sub

        Public Sub KeyDown(e As Keys)
            Keys(e) = True
        End Sub

        Public Sub KeyUp(e As Keys)
            Keys(e) = False
        End Sub

        Public Sub Shoot()
            bullets.Add(New Bullet(X, Y, Me))
        End Sub

        Public Sub Tick()
            X += VelX
            Y += VelY
            VelX -= VelX / diss
            VelY -= VelY / diss
            If Team = 0 And X < 0 - world_size / 2 + base_size Then HP += regen_per_tick Else HP += regen_per_tick / 4
            If Team = 1 And X > world_size / 2 - base_size Then HP += regen_per_tick Else HP += regen_per_tick / 4
            If X > world_size / 2 Then X = world_size / 2
            If X < 0 - world_size / 2 Then X = 0 - world_size / 2
            If Y > world_size / 2 Then Y = world_size / 2
            If Y < 0 - world_size / 2 Then Y = 0 - world_size / 2
            If HP > max_life Then HP = max_life
            If VelX > max_vel Then VelX = max_vel
            If VelX < 0 - max_vel Then VelX = 0 - max_vel
            If VelY > max_vel Then VelY = max_vel
            If VelY < 0 - max_vel Then VelY = 0 - max_vel
            Size = (Score / ss_div) + min_size
            If Size > max_size Then Size = max_size
            If Keys(System.Windows.Forms.Keys.A) Or Keys(System.Windows.Forms.Keys.Left) Then VelX -= accel
            If Keys(System.Windows.Forms.Keys.D) Or Keys(System.Windows.Forms.Keys.Right) Then VelX += accel
            If Keys(System.Windows.Forms.Keys.S) Or Keys(System.Windows.Forms.Keys.Down) Then VelY += accel
            If Keys(System.Windows.Forms.Keys.W) Or Keys(System.Windows.Forms.Keys.Up) Then VelY -= accel
            If IsDivisible(Module1.tick, shoot_tick) And Shooting = True Then Shoot()
        End Sub
    End Class

    Class Bullet
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public VelX As Single
        Public VelY As Single
        Public Sender As Player
        Public Life As Integer = 300

        Public Sub Tick()
            X += VelX
            Y += VelY
            Life -= 1
            If X > world_size / 2 Then Life = -1
            If X < 0 - world_size / 2 Then Life = -1
            If Y > world_size / 2 Then Life = -1
            If Y < 0 - world_size / 2 Then Life = -1
        End Sub

        Public Sub New(x As Single, y As Single, s As Player)
            Sender = s
            Size = (s.Size / 2) - 4
            Dim o As Point = RotatePoint(New Point(bullet_speed, 0), New Point(0, 0), s.Direction)
            VelX = o.X
            VelY = o.Y
            Me.X = x
            Me.Y = y
        End Sub
    End Class

    Class DeadBullet
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public VelX As Single
        Public VelY As Single
        Public Alpha As Single
        Public Expired As Boolean = False
        Public Team As Integer

        Public Sub New(b As Bullet)
            X = b.X
            Y = b.Y
            Size = b.Size
            VelX = b.VelX / 5
            VelY = b.VelY / 5
            Team = b.Sender.Team
            Alpha = 255
            Expired = False
        End Sub

        Public Sub Tick()
            X += VelX
            Y += VelY
            Alpha -= 30
            Size += 1
            If Alpha < 0 Then Expired = True
        End Sub
    End Class

    Class DeadPlayer
        Public X As Single
        Public Y As Single
        Public Size As Single
        Public Direction As Single
        Public VelX As Single
        Public VelY As Single
        Public Alpha As Single = 255
        Public Expired As Boolean = False
        Public Team As Integer

        Public Sub New(p As Player)
            X = p.X
            Y = p.Y
            Size = p.Size
            Direction = p.Direction
            VelX = p.VelX / 5
            VelY = p.VelY / 5
            Team = p.Team
        End Sub

        Public Sub Tick()
            X += VelX
            Y += VelY
            Alpha -= 30
            Size += 3
            If Alpha < 0 Then Expired = True
        End Sub
    End Class
End Module
