Public Class Blocco
    Implements ICloneable

    Public Property colore As Form1.BColor

    Public Property tipo As Form1.BType

    Public Property posizionegriglia As Faccia

    Public Property CustomHeight As UInt16

    Public Sub New()
        colore = Form1.BColor.Yellow
        tipo = Form1.BType._2_Hole
        CustomHeight = 0
        posizionegriglia = Faccia.Nessuna
    End Sub

    Public Sub New(colore As Form1.BColor, Optional tipo As Form1.BType = Form1.BType._2_Hole, Optional posizionegriglia As Faccia = Faccia.Su, Optional CustomHeight As UInt16 = 0)
        Me.colore = colore
        If colore = Form1.BColor.Empty Then
            tipo = Form1.BType._0_Ground
        End If
        Me.tipo = tipo
        Me.CustomHeight = CustomHeight
        Me.posizionegriglia = posizionegriglia
    End Sub

    Public Function Clone() As Object Implements System.ICloneable.Clone
        Return New Blocco(colore, tipo, posizionegriglia, CustomHeight)
    End Function
End Class
