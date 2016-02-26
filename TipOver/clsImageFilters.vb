'i00 .Net Image Filters
'©i00 Productions All rights reserved
'Created by Kris Bennett
'----------------------------------------------------------------------------------------------------
'All property in this file is and remains the property of i00 Productions, regardless of its usage,
'unless stated otherwise in writing from i00 Productions.
'
'i00 is not and shall not be held accountable for any damages directly or indirectly caused by the
'use or miss-use of this product.  This product is only a component and thus is intended to be used 
'as part of other software, it is not a complete software package, thus i00 Productions is not 
'responsible for any legal ramifications that software using this product breaches.

Public Module ext_ImageFilters

    <System.Runtime.CompilerServices.Extension()>
    Public Function Filters(ByVal b As Bitmap) As ImageFilter
        Dim ImageFilter = New ImageFilter(b)
        Return ImageFilter
    End Function

    Public Class ImageFilter
        Dim b As Bitmap
        Public Sub New(ByVal b As Bitmap)
            Me.b = b
        End Sub

        Public Shared Sub ConvertToSupportLockBits(ByRef b As Bitmap)
            Dim bClone As New Bitmap(b.Width, b.Height, Imaging.PixelFormat.Format32bppArgb)
            Using g = Graphics.FromImage(bClone)
                b.SetResolution(96, 96)
                g.DrawImage(b, Point.Empty)
            End Using
            b.Dispose()
            b = bClone
        End Sub

        Public Shared Function SupportsLockBits(ByVal b As Bitmap) As Boolean
            Return Bitmap.GetPixelFormatSize(b.PixelFormat) = 32 AndAlso b.PixelFormat <> Imaging.PixelFormat.Indexed
        End Function

#Region "Bitmap Data"

        Private Class BitmapData
            Public Bitmap As Bitmap
            Public ByteData As Byte()
            Public BitmapData As Imaging.BitmapData

            Private Sub New()

            End Sub

            Public Shared Function LockBits(ByVal b As Bitmap) As BitmapData
                If SupportsLockBits(b) = False Then
                    Throw New Exception("Your bitmap must be 32bit non-indexed in order to apply filters")
                End If
                LockBits = New BitmapData

                LockBits.Bitmap = b

                Dim bmpRect As New Rectangle(0, 0, b.Width, b.Height)
                ReDim LockBits.ByteData(b.Width * b.Height * 4 - 1)

                LockBits.BitmapData = b.LockBits(bmpRect, Imaging.ImageLockMode.ReadWrite, b.PixelFormat)
                System.Runtime.InteropServices.Marshal.Copy(LockBits.BitmapData.Scan0, LockBits.ByteData, 0, LockBits.ByteData.Length)
            End Function

            Public Sub UnlockBits()
                System.Runtime.InteropServices.Marshal.Copy(Me.ByteData, 0, Me.BitmapData.Scan0, Me.ByteData.Length)
                Me.Bitmap.UnlockBits(Me.BitmapData)
            End Sub

            Public Function YFromOffset(ByVal Offset As Integer) As Integer
                Return CInt(Int(Offset / (Bitmap.Width * 4)))
            End Function

            Public Function StartOfLineFromOffset(ByVal Offset As Integer) As Integer
                Return Offset - (Offset Mod (Bitmap.Width * 4))
            End Function

        End Class

#End Region

        Private Sub Clear()
            Dim bData = BitmapData.LockBits(b)

            Dim OutBytes As Byte()
            ReDim OutBytes(bData.ByteData.Count - 1)
            bData.ByteData = OutBytes

            bData.UnlockBits()
        End Sub

        Private Function BlendByte(ByVal From As Byte, ByVal [To] As Byte, ByVal Amount As Byte) As Byte
            Return CByte(((CInt([To]) - [From]) * (Amount / 255)) + [From])
        End Function

#Region "Filters"

        Public Event FilterStarted(ByVal sender As Object, ByVal e As EventArgs)
        Public Event FilterFinished(ByVal sender As Object, ByVal e As EventArgs)

        Protected Overridable Sub OnFilterStarted()
            RaiseEvent FilterStarted(Me, EventArgs.Empty)
        End Sub

        Protected Overridable Sub OnFilterFinished()
            RaiseEvent FilterFinished(Me, EventArgs.Empty)
        End Sub

        Public Sub Invert()
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)
            For ii = LBound(bData.ByteData) To UBound(bData.ByteData)
                Select Case ii Mod 4
                    Case 0, 1, 2 'blue, green, red
                        bData.ByteData(ii) = CByte(255 - bData.ByteData(ii))
                    Case 3 'alpha
                End Select
            Next
            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub ApplyWPFEffect(ByVal Effect As System.Windows.Media.Effects.Effect)
            OnFilterStarted()
            Dim ImageSource As New System.Windows.Media.Imaging.BitmapImage()
            Using ms As New IO.MemoryStream()
                b.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
                ImageSource.BeginInit()
                ImageSource.StreamSource = ms
                ImageSource.EndInit()
                Dim FormImage As New System.Windows.Controls.Image()
                FormImage.Effect = Effect
                FormImage.Source = ImageSource
                Dim ImageSize = New System.Windows.Size(b.Width, b.Height)
                FormImage.Measure(ImageSize)
                Dim renderingRectangle As New System.Windows.Rect(ImageSize)
                FormImage.Arrange(renderingRectangle)


                Dim enc As New System.Windows.Media.Imaging.PngBitmapEncoder
                Dim bmpSource As New System.Windows.Media.Imaging.RenderTargetBitmap(b.Width, b.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32)
                bmpSource.Render(FormImage)
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmpSource))
                Using msOut As New IO.MemoryStream
                    enc.Save(msOut)
                    Using bOut As New Bitmap(msOut)
                        Using g As Graphics = Graphics.FromImage(b)
                            g.Clear(Color.Transparent)
                            g.DrawImageUnscaled(bOut, New Point(0, 0))
                        End Using
                    End Using
                End Using
            End Using
            OnFilterFinished()
        End Sub

        Public Sub GausianBlur(Optional ByVal Amount As Integer = 4)
            OnFilterStarted()
            Dim Blur As New System.Windows.Media.Effects.BlurEffect
            Blur.Radius = Amount
            b.Filters.ApplyWPFEffect(Blur)
            OnFilterFinished()

            ''THE OLD METHOD IS BELOW >>> THIS TOOK A WHILE SO I NOW USE WPF :)
            'Dim bData = BitmapData.LockBits(b)

            'Dim OutBytes As Byte()
            'ReDim OutBytes(bData.ByteData.Count - 1)

            'For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
            '    'find the average color of the area around the pixel...
            '    Dim cR As Integer = 0
            '    Dim cG As Integer = 0
            '    Dim cB As Integer = 0
            '    Dim cA As Integer = 0
            '    Dim PixelCounter As Integer = 0

            '    Dim YLine = bData.YFromOffset(ii)

            '    For iY = Math.Max(YLine - Int(Amount / 2), 0) To Math.Min(YLine + Int(Amount / 2), bData.Bitmap.Height - 1)
            '        Dim StartOfLine As Integer = bData.StartOfLineFromOffset(CInt(iY * (bData.Bitmap.Width * 4)))
            '        Dim EndOfLine As Integer = StartOfLine + ((bData.Bitmap.Width * 4) - 1)
            '        Dim ThisX = ii + ((iY - YLine) * (bData.Bitmap.Width * 4))
            '        For iX As Integer = CInt(Math.Max(ThisX - (Int(Amount / 2) * 4), StartOfLine)) To CInt(Math.Min((ThisX + (Int((Amount / 2) + 1) * 4)) - 1, EndOfLine))
            '            Select Case iX Mod 4
            '                Case 0 'blue
            '                    cB += bData.ByteData(iX)
            '                Case 1 'green
            '                    cG += bData.ByteData(iX)
            '                Case 2 'red
            '                    cR += bData.ByteData(iX)
            '                Case 3 'alpha
            '                    cA += bData.ByteData(iX)
            '                    PixelCounter += 1
            '            End Select
            '        Next
            '    Next

            '    OutBytes(ii) = CByte(cB / PixelCounter) 'blue
            '    OutBytes(ii + 1) = CByte(cG / PixelCounter) 'green
            '    OutBytes(ii + 2) = CByte(cR / PixelCounter) 'red
            '    OutBytes(ii + 3) = CByte(cA / PixelCounter) 'alpha
            'Next

            'bData.ByteData = OutBytes

            'bData.UnlockBits()
        End Sub

        Public Sub Alpha(Optional ByVal Alpha As Byte = 127)
            OnFilterStarted()
            Dim cm As New System.Drawing.Imaging.ColorMatrix
            cm.Matrix33 = CSng(Alpha / 255)
            Using ia As New System.Drawing.Imaging.ImageAttributes
                ia.SetColorMatrix(cm)
                Dim rc As New Rectangle(0, 0, b.Width, b.Height)
                Using bClone As Bitmap = DirectCast(b.Clone, Bitmap)
                    Using g As Graphics = Graphics.FromImage(b)
                        g.Clear(Color.Transparent)
                        g.DrawImage(bClone, rc, 0, 0, bClone.Width, bClone.Height, GraphicsUnit.Pixel, ia)
                    End Using
                End Using
            End Using
            OnFilterFinished()
        End Sub

        Public Sub GrayScale()
            OnFilterStarted()
            Dim cm As New System.Drawing.Imaging.ColorMatrix(New Single()() _
                               {New Single() {0.299, 0.299, 0.299, 0, 0},
                                New Single() {0.587, 0.587, 0.587, 0, 0},
                                New Single() {0.114, 0.114, 0.114, 0, 0},
                                New Single() {0, 0, 0, 1, 0},
                                New Single() {0, 0, 0, 0, 1}})
            Using ia As New System.Drawing.Imaging.ImageAttributes
                ia.SetColorMatrix(cm)
                Dim rc As New Rectangle(0, 0, b.Width, b.Height)
                Using bClone As Bitmap = DirectCast(b.Clone, Bitmap)
                    Using g As Graphics = Graphics.FromImage(b)
                        g.Clear(Color.Transparent)
                        g.DrawImage(bClone, rc, 0, 0, bClone.Width, bClone.Height, GraphicsUnit.Pixel, ia)
                    End Using
                End Using
            End Using
            OnFilterFinished()
        End Sub

        Public Sub AlphaMask(ByVal AlphaColor As Color, ByVal SolidColor As Color)
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)
            For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                Dim AlphaByte = bData.ByteData(ii + 3)
                bData.ByteData(ii) = BlendByte(AlphaColor.B, SolidColor.B, AlphaByte) 'blue
                bData.ByteData(ii + 1) = BlendByte(AlphaColor.G, SolidColor.G, AlphaByte)  'green
                bData.ByteData(ii + 2) = BlendByte(AlphaColor.R, SolidColor.R, AlphaByte)  'red
                bData.ByteData(ii + 3) = BlendByte(AlphaColor.A, SolidColor.A, AlphaByte)  'alpha
            Next
            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub Brightness(Optional ByVal amount As Single = 0)
            OnFilterStarted()
            If amount = 0 Then Return
            Dim bData = BitmapData.LockBits(b)
            If amount > 0 Then
                For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                    bData.ByteData(ii) = CByte(bData.ByteData(ii) + (amount * (255 - bData.ByteData(ii)))) 'blue
                    bData.ByteData(ii + 1) = CByte(bData.ByteData(ii + 1) + (amount * (255 - bData.ByteData(ii + 1)))) 'green
                    bData.ByteData(ii + 2) = CByte(bData.ByteData(ii + 2) + (amount * (255 - bData.ByteData(ii + 2)))) 'red
                Next
            Else
                For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                    bData.ByteData(ii) = CByte(bData.ByteData(ii) - (Math.Abs(amount) * bData.ByteData(ii))) 'blue
                    bData.ByteData(ii + 1) = CByte(bData.ByteData(ii + 1) - (Math.Abs(amount) * bData.ByteData(ii + 1))) 'green
                    bData.ByteData(ii + 2) = CByte(bData.ByteData(ii + 2) - (Math.Abs(amount) * bData.ByteData(ii + 2))) 'red
                Next
            End If
            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub [Static](Optional ByVal Amount As Integer = 4)
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)

            'create a random order that we will use to step through the pixels...
            'this is used to prevent image "shifting"
            Randomize()
            Dim RndPixelOrder = (From xItem In Enumerable.Range(0, CInt(UBound(bData.ByteData) / 4)) Select xItem * 4 Order By Rnd()).ToArray
            For Each item In RndPixelOrder
                'get a random pixel around the area specified from the pixel and swap it...
                Dim iY = bData.YFromOffset(item)
                Dim rY = (iY - CInt(Amount / 2)) + Int((Amount + 1) * Rnd())
                rY = Math.Max(0, rY)
                rY = Math.Min(bData.Bitmap.Height - 1, rY)

                'rX is the real offset where as Y is the nice offset
                Dim rX As Integer = CInt((Int((Rnd() * (Amount + 1 + (Amount Mod 2)))) * 4) - (CInt(Amount + (Amount Mod 2)) * 2))
                rX = item + rX

                Dim StartOfLine As Integer = bData.StartOfLineFromOffset(item)
                Dim EndOfLine As Integer = StartOfLine + ((bData.Bitmap.Width - 1) * 4)

                rX = Math.Max(StartOfLine, rX)
                rX = Math.Min(EndOfLine, rX)

                rX += CInt((bData.Bitmap.Width * 4) * (rY - iY))

                Dim Hold As Byte
                'b
                Hold = bData.ByteData(item)
                bData.ByteData(item) = bData.ByteData(rX)
                bData.ByteData(rX) = Hold
                'g
                Hold = bData.ByteData(item + 1)
                bData.ByteData(item + 1) = bData.ByteData(rX + 1)
                bData.ByteData(rX + 1) = Hold
                'r
                Hold = bData.ByteData(item + 2)
                bData.ByteData(item + 2) = bData.ByteData(rX + 2)
                bData.ByteData(rX + 2) = Hold
                'a
                Hold = bData.ByteData(item + 3)
                bData.ByteData(item + 3) = bData.ByteData(rX + 3)
                bData.ByteData(rX + 3) = Hold
            Next

            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub DropShadow(ByVal ShadowColor As Color, ByVal Depth As Point, Optional ByVal BlurAmount As Integer = 4)
            OnFilterStarted()
            'copy the origional image...
            Using bCloneShadow = DirectCast(b.Clone, Bitmap)
                Using bImage = DirectCast(b.Clone, Bitmap)

                    'clear image
                    Clear()

                    'draw the shadow
                    Using g = Graphics.FromImage(b)
                        bCloneShadow.Filters.AlphaMask(Color.Transparent, ShadowColor)
                        bCloneShadow.Filters.GausianBlur(BlurAmount)
                        g.DrawImage(bCloneShadow, Depth)
                    End Using

                    'draw the item
                    Using g = Graphics.FromImage(b)
                        g.DrawImage(bImage, Point.Empty)
                    End Using
                End Using
            End Using
            OnFilterFinished()
        End Sub

        Public Sub Emboss()
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)

            Const Level1 As Byte = 127
            Const Level2 As Byte = 255
            For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                If (ii + 4) Mod (bData.Bitmap.Width * 4) = 0 Then
                    'right line - make gray
                    bData.ByteData(ii) = Level1 'b
                    bData.ByteData(ii + 1) = Level1 'g
                    bData.ByteData(ii + 2) = Level1 'r
                    Continue For
                End If
                Dim iY = bData.YFromOffset(ii)
                If iY = bData.Bitmap.Height - 1 Then
                    'bottom line - make gray
                    bData.ByteData(ii) = Level1 'b
                    bData.ByteData(ii + 1) = Level1 'g
                    bData.ByteData(ii + 2) = Level1 'r
                    Continue For
                End If

                Dim OffsetPixelPointer = ii + ((bData.Bitmap.Width + 1) * 4)

                bData.ByteData(ii) = CByte(Math.Min(Math.Abs(CInt(bData.ByteData(ii)) - CInt(bData.ByteData(OffsetPixelPointer))) + Level1, Level2)) 'b
                bData.ByteData(ii + 1) = CByte(Math.Min(Math.Abs(CInt(bData.ByteData(ii + 1)) - CInt(bData.ByteData(OffsetPixelPointer + 1))) + Level1, Level2)) 'g
                bData.ByteData(ii + 2) = CByte(Math.Min(Math.Abs(CInt(bData.ByteData(ii + 2)) - CInt(bData.ByteData(OffsetPixelPointer + 2))) + Level1, Level2)) 'r
                'bData.ByteData(ii + 3) = 255 'a

            Next

            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        'This code for HSL is an adaptation of "Hue Saturation Lightness Filter" by Miran.Uhan
        'The origional project can be found @ http://www.codeproject.com/KB/graphics/HSL_Filter.aspx
        Public Sub HSL(Optional ByVal hue As Single = 0, Optional ByVal sat As Single = 0, Optional ByVal lum As Single = 0)
            OnFilterStarted()
            Const c1o60 As Double = 1 / 60
            Const c1o255 As Double = 1 / 255

            If hue < 0 Then
                hue = (hue + ((Int(Math.Abs(hue) / 360) + 1) * 360)) Mod 360
            Else
                hue = hue Mod 360
            End If
            sat = sat * 255
            lum = lum * 255

            Dim bData = BitmapData.LockBits(Me.b)

            Dim f1, f2 As Double
            Dim R, G, B As Double
            Dim H, S, L, H1 As Double
            Dim v1, v2, v3 As Double
            Dim dif, min, sum As Double

            For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4

                R = bData.ByteData(ii + 2)
                G = bData.ByteData(ii + 1)
                B = bData.ByteData(ii)

                min = R
                If (G < min) Then min = G
                If (B < min) Then min = B
                Dim max As Double = R : f1 = 0.0 : f2 = G - B
                If (G > max) Then
                    max = G : f1 = 120.0 : f2 = B - R
                End If
                If (B > max) Then
                    max = B : f1 = 240.0 : f2 = R - G
                End If
                dif = max - min
                sum = max + min
                L = 0.5 * sum
                If (dif = 0) Then
                    H = 0.0 : S = 0.0
                Else
                    If (L < 127.5) Then
                        S = 255.0 * dif / sum
                    Else
                        S = 255.0 * dif / (510.0 - sum)
                    End If
                    H = (f1 + 60.0 * f2 / dif)
                    If H < 0.0 Then H += 360.0
                    If H >= 360.0 Then H -= 360.0
                End If
                'transformation
                H = H + hue
                If H >= 360.0 Then H = H - 360.0
                S = S + sat
                If S < 0.0 Then S = 0.0
                If S > 255.0 Then S = 255.0
                L = L + lum
                If L < 0.0 Then L = 0.0
                If L > 255.0 Then L = 255.0
                'back to RGB
                If (S = 0) Then
                    R = L : G = L : B = L
                Else
                    If (L < 127.5) Then
                        v2 = c1o255 * L * (255 + S)
                    Else
                        v2 = L + S - c1o255 * S * L
                    End If
                    v1 = 2 * L - v2
                    v3 = v2 - v1
                    H1 = H + 120.0
                    If (H1 >= 360.0) Then H1 -= 360.0
                    If (H1 < 60.0) Then
                        R = v1 + v3 * H1 * c1o60
                    ElseIf (H1 < 180.0) Then
                        R = v2
                    ElseIf (H1 < 240.0) Then
                        R = v1 + v3 * (4 - H1 * c1o60)
                    Else
                        R = v1
                    End If
                    H1 = H
                    If (H1 < 60.0) Then
                        G = v1 + v3 * H1 * c1o60
                    ElseIf (H1 < 180.0) Then
                        G = v2
                    ElseIf (H1 < 240.0) Then
                        G = v1 + v3 * (4 - H1 * c1o60)
                    Else
                        G = v1
                    End If
                    H1 = H - 120.0
                    If (H1 < 0.0) Then H1 += 360.0
                    If (H1 < 60.0) Then
                        B = v1 + v3 * H1 * c1o60
                    ElseIf (H1 < 180.0) Then
                        B = v2
                    ElseIf (H1 < 240.0) Then
                        B = v1 + v3 * (4 - H1 * c1o60)
                    Else
                        B = v1
                    End If
                End If
                'set new colors
                bData.ByteData(ii + 2) = CByte(R)
                bData.ByteData(ii + 1) = CByte(G)
                bData.ByteData(ii) = CByte(B)
            Next

            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub OilPainting(Optional ByVal Amount As Integer = 4, Optional ByVal BleedTransparentEdges As Boolean = False)
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)

            Dim aSrc = bData.ByteData.ToArray

            Dim red As Integer() = New Integer(255) {}
            Dim green As Integer() = New Integer(255) {}
            Dim blue As Integer() = New Integer(255) {}

            Dim radius = Amount >> 1

            Dim t As Integer, j As Integer

            Dim startY = 0
            Dim stopY = bData.Bitmap.Height
            Dim startX = 0
            Dim stopX = bData.Bitmap.Width

            Dim intensities As Integer() = New Integer(255) {}

            Dim src As Integer = 0
            Dim dest As Integer = 0

            'for each line
            For y As Integer = startY To stopY - 1
                ' for each pixel
                Dim x As Integer = startX
                While x < stopX
                    ' clear arrays
                    Array.Clear(intensities, 0, 256)
                    Array.Clear(red, 0, 256)
                    Array.Clear(green, 0, 256)
                    Array.Clear(blue, 0, 256)

                    Dim alphaValues As New List(Of Byte)

                    ' for each kernel row
                    For i = -radius To radius
                        t = y + i

                        ' skip row
                        If t < startY Then
                            Continue For
                        End If
                        ' break
                        If t >= stopY Then
                            Exit For
                        End If

                        'for each x
                        For j = -radius To radius
                            t = x + j

                            'skip x
                            If t < startX Then
                                Continue For
                            End If

                            If t < stopX Then
                                Dim p = src + (i * bData.BitmapData.Stride + j * 4)

                                ' grayscale value
                                Dim intensity = CByte(0.2125 * aSrc(p + 2) + 0.7154 * aSrc(p + 1) + 0.0721 * aSrc(p))
                                intensities(intensity) += 1
                                'a
                                If BleedTransparentEdges Then alphaValues.Add(aSrc(p + 3))
                                'r
                                red(intensity) += aSrc(p + 2)
                                'g
                                green(intensity) += aSrc(p + 1)
                                'b
                                blue(intensity) += aSrc(p)
                            End If
                        Next
                    Next

                    ' get most frequent intesity
                    Dim maxIntensity = 0
                    j = 0

                    For i = 0 To 255
                        If intensities(i) > j Then
                            maxIntensity = CByte(i)
                            j = intensities(i)
                        End If
                    Next

                    ' set destination pixel
                    bData.ByteData(dest + 2) = CByte(red(maxIntensity) / intensities(maxIntensity)) 'r
                    bData.ByteData(dest + 1) = CByte(green(maxIntensity) / intensities(maxIntensity)) 'g
                    bData.ByteData(dest) = CByte(blue(maxIntensity) / intensities(maxIntensity)) 'b
                    'a
                    If BleedTransparentEdges Then
                        bData.ByteData(dest + 3) = CByte(alphaValues.Average(Function(b) b)) 'a
                    Else
                        'bData.ByteData(dest + 3) = CByte(If(bData.ByteData(dest + 2) <> 0 AndAlso bData.ByteData(dest + 1) <> 0 AndAlso bData.ByteData(dest) <> 0, 255, 0))
                    End If

                    x += 1
                    src += 4
                    dest += 4
                End While
                'src += theOffset
                'dest += theOffset
            Next

            bData.UnlockBits()

            OnFilterFinished()
        End Sub

        Public Sub TwoTone(Optional ByVal Amount As Byte = 128)
            OnFilterStarted()
            Dim bData = BitmapData.LockBits(b)
            For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                Dim Average = CByte((CInt(bData.ByteData(ii)) + CInt(bData.ByteData(ii + 1)) + CInt(bData.ByteData(ii + 2))) / 3)
                Dim Black As Boolean = False
                If Average < Amount Then
                    Black = True
                End If

                bData.ByteData(ii) = CByte(If(Black, 0, 255)) 'blue
                bData.ByteData(ii + 1) = bData.ByteData(ii) 'green
                bData.ByteData(ii + 2) = bData.ByteData(ii)  'red
            Next
            bData.UnlockBits()
            OnFilterFinished()
        End Sub

        Public Sub EdgeDetection()
            OnFilterStarted()
            b.Filters.Emboss()
            b.Filters.TwoTone(130)
            b.Filters.Invert()
            OnFilterFinished()
        End Sub

#Region "Fontify"

        Private Class FontifyFontInfo
            Dim LetterInfo As New List(Of FontifyLetterInfo)
            Public Class FontifyLetterInfo
                Public Letter As Char
                Public Path As Drawing2D.GraphicsPath
                Public Sub New(ByVal Letter As Char, ByVal Font As Font)
                    Me.Letter = Letter
                    Me.Path = New Drawing2D.GraphicsPath
                    Me.Path.AddString(CStr(Letter), Font.FontFamily, Font.Style, Font.Size, New Point(0, 0), StringFormat.GenericDefault)
                End Sub
                Public ReadOnly Property LetterSize() As Size
                    Get
                        Return New Size(CInt(Path.GetBounds.Width), CInt(Path.GetBounds.Height))  'Size(CInt(Path.GetBounds.Right + Path.GetBounds.Left), CInt(Path.GetBounds.Bottom + Path.GetBounds.Top))
                    End Get
                End Property

                Friend Blackness As Long
            End Class

            Public Sub New(ByVal font As Font)
                'go through all of the letters that this font uses to find the largest size...
                For i = 0 To 255
                    Dim thisChar = CChar(Chr(i))
                    If Char.IsSymbol(thisChar) = True OrElse Char.IsLetterOrDigit(thisChar) = True Or thisChar = " "c Then
                        Dim fli = New FontifyLetterInfo(thisChar, font)
                        LetterInfo.Add(fli)
                        Dim thisLetterSize = fli.LetterSize
                        If thisLetterSize.Width > MaxLetterSize.Width Then
                            MaxLetterSize.Width = thisLetterSize.Width
                        End If
                        If thisLetterSize.Height > MaxLetterSize.Height Then
                            MaxLetterSize.Height = thisLetterSize.Height
                        End If
                    End If
                Next
                MinLetterGap = New Point(MaxLetterSize.Width, MaxLetterSize.Height)

                'now find each letters blackness
                For Each item In LetterInfo
                    Dim ItemBounds = item.Path.GetBounds
                    If ItemBounds.Width > 0 Then
                        If MinLetterGap.X > ItemBounds.Left Then
                            MinLetterGap.X = CInt(ItemBounds.Left)
                        End If
                    End If
                    Using b As New Bitmap(MaxLetterSize.Width, MaxLetterSize.Height)
                        Using g = Graphics.FromImage(b)
                            g.DrawString(CStr(item.Letter), font, Brushes.Black, 0, 0)
                        End Using
                        Dim bData = ImageFilter.BitmapData.LockBits(b)
                        For ii = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                            item.Blackness += bData.ByteData(ii + 3)
                        Next
                        bData.UnlockBits()
                    End Using
                    If item.Blackness > MaxBlackness Then
                        MaxBlackness = item.Blackness
                    End If
                Next
            End Sub

            Public MaxBlackness As Long

            Public MinLetterGap As Point

            Public MaxLetterSize As Size

            Public Function GetClosestLetterToColor(ByVal Color As Color, Optional ByVal ColorCount As Byte = 255) As FontifyLetterInfo
                If ColorCount < 1 Then ColorCount = 1
                Dim ColorAve = CByte((CInt(Color.A) + CInt(Color.R) + CInt(Color.G) + CInt(Color.B)) / 4)
                'reduce to 16 colors...
                Dim colorDivider = 256 / (ColorCount + 1)
                Dim NewColorAve = CLng((ColorAve / colorDivider)) * colorDivider
                If NewColorAve < 0 Then NewColorAve = 0
                If NewColorAve > 255 Then NewColorAve = 255
                ColorAve = CByte(NewColorAve)

                Dim SymbolsToUse = (From xItem In LetterInfo Where Char.IsSymbol(xItem.Letter) = True OrElse Char.IsLetterOrDigit(xItem.Letter) = True Or xItem.Letter = " "c Order By xItem.Blackness).ToArray
                Return SymbolsToUse(CInt((ColorAve / 255) * (SymbolsToUse.Count - 1)))
            End Function
        End Class

        Public Sub Fontify(ByVal font As Font, Optional ByVal ColorCount As Byte = 255, Optional ByVal ColorAlso As Boolean = True)
            OnFilterStarted()

            Dim FontData As New FontifyFontInfo(font)

            'create a smaller image that is the size of the letters across and down...
            Using bSmall As New Bitmap(CInt(b.Width / FontData.MaxLetterSize.Width), CInt(b.Height / FontData.MaxLetterSize.Height))
                Using g = Graphics.FromImage(bSmall)
                    'g.InterpolationMode = Drawing2D.InterpolationMode.High
                    g.DrawImage(b, New Rectangle(0, 0, bSmall.Width, bSmall.Height))
                End Using
                'now find the closest char for each 
                Dim bData = ImageFilter.BitmapData.LockBits(bSmall)
                Using g = Graphics.FromImage(b)
                    g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                    g.Clear(Color.Transparent)
                    For i = LBound(bData.ByteData) To UBound(bData.ByteData) Step 4
                        Dim p As New PointF(0, bData.YFromOffset(i))
                        p.X = (i \ 4) - (p.Y * bSmall.Width)
                        'expand the p from the small image to the large image...
                        p.X *= CSng((b.Width / bSmall.Width))
                        p.Y *= CSng((b.Height / bSmall.Height))
                        Dim pixcolor = Color.FromArgb(bData.ByteData(i + 3), bData.ByteData(i + 2), bData.ByteData(i + 1), bData.ByteData(i))
                        Using sb As New SolidBrush(pixcolor)
                            g.ResetTransform()
                            Dim LetterData = FontData.GetClosestLetterToColor(pixcolor, ColorCount)
                            g.TranslateTransform(p.X - LetterData.Path.GetBounds.Left, p.Y - LetterData.Path.GetBounds.Top)
                            'g.ScaleTransform(2, 2)
                            g.FillPath(If(ColorAlso, sb, Brushes.Black), LetterData.Path)
                        End Using
                    Next
                End Using
                bData.UnlockBits()
            End Using

            OnFilterFinished()
        End Sub

#End Region

#End Region

    End Class
End Module
