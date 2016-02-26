Option Explicit On
Option Strict On

Public Class Form1
    Dim LEVELBMP As New Bitmap(130, 150 + 50)
    Dim LEVELGRAPHIC As Graphics = Graphics.FromImage(LEVELBMP)
    Dim BUFFERBITMAP As New Bitmap(130, 150 + 50)
    Dim BUFFER As Graphics = Graphics.FromImage(BUFFERBITMAP)
    Dim game As New Level()
    Dim multiplier As Integer = 5
    Dim schermata As String = "Schermata iniziale"
    Dim randInt As New Random
    Dim _numerooggettosuccessivo As ULong
    ReadOnly Property numerooggettosuccessivo As ULong
        Get
            If _numerooggettosuccessivo >= ULong.MaxValue - 1 Then
                _numerooggettosuccessivo = 0
            Else
                _numerooggettosuccessivo += CType(1, ULong)
            End If
            Return _numerooggettosuccessivo
        End Get
    End Property

    Private Property livellocorrente As UInteger
        Get
            Return game.number
        End Get
        Set(value As UInteger)
            If livelli.ContainsKey(value) Then
                game = CType(livelli(value).Clone, Level)
            Else
                MsgBox("Livello " & value & " non esistente", MsgBoxStyle.Critical)
            End If
        End Set
    End Property
    Dim livelli As New Dictionary(Of UInteger, Level)
    Dim _risorse_immagini As New Dictionary(Of UInt16, Bitmap)
    Dim _animazioni As Animazione()() = New Animazione()() {}
    Public Shared Oggetti As Oggetto() = New Oggetto() {}

    Function PrendiRisorseImmagini(key As UInt16) As Bitmap
        If _risorse_immagini.ContainsKey(key) Then
            Return _risorse_immagini(key)
        Else
            Return _risorse_immagini(10)
        End If
    End Function
    Sub ImpostaRisorseImmagini(key As UInt16, value As Bitmap)
        If _risorse_immagini.ContainsKey(key) Then
            _risorse_immagini(key) = value
        Else
            _risorse_immagini.Add(key, value)
        End If
    End Sub

    Function PrendiIndexOggetto(Name As String) As Integer
        For index = 0 To Oggetti.Length - 1
            If Not IsNothing(Oggetti(index)) Then
                Try
                    If Oggetti(index).Name = Name Then
                        Return index
                    End If
                Catch
                    Return -1
                End Try
            End If
        Next
        Return -1
    End Function

    Function AggiungiOggetto(ogg As Oggetto) As Integer
        For index = 0 To Oggetti.Length
            If index = Oggetti.Length Then
                ReDim Preserve Oggetti(Oggetti.Length)
            End If
            If IsNothing(Oggetti(index)) Then
                Oggetti(index) = ogg
                Return index
            End If
        Next
        Return -1
    End Function

    Sub ImpostaOggetto(index As Integer, ogg As Oggetto)
        Oggetti(index) = ogg
    End Sub
    Sub ImpostaAnimazione(index As Integer, an As Animazione())
        If _animazioni.Length <= index And index >= 0 Then
            ReDim _animazioni(index + 1)
        End If
        _animazioni(index) = an
    End Sub

    Sub EliminaOggetto(index As Integer, Optional EliminaAnimazioni As Boolean = True)
        If index < Oggetti.Length Then
            If EliminaAnimazioni = True And index < _animazioni.Length Then
                _animazioni(index) = Nothing
            End If
            Oggetti(index) = Nothing
        End If
    End Sub
    Dim currentPosition As Point = Nothing
    Dim schermatainizialeattivata As Boolean = False

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        BUFFER.Clear(Color.Black)
        Dim rect As Rectangle
        If schermata = "Livello" Then
            LEVELGRAPHIC.Clear(Color.Black)
            rect = New Rectangle(0, 0 + 50, 130, 150)
            LEVELGRAPHIC.DrawImage(My.Resources.bg, rect)
            rect = New Rectangle(3, 131 + 50, 52, 5)
            LEVELGRAPHIC.DrawImage(CType(IIf(game.difficulty = DifficultyEnum.Beginner, My.Resources.beginner, IIf(game.difficulty = DifficultyEnum.Intermediate, My.Resources.intermediate, IIf(game.difficulty = DifficultyEnum.Advanced, My.Resources.advanced, My.Resources.expert))), Image), rect)
            For x = 5 To 0 Step -1
                For y = 0 To 5
                    rect = New Rectangle(x * 20 + 5, y * 20 + 8 + 50, 20, 20)
                    LEVELGRAPHIC.FillRectangle(New SolidBrush(Color.Black), rect)
                    rect = New Rectangle(x * 20 + 5, y * 20 + 8 + 50, 20, 20)
                    LEVELGRAPHIC.DrawImage(My.Resources._0, rect)
                Next
            Next
            rect = New Rectangle(0, 0 + 50, 130, 150)
            LEVELGRAPHIC.DrawImage(My.Resources.shadow, rect)
            For x = 5 To 0 Step -1
                For y = 0 To 5
                    Dim i = game.path.Item(New Point(x, y))
                    If i.colore <> BColor.Empty And i.tipo <> BType._0_Ground Then
                        DrawBlock(i, x, y, False)
                        DrawBlock(i, x, y, True)
                    End If
                    i = Nothing
                Next
            Next

            'GUI
            If Me._giocooccupato = True Then
                rect = New Rectangle(58, 132 + 1 + 50, 14, 14)
                LEVELGRAPHIC.DrawImage(PrendiRisorseImmagini(1), rect)
            Else
                rect = New Rectangle(58 - 1, 132 + 1 + 50, 14, 14)
                LEVELGRAPHIC.DrawImage(PrendiRisorseImmagini(2), rect)
                rect = New Rectangle(58, 132 + 1 + 50, 14, 14)
                LEVELGRAPHIC.DrawImage(PrendiRisorseImmagini(2), rect)
                rect = New Rectangle(58, 132 + 50, 14, 14)
                LEVELGRAPHIC.DrawImage(PrendiRisorseImmagini(0), rect)
            End If
        ElseIf schermata = "Schermata iniziale" And schermatainizialeattivata = False Then
            schermatainizialeattivata = True
            Dim t As New Threading.Thread(AddressOf HomeThread)
            t.SetApartmentState(Threading.ApartmentState.STA)
            t.Start()
            For Index = 0 To Oggetti.Length - 1
                Dim obj As Oggetto = Oggetti(Index)
                If Not IsNothing(obj) Then
                    If obj.Name.StartsWith("iBlocco") Then
                        EliminaOggetto(Index)
                    End If
                End If
            Next
            Dim nomeogg As String = "iBlocco" & numerooggettosuccessivo
            'ImpostaAnimazione(AggiungiOggetto(New Oggetto(nomeogg, New Rectangle(0, 150 - 20, 20, 20), PrendiRisorseImmagini(22), PosizioneZ.Dietro)), New Animazione() {New Animazione(nomeogg, TipoAnimazione.Posizione, New Point(-20, 150 - 20), New Point(130, 150 - 20), 3000, 1)})
        ElseIf schermata = "Selezione Livello" Then

        End If
        For Each obj In Oggetti
            If Not IsNothing(obj) Then
                If obj.posizione = PosizioneZ.Dietro And obj.visible Then
                    BUFFER.DrawImage(obj.Image, New Rectangle(New Point(obj.Location.X, obj.Location.Y + 50), obj.Size))
                End If
            End If
        Next
        For Each obj In Oggetti
            If Not IsNothing(obj) Then
                If obj.posizione = PosizioneZ.Normale And obj.visible Then
                    BUFFER.DrawImage(obj.Image, New Rectangle(New Point(obj.Location.X, obj.Location.Y + 50), obj.Size))
                End If
            End If
        Next
        For Each obj In Oggetti
            If Not IsNothing(obj) Then
                If obj.posizione = PosizioneZ.Davanti And obj.visible Then
                    BUFFER.DrawImage(obj.Image, New Rectangle(New Point(obj.Location.X, obj.Location.Y + 50), obj.Size))
                End If
            End If
        Next

        'FINE DISEGNO
        e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        rect = New Rectangle(CInt(ClientRectangle.Width / 2 - (130 * multiplier) / 2), CInt((ClientRectangle.Height / 2 - ((150 * multiplier) / 2) - 50 * multiplier)), 130 * multiplier, (150 + 50) * multiplier)
        e.Graphics.DrawImage(BUFFERBITMAP, rect)
        rect = Nothing
    End Sub

    Private Sub HomeThread()
        While schermata = "Schermata iniziale"
            Dim i As UInt16 = CUShort(randInt.Next(1, 15))
            Dim indexogg As Integer = PrendiIndexOggetto("Luce di sfondo")
            If indexogg <> -1 And i = 1 Then
                Oggetti(indexogg).visible = False
            ElseIf indexogg <> -1 And i = 2 Then
                Oggetti(indexogg).visible = True
                ImpostaOggetto(indexogg, New Oggetto("Luce di sfondo", New Rectangle(0, 0, 130, 150), PrendiRisorseImmagini(CUShort(2 + i)), PosizioneZ.Normale))
            ElseIf indexogg <> -1 And i > 2 Then
                Oggetti(indexogg).visible = True
                ImpostaOggetto(indexogg, New Oggetto("Luce di sfondo", New Rectangle(0, 0, 130, 150), PrendiRisorseImmagini(9), PosizioneZ.Normale))
            End If

            System.Threading.Thread.Sleep(10)
        End While
        For index = 0 To Oggetti.Length - 1
            Dim obj As Oggetto = Oggetti(index)
            If Not IsNothing(obj) Then
                If obj.Name.StartsWith("iBlocco") Then
                    EliminaOggetto(index)
                End If
            End If
        Next
    End Sub

    Private Sub LoadResources()
        Dim tempimage As Bitmap
        ImpostaRisorseImmagini(0, My.Resources.restart) 'RESTART
        tempimage = CType(My.Resources.restart.Clone, Bitmap)
        tempimage.Filters.GrayScale()
        ImpostaRisorseImmagini(1, tempimage) 'RESTART GRAY
        tempimage = CType(My.Resources.restart.Clone, Bitmap)
        tempimage.Filters.Brightness(-1)
        ImpostaRisorseImmagini(2, tempimage) 'RESTART BLACK

        ImpostaRisorseImmagini(3, My.Resources.logo_off)
        ImpostaRisorseImmagini(4, My.Resources.logo_off2)
        ImpostaRisorseImmagini(5, My.Resources.logo_off3)
        ImpostaRisorseImmagini(6, My.Resources.logo_off4)
        ImpostaRisorseImmagini(7, My.Resources.logo_off5)
        ImpostaRisorseImmagini(8, My.Resources.logo_off6)
        ImpostaRisorseImmagini(9, My.Resources.logo_on)

        ImpostaRisorseImmagini(010, My.Resources._0) 'BLOCK IMAGE GROUND

        ImpostaRisorseImmagini(020, My.Resources._2) 'BLOCK IMAGE HOLLOW

        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(021, tempimage) 'BLOCK IMAGE HOLLOW, SIDE, RED
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(022, tempimage) 'BLOCK IMAGE HOLLOW, SIDE, YELLOW
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(023, tempimage) 'BLOCK IMAGE HOLLOW, SIDE, GREEN
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(024, tempimage) 'BLOCK IMAGE HOLLOW, SIDE, BLUE

        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 1, 0)
        ImpostaRisorseImmagini(025, tempimage) 'BLOCK IMAGE HOLLOW, TOP, RED
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 1, 0)
        ImpostaRisorseImmagini(026, tempimage) 'BLOCK IMAGE HOLLOW, TOP, YELLOW
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 1, 0)
        ImpostaRisorseImmagini(027, tempimage) 'BLOCK IMAGE HOLLOW, TOP, GREEN
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 1, 0)
        ImpostaRisorseImmagini(028, tempimage) 'BLOCK IMAGE HOLLOW, TOP, BLUE

        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(125, tempimage) 'BLOCK IMAGE HOLLOW, BOTTOM, RED
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(126, tempimage) 'BLOCK IMAGE HOLLOW, BOTTOM, YELLOW
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(127, tempimage) 'BLOCK IMAGE HOLLOW, BOTTOM, GREEN
        tempimage = CType(PrendiRisorseImmagini(20).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(128, tempimage) 'BLOCK IMAGE HOLLOW, BOTTOM, BLUE

        ImpostaRisorseImmagini(030, My.Resources._3) 'BLOCK IMAGE FILL

        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(031, tempimage) 'BLOCK IMAGE FILL, SIDE, RED
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(032, tempimage) 'BLOCK IMAGE FILL, SIDE, YELLOW
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(033, tempimage) 'BLOCK IMAGE FILL, SIDE, GREEN
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(034, tempimage) 'BLOCK IMAGE FILL, SIDE, BLUE

        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 1, 0)
        ImpostaRisorseImmagini(035, tempimage) 'BLOCK IMAGE FILL, TOP, RED
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 1, 0)
        ImpostaRisorseImmagini(036, tempimage) 'BLOCK IMAGE FILL, TOP, YELLOW
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 1, 0)
        ImpostaRisorseImmagini(037, tempimage) 'BLOCK IMAGE FILL, TOP, GREEN
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 1, 0)
        ImpostaRisorseImmagini(038, tempimage) 'BLOCK IMAGE FILL, TOP, BLUE

        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(135, tempimage) 'BLOCK IMAGE FILL, BOTTOM, RED
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(136, tempimage) 'BLOCK IMAGE FILL, BOTTOM, YELLOW
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(137, tempimage) 'BLOCK IMAGE FILL, BOTTOM, GREEN
        tempimage = CType(PrendiRisorseImmagini(30).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(138, tempimage) 'BLOCK IMAGE FILL, BOTTOM, BLUE

        ImpostaRisorseImmagini(040, My.Resources._1) 'BLOCK IMAGE GRID

        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(041, tempimage) 'BLOCK IMAGE GRID, SIDE, RED
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(042, tempimage) 'BLOCK IMAGE GRID, SIDE, YELLOW
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(043, tempimage) 'BLOCK IMAGE GRID, SIDE, GREEN
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.3)
        ImpostaRisorseImmagini(044, tempimage) 'BLOCK IMAGE GRID, SIDE, BLUE

        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 1, 0)
        ImpostaRisorseImmagini(045, tempimage) 'BLOCK IMAGE GRID, TOP, RED
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 1, 0)
        ImpostaRisorseImmagini(046, tempimage) 'BLOCK IMAGE GRID, TOP, YELLOW
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 1, 0)
        ImpostaRisorseImmagini(047, tempimage) 'BLOCK IMAGE GRID, TOP, GREEN
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 1, 0)
        ImpostaRisorseImmagini(048, tempimage) 'BLOCK IMAGE GRID, TOP, BLUE

        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Red.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(145, tempimage) 'BLOCK IMAGE GRID, BOTTOM, RED
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Yellow.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(146, tempimage) 'BLOCK IMAGE GRID, BOTTOM, YELLOW
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Green.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(147, tempimage) 'BLOCK IMAGE GRID, BOTTOM, GREEN
        tempimage = CType(PrendiRisorseImmagini(40).Clone, Bitmap)
        tempimage.Filters.HSL(Color.Blue.GetHue, 0.5, -0.6)
        ImpostaRisorseImmagini(148, tempimage) 'BLOCK IMAGE GRID, BOTTOM, BLUE
    End Sub

    Private Sub DrawBlock(i As Blocco, x As Integer, y As Integer, ShadowMode As Boolean)
        Dim multiplier3D As UInt16 = 6
        Dim shadow_size As UInt16 = 4
        Dim leftheight As Int16 = BlockHeight(i)
        Dim bottomheight As Int16 = BlockHeight(i)
        Dim angleheight As Int16 = BlockHeight(i)
        If game.path.ContainsKey(New Point(x - 1, y)) Then
            leftheight -= BlockHeight(game.path(New Point(x - 1, y)))
        End If
        If game.path.ContainsKey(New Point(x, y + 1)) Then
            bottomheight -= BlockHeight(game.path(New Point(x, y + 1)))
        End If
        If game.path.ContainsKey(New Point(x - 1, y + 1)) Then
            angleheight -= BlockHeight(game.path(New Point(x - 1, y + 1)))
        End If
        Dim block_height As Int16 = BlockHeight(i)
        Dim blockimage As Bitmap = PrendiRisorseImmagini(CUShort((i.tipo + 1) * 10 + i.colore + 4))
        Dim blockimage_side As Bitmap = PrendiRisorseImmagini(CUShort((i.tipo + 1) * 10 + i.colore))

        Dim blockcolor As Color = DirectCast(IIf(i.colore = BColor.Blue, Color.Blue, IIf(i.colore = BColor.Green, Color.Green, IIf(i.colore = BColor.Red, Color.Red, Color.Yellow))), Color)
        If i.tipo <> BType._0_Ground And i.colore <> BColor.Empty Then
            If ShadowMode = False Then
                LEVELGRAPHIC.DrawImage(PrendiRisorseImmagini(CUShort((i.tipo + 1) * 10 + i.colore + 104)), New Rectangle(x * 20 + 5, y * 20 + 8 + 50, 20, 20))
            End If
        End If
        If ShadowMode = False Then
            For index = block_height - 1 To 0 Step -1
                LEVELGRAPHIC.DrawImage(CType(IIf(i.posizionegriglia = Faccia.Indietro, PrendiRisorseImmagini(CUShort(40 + i.colore)), blockimage_side), Image), New Rectangle(x * 20 + 5, y * 20 + 8 - block_height * multiplier3D + index * multiplier3D + 50, 20, multiplier3D))
            Next
            LEVELGRAPHIC.DrawImage(CType(IIf(i.posizionegriglia = Faccia.Su, PrendiRisorseImmagini(CUShort(40 + i.colore + 4)), blockimage), Image), New Rectangle(x * 20 + 5, y * 20 + 8 - block_height * multiplier3D + 50, 20, 20))
            For index = block_height - 1 To 0 Step -1
                LEVELGRAPHIC.DrawImage(CType(IIf(i.posizionegriglia = Faccia.Avanti, PrendiRisorseImmagini(CUShort(40 + i.colore)), blockimage_side), Image), New Rectangle(x * 20 + 5, y * 20 + 8 + 20 - block_height * multiplier3D + index * multiplier3D + 50, 20, multiplier3D))
            Next
        End If
        If ShadowMode = True Then
            Dim leftsh As New Bitmap(shadow_size, 20)
            Dim anglesh As New Bitmap(shadow_size, shadow_size)
            Dim bottomsh As New Bitmap(20, shadow_size)
            Dim leftg As Graphics = Graphics.FromImage(leftsh)
            If leftheight > 0 Then
                leftg.DrawImage(CType(IIf(leftheight = 1, My.Resources.s1, IIf(leftheight = 2, My.Resources.s2, IIf(leftheight = 3, My.Resources.s3, My.Resources.s4))), Image), New Rectangle(0, 0, shadow_size + 20, shadow_size + 20))
            End If
            Dim bottomg As Graphics = Graphics.FromImage(bottomsh)
            If bottomheight > 0 Then
                bottomg.DrawImage(CType(IIf(bottomheight = 1, My.Resources.s1, IIf(bottomheight = 2, My.Resources.s2, IIf(bottomheight = 3, My.Resources.s3, My.Resources.s4))), Image), New Rectangle(-shadow_size, -20, shadow_size + 20, shadow_size + 20))
            End If
            Dim angleg As Graphics = Graphics.FromImage(anglesh)
            If angleheight > 0 Then
                angleg.DrawImage(CType(IIf(angleheight = 1, My.Resources.s1, IIf(angleheight = 2, My.Resources.s2, IIf(angleheight = 3, My.Resources.s3, My.Resources.s4))), Image), New Rectangle(0, -20, shadow_size + 20, shadow_size + 20))
            End If
            Dim leftshadowposition As New Point(x * 20 + 5 - shadow_size, y * 20 + 8 - (block_height - leftheight) * multiplier3D)
            For index = y + 1 To 5 Step 1
                If x > 0 Then
                    If BlockHeight(game.path(New Point((x - 1), index))) >= 1 Then
                        leftg.FillRectangle(Brushes.Magenta, New Rectangle(((x - 1) * 20) - leftshadowposition.X + 5, (index * 20 - (BlockHeight(game.path(New Point((x - 1), index))) * multiplier3D)) - leftshadowposition.Y + 8, 20, 20 + (BlockHeight(game.path(New Point((x - 1), index))) * multiplier3D)))
                    End If
                End If
            Next
            leftsh.MakeTransparent(Color.Magenta)
            Dim bottomshadowposition As New Point(x * 20 + 5, (y + 1) * 20 + 8 - (block_height - bottomheight) * multiplier3D)
            For index = y + 2 To 5 Step 1
                If BlockHeight(game.path(New Point(x, index))) >= 1 Then
                    bottomg.FillRectangle(Brushes.Magenta, New Rectangle(((x) * 20) - bottomshadowposition.X + 5, (index * 20 - (BlockHeight(game.path(New Point((x), index))) * multiplier3D)) - bottomshadowposition.Y + 8, shadow_size + 20, BlockHeight(game.path(New Point(x, index))) * multiplier3D + 20))
                End If
            Next
            bottomsh.MakeTransparent(Color.Magenta)
            Dim angleshadowposition As New Point(x * 20 + 5 - shadow_size, (y + 1) * 20 + 8 - (block_height - angleheight) * multiplier3D)
            For index = y + 1 To 5 Step 1
                If x > 0 Then
                    If BlockHeight(game.path(New Point(x - 1, index))) >= 1 Then
                        angleg.FillRectangle(Brushes.Magenta, New Rectangle(((x - 1) * 20) - angleshadowposition.X + 5, (index * 20 - (BlockHeight(game.path(New Point((x - 1), index))) * multiplier3D)) - angleshadowposition.Y + 8, shadow_size + 20, BlockHeight(game.path(New Point(x - 1, index))) * multiplier3D + 20))
                    End If
                End If
            Next
            anglesh.MakeTransparent(Color.Magenta)
            LEVELGRAPHIC.DrawImage(leftsh, New Rectangle(leftshadowposition.X, leftshadowposition.Y + 50, leftsh.Width, leftsh.Height))
            LEVELGRAPHIC.DrawImage(bottomsh, New Rectangle(bottomshadowposition.X, bottomshadowposition.Y + 50, bottomsh.Width, bottomsh.Height))
            LEVELGRAPHIC.DrawImage(anglesh, New Rectangle(angleshadowposition.X, angleshadowposition.Y + 50, anglesh.Width, anglesh.Height))
        End If
        If game.SelectedBlock = New Point(x, y) Then
            If ShadowMode = False Then
                LEVELGRAPHIC.DrawImage(My.Resources.selection, New Rectangle(x * 20 + 5 + 1, y * 20 + 8 + 1 - block_height * multiplier3D + 50, 18, 18))
                currentPosition = New Point(x, y)
            End If
        End If
    End Sub

    Private Function BlockHeight(i As Blocco) As Int16
        Dim BlockHeightv As Int16 = 0
        If i.tipo = BType._0_Ground Or i.colore = BColor.Empty Then
            BlockHeightv = 0
        ElseIf i.CustomHeight > 0 Then
            BlockHeightv = CShort(i.CustomHeight)
        ElseIf i.colore = BColor.Blue Then
            BlockHeightv = 4
        ElseIf i.colore = BColor.Green Then
            BlockHeightv = 3
        ElseIf i.colore = BColor.Yellow Then
            BlockHeightv = 2
        ElseIf i.colore = BColor.Red Then
            BlockHeightv = 1
        End If
        Return BlockHeightv
    End Function

    Private Sub tickloop_Tick(sender As Object, e As EventArgs) Handles tickloop.Tick
        Me.Invalidate()
    End Sub

    Private Sub Tick(sender As Object, e As EventArgs)
        If _animazioni.Length > 0 Then
            For index = 0 To Oggetti.Length - 1
                Dim obj As Oggetto = Oggetti(index)
                Dim animations As Animazione() = _animazioni(index)
                If Not IsNothing(animations) And Not IsNothing(obj) Then
                    Dim i As UInteger = 0
                    For Each an In animations
                        If Not IsNothing(an) Then
                            If an.Ripetizioni = 0 Then
                                If Not IsNothing(_animazioni(index)) Then
                                    _animazioni(index).SetValue(Nothing, i)
                                End If
                                an = Nothing
                                Console.WriteLine("[LANCopter] Fine ripetizioni")
                                Continue For
                            End If
                            If an.TempoAttualeTicks >= an.TempoTicks Then
                                If an.AnimationType = TipoAnimazione.Immagine Then
                                    Dim img As Image
                                    If an.Prop1 Is GetType(Image) Then
                                        img = CType(an.Prop1, Image)
                                    Else
                                        If Not IsNothing(_animazioni(index)) Then
                                            _animazioni(index).SetValue(Nothing, i)
                                        End If
                                        an = Nothing
                                        Continue For
                                    End If
                                    obj.Image = img
                                    If an.Ripetizioni = -1 Then
                                        an.Ripetizioni = 0
                                    End If
                                ElseIf an.AnimationType = TipoAnimazione.Posizione Then
                                    Dim posiniziale As Point
                                    Dim posfinale As Point
                                    If an.Prop1 Is GetType(Point) And an.Prop2 Is GetType(Point) Then
                                        posiniziale = CType(an.Prop1, Point)
                                        posfinale = CType(an.Prop2, Point)
                                    Else
                                        If Not IsNothing(_animazioni(index)) Then
                                            _animazioni(index).SetValue(Nothing, i)
                                        End If
                                        an = Nothing
                                        Continue For
                                    End If
                                    obj.Location = posfinale

                                    an.Prop1 = posfinale
                                    an.Prop2 = New Point(posfinale.X + posfinale.X - posiniziale.X, posfinale.Y + posfinale.Y - posiniziale.Y)
                                Else
                                    Console.WriteLine("[LANCopter] è stata individuata un'animazione di tipo sconosciuto")
                                End If
                                an.TempoAttualeTicks = 0
                                If an.Ripetizioni = 0 Then
                                    If Not IsNothing(_animazioni(index)) Then
                                        _animazioni(index).SetValue(Nothing, i)
                                    End If
                                    an = Nothing
                                    Continue For
                                ElseIf an.Ripetizioni > 0 Then
                                    an.Ripetizioni -= 1
                                End If
                            ElseIf an.TempoAttualeTicks < an.TempoTicks Then
                                If an.AnimationType = TipoAnimazione.Posizione Then
                                    Dim posiniziale As Point
                                    Dim posfinale As Point
                                    posiniziale = CType(an.Prop1, Point)
                                    posfinale = CType(an.Prop2, Point)
                                    Dim DeltaStepPos As PointF = New PointF(CSng((posfinale.X - posiniziale.X) / an.TempoTicks), CSng((posfinale.Y - posiniziale.Y) / an.TempoTicks))
                                    Dim PosInThisTick As PointF = New PointF(posiniziale.X + (DeltaStepPos.X * an.TempoAttualeTicks), posiniziale.Y + (DeltaStepPos.Y * an.TempoAttualeTicks))
                                    obj.Location = New Point(CInt(PosInThisTick.X), CInt(PosInThisTick.Y))
                                    'TODO -SPOSTARE LE COSE DI "QUA SOPRA" QUI SOTTO.
                                End If
                            End If
                            an.TempoAttualeTicks = CUInt(an.TempoAttualeTicks + Me.tickloop.Interval) 'CAMBIARE IL VALORE + tot PER IMPOSTARLO CON IL CORRETTO TEMPO DEL THREAD
                            If Not IsNothing(_animazioni(index)) Then
                                _animazioni(index).SetValue(an, i)
                            End If
                        End If
                        i = CUInt(i + 1)
                    Next
                End If
            Next
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        multiplier = CInt(Math.Round(My.Computer.Screen.Bounds.Height / 150) - 1)
        Me.Refresh()
        LoadResources()
        Me.DoubleBuffered = True
        Me.Size = New Size(130 * multiplier, 150 * multiplier) + New Rectangle(New Point(Me.ClientRectangle.X, Me.ClientRectangle.Y), New Size(Me.Size.Width - Me.ClientRectangle.Width - 2, Me.Size.Height - Me.ClientRectangle.Height - 2)).Size
        Me.MinimumSize = Me.Size
        Me.ResizeRedraw = False
        With livelli
            Dim l As Dictionary(Of Point, Blocco)
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(3, 0), New Blocco(BColor.Green))
            l.Add(New Point(0, 1), New Blocco(BColor.Green))
            l.Add(New Point(4, 2), New Blocco(BColor.Yellow))
            l.Add(New Point(1, 4), New Blocco(BColor.Red))
            .Add(1, New Level(1, DifficultyEnum.Beginner, l, New Point(4, 2)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(0, 0), New Blocco(BColor.Blue))
            l.Add(New Point(4, 0), New Blocco(BColor.Green))
            l.Add(New Point(2, 1), New Blocco(BColor.Yellow))
            l.Add(New Point(5, 1), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 3), New Blocco(BColor.Red))
            l.Add(New Point(0, 5), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Green))
            .Add(2, New Level(2, DifficultyEnum.Beginner, l, New Point(0, 0)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(1, 0), New Blocco(BColor.Green))
            l.Add(New Point(5, 0), New Blocco(BColor.Green))
            l.Add(New Point(4, 1), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Blue))
            l.Add(New Point(1, 4), New Blocco(BColor.Red))
            .Add(3, New Level(3, DifficultyEnum.Beginner, l, New Point(4, 1)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(0, 0), New Blocco(BColor.Green))
            l.Add(New Point(1, 0), New Blocco(BColor.Green))
            l.Add(New Point(4, 0), New Blocco(BColor.Green))
            l.Add(New Point(0, 4), New Blocco(BColor.Yellow))
            l.Add(New Point(0, 5), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Blue))
            l.Add(New Point(5, 3), New Blocco(BColor.Red))
            .Add(4, New Level(4, DifficultyEnum.Beginner, l, New Point(5, 5)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(1, 0), New Blocco(BColor.Yellow))
            l.Add(New Point(5, 0), New Blocco(BColor.Blue))
            l.Add(New Point(3, 1), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 3), New Blocco(BColor.Yellow))
            l.Add(New Point(4, 4), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Red))
            l.Add(New Point(1, 2), New Blocco(BColor.Green))
            l.Add(New Point(2, 5), New Blocco(BColor.Green))
            .Add(5, New Level(5, DifficultyEnum.Beginner, l, New Point(1, 0)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(3, 1), New Blocco(BColor.Yellow))
            l.Add(New Point(1, 2), New Blocco(BColor.Green))
            l.Add(New Point(0, 4), New Blocco(BColor.Blue))
            l.Add(New Point(2, 5), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 5), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Red))
            .Add(6, New Level(6, DifficultyEnum.Beginner, l, New Point(3, 1)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(3, 0), New Blocco(BColor.Red))
            l.Add(New Point(1, 1), New Blocco(BColor.Yellow))
            l.Add(New Point(2, 3), New Blocco(BColor.Yellow))
            l.Add(New Point(2, 4), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 4), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 5), New Blocco(BColor.Yellow))
            l.Add(New Point(0, 4), New Blocco(BColor.Green))
            .Add(7, New Level(7, DifficultyEnum.Beginner, l, New Point(3, 5)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(0, 0), New Blocco(BColor.Green))
            l.Add(New Point(1, 1), New Blocco(BColor.Green))
            l.Add(New Point(2, 1), New Blocco(BColor.Green))
            l.Add(New Point(2, 2), New Blocco(BColor.Green))
            l.Add(New Point(5, 2), New Blocco(BColor.Yellow))
            l.Add(New Point(5, 3), New Blocco(BColor.Yellow))
            l.Add(New Point(0, 4), New Blocco(BColor.Blue))
            l.Add(New Point(5, 5), New Blocco(BColor.Red))
            .Add(8, New Level(8, DifficultyEnum.Beginner, l, New Point(0, 0)))
            l = New Dictionary(Of Point, Blocco)
            l.Add(New Point(2, 2), New Blocco(BColor.Yellow))
            l.Add(New Point(1, 3), New Blocco(BColor.Green))
            l.Add(New Point(2, 3), New Blocco(BColor.Yellow))
            l.Add(New Point(3, 3), New Blocco(BColor.Blue))
            l.Add(New Point(0, 4), New Blocco(BColor.Green))
            l.Add(New Point(2, 4), New Blocco(BColor.Blue))
            l.Add(New Point(4, 4), New Blocco(BColor.Red))
            l.Add(New Point(0, 5), New Blocco(BColor.Yellow))
            .Add(9, New Level(9, DifficultyEnum.Beginner, l, New Point(2, 2)))
        End With
        currentPosition = livelli(1).SelectedBlock
        LivelloSelezionato.Items.Clear()
        For Each livello In livelli
            LivelloSelezionato.Items.Add("Livello " & livello.Value.number & " (" & livello.Value.difficulty.ToString & ")")
        Next
        LivelloSelezionato.SelectedIndex = 0

        Dim ticktimer As New Timer()
        ticktimer.Interval = 50
        AddHandler ticktimer.Tick, AddressOf Tick
        ticktimer.Start()

        Me.Visible = False
        Me.WindowState = FormWindowState.Normal
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.TopMost = True
        Me.WindowState = FormWindowState.Maximized
        Me.Visible = True
        Me.Activate()

        AggiungiOggetto(New Oggetto("Sfondo", New Rectangle(0, 0, 130, 150), My.Resources.logo_off, PosizioneZ.Dietro))
        AggiungiOggetto(New Oggetto("Luce di sfondo", New Rectangle(0, 0, 130, 150), My.Resources.logo_on, PosizioneZ.Normale))
        AggiungiOggetto(New Oggetto("Pulsante seleziona livello", New Rectangle(39, 136, 53, 17), My.Resources.level_select, PosizioneZ.Davanti))
    End Sub

    Private Sub LivelloSelezionato_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LivelloSelezionato.SelectedIndexChanged
        livellocorrente = CUInt(LivelloSelezionato.Text.ToLower.Replace("livello ", "").Replace(" (beginner)", "").Replace(" (intermediate)", "").Replace(" (advanced)", "").Replace(" (expert)", ""))
        keygrabber.Focus()
    End Sub

    Public Enum BType
        _0_Ground
        _2_Hole
        _3_Fill
    End Enum

    Public Enum BColor
        Empty
        Red
        Yellow
        Green
        Blue
    End Enum

    Private Sub LivelloSelezionato_MouseEnter(sender As Object, e As EventArgs) Handles LivelloSelezionato.DropDownClosed
        keygrabber.Focus()
    End Sub

    Dim _movimentooccupato As Boolean = False
    Dim _giocooccupato As Boolean = False
    Private Sub keygrabber_KeyPress(sender As Object, e As PreviewKeyDownEventArgs) Handles keygrabber.PreviewKeyDown
        If _movimentooccupato = False Then
            _movimentooccupato = True
            Dim segno As Point
            Dim n As New Point(0, 0)
            If e.KeyCode = Keys.Up Then
                segno = New Point(0, -1)
            ElseIf e.KeyCode = Keys.Down Then
                segno = New Point(0, 1)
            ElseIf e.KeyCode = Keys.Left Then
                segno = New Point(-1, 0)
            ElseIf e.KeyCode = Keys.Right Then
                segno = New Point(1, 0)
            ElseIf e.KeyCode = Keys.F11 Or e.KeyCode = Keys.Escape
                If Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None Then
                    Me.Visible = False
                    Me.WindowState = FormWindowState.Normal
                    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
                    Me.TopMost = False
                    Me.WindowState = FormWindowState.Maximized
                    Me.Visible = True
                    Me.Activate()
                ElseIf e.KeyCode = Keys.F11
                    Me.Visible = False
                    Me.WindowState = FormWindowState.Normal
                    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
                    Me.TopMost = True
                    Me.WindowState = FormWindowState.Maximized
                    Me.Visible = True
                    Me.Activate()
                End If
            ElseIf schermata = "Schermata iniziale" And e.KeyCode = Keys.Enter Then
                schermata = "Livello"
                Oggetti = New Oggetto() {}
                AggiungiOggetto(New Oggetto("Livello", New Rectangle(0, -50, 130, 200), LEVELBMP, PosizioneZ.Dietro))
            End If
            If Not IsNothing(segno) Then
                If game.path(currentPosition).colore = BColor.Red Then
                    n = New Point(0 * segno.X, 0 * segno.Y)
                ElseIf game.path(currentPosition).colore = BColor.Yellow Then
                    n = New Point(1 * segno.X, 1 * segno.Y)
                ElseIf game.path(currentPosition).colore = BColor.Green Then
                    n = New Point(2 * segno.X, 2 * segno.Y)
                ElseIf game.path(currentPosition).colore = BColor.Blue Then
                    n = New Point(3 * segno.X, 3 * segno.Y)
                End If
                If game.path.ContainsKey(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)) Then
                    If game.path(currentPosition).posizionegriglia = Faccia.Su And game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).colore = BColor.Empty And game.path(currentPosition).colore <> BColor.Red Then
                        Dim doit As Boolean = True
                        For nnx As Integer = segno.X To n.X Step CInt(IIf(segno.X = 0, 1, segno.X))
                            For nny As Integer = segno.Y To n.Y Step CInt(IIf(segno.Y = 0, 1, segno.Y))
                                Dim nn As New Point(nnx, nny)
                                If game.path.ContainsKey(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)) Then
                                    If game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).colore <> BColor.Empty Or game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).tipo <> BType._0_Ground Then
                                        doit = False
                                        Exit For
                                    End If
                                Else
                                    doit = False
                                    Exit For
                                End If
                            Next
                        Next
                        If doit And game.path.ContainsKey(New Point(currentPosition.X + segno.X + n.X, currentPosition.Y + segno.Y + n.Y)) Then
                            If (New Point(currentPosition.X + segno.X + n.X, currentPosition.Y + segno.Y + n.Y)).X >= 0 Or (New Point(currentPosition.X + segno.X + n.X, currentPosition.Y + segno.Y + n.Y)).Y >= 0 Then
                                game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).colore = game.path(currentPosition).colore
                                game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).tipo = BType._3_Fill
                                game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).posizionegriglia = Faccia.Nessuna
                                game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).CustomHeight = 1
                                For nnx As Integer = segno.X To n.X Step CInt(IIf(segno.X = 0, 1, segno.X))
                                    For nny As Integer = segno.Y To n.Y Step CInt(IIf(segno.Y = 0, 1, segno.Y))
                                        Dim nn As New Point(nnx, nny)
                                        game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).colore = game.path(currentPosition).colore
                                        game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).tipo = BType._2_Hole
                                        game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).CustomHeight = 1
                                        If nnx = n.X And nny = n.Y Then
                                            If segno.X = 1 Then
                                                game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).posizionegriglia = Faccia.Destra
                                            ElseIf segno.X = -1 Then
                                                game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).posizionegriglia = Faccia.Sinistra
                                            ElseIf segno.Y = 1 Then
                                                game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).posizionegriglia = Faccia.Avanti
                                            ElseIf segno.Y = -1 Then
                                                game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).posizionegriglia = Faccia.Indietro
                                            End If
                                        Else
                                            game.path(New Point(currentPosition.X + segno.X + nn.X, currentPosition.Y + segno.Y + nn.Y)).posizionegriglia = Faccia.Nessuna
                                        End If
                                    Next
                                Next
                                game.path(currentPosition).colore = BColor.Empty
                                game.path(currentPosition).tipo = BType._0_Ground
                                game.path(currentPosition).CustomHeight = 0
                                game.path(currentPosition).posizionegriglia = Faccia.Nessuna
                                game.SelectedBlock = New Point(currentPosition.X + segno.X + n.X, currentPosition.Y + segno.Y + n.Y)
                            End If
                        End If
                    Else
                        If game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).colore <> BColor.Empty Then
                            game.SelectedBlock = New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)
                            If game.path(New Point(currentPosition.X + segno.X, currentPosition.Y + segno.Y)).colore = BColor.Red And game.finito = False Then
                                game.finito = True
                                MsgBox("Hai vinto!", MsgBoxStyle.Information)
                                Dim t As New Threading.Thread(AddressOf WinThread)
                                t.Start()
                            End If
                        End If
                    End If
                End If
            End If
            _movimentooccupato = False
        End If
        e.IsInputKey = True
    End Sub

    Private Sub WinThread()
        _giocooccupato = True
        Dim nr As New Random
        For index = 1 To 20
            For Each item In game.path
                item.Value.CustomHeight = 0
                item.Value.tipo = CType(nr.Next(1, 2), BType)
                If item.Value.tipo = BType._0_Ground Then
                    item.Value.colore = BColor.Empty
                Else
                    If item.Value.posizionegriglia <> Faccia.Su Then
                        item.Value.CustomHeight = 1
                        item.Value.posizionegriglia = Faccia.Nessuna
                    End If
                    item.Value.colore = CType(CInt(Math.Round(nr.Next(1, 5) / (index / 2))), BColor)
                    If item.Value.colore = BColor.Empty Then
                        item.Value.tipo = BType._0_Ground
                    End If
                End If
            Next
            Threading.Thread.Sleep(100)
        Next
        For Each item In game.path
            item.Value.colore = 0
            item.Value.tipo = 0
            item.Value.CustomHeight = 0
            item.Value.posizionegriglia = Faccia.Nessuna
        Next
        Try
            Me.Invoke(Sub()
                          If livelli.ContainsKey(CUInt(game.number + 1)) Then
                              LivelloSelezionato.SelectedIndex = LivelloSelezionato.Items.IndexOf("Livello " & livelli(CUInt(game.number + 1)).number & " (" & livelli(CUInt(game.number + 1)).difficulty.ToString & ")")

                          Else
                              game = CType(livelli(game.number).Clone, Level)
                          End If
                      End Sub)
        Catch
        End Try
        _giocooccupato = False
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown
        Dim posx As Double = (e.Location.X - (Me.ClientRectangle.Width / 2 - (130 * multiplier) / 2)) / multiplier
        Dim posy As Double = (e.Location.Y - (Me.ClientRectangle.Height / 2 - (150 * multiplier) / 2)) / multiplier
        If _giocooccupato = False Then
            If schermata = "Livello" Then
                If posx >= 58 And posx <= 58 + 13 Then
                    If posy >= 132 And posy <= 132 + 13 Then
                        game = CType(livelli(game.number).Clone, Level)
                    End If
                End If
            ElseIf schermata = "Schermata iniziale" Then
                If posx >= 39 And posy >= 136 And posx <= 39 + 53 - 2 And posy <= 136 + 17 - 2 Then
                    schermata = "Selezione Livello"
                    EliminaOggetto(PrendiIndexOggetto("Sfondo"))
                    EliminaOggetto(PrendiIndexOggetto("Luce di sfondo"))
                    EliminaOggetto(PrendiIndexOggetto("Pulsante seleziona livello"))
                    AggiungiOggetto(New Oggetto("Seleziona Oggetto Sfondo", New Rectangle(0, 0, 130, 150), My.Resources.levelselectbg, PosizioneZ.Dietro))

                    AggiungiOggetto(New Oggetto("Menu immagine 1", New Rectangle(19, 31, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 1", New Rectangle(39, 36, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu immagine 2", New Rectangle(19, 49, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 2", New Rectangle(39, 53, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu immagine 3", New Rectangle(19, 67, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 3", New Rectangle(39, 70, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu immagine 4", New Rectangle(19, 84, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 4", New Rectangle(39, 86, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu immagine 5", New Rectangle(19, 102, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 5", New Rectangle(39, 103, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu immagine 6", New Rectangle(19, 120, 15, 15), My.Resources.Beginner_icon, PosizioneZ.Normale))
                    AggiungiOggetto(New Oggetto("Menu titolo 6", New Rectangle(39, 120, 66, 10), My.Resources.Livello1, PosizioneZ.Normale))

                End If
            End If
        End If
    End Sub
End Class

Public Enum Faccia
    Nessuna
    Su
    Giu
    Destra
    Sinistra
    Avanti
    Indietro
End Enum

Public Enum PosizioneZ
    Davanti
    Normale
    Dietro
End Enum