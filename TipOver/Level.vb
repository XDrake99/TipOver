Public Class Level
    Implements ICloneable

    Public Property number As UInteger

    Public Property difficulty As DifficultyEnum

    Public Property path As Dictionary(Of Point, Blocco)

    Public Property finito As Boolean

    Public Property SelectedBlock As Point

    Sub New()
        number = 1
        difficulty = DifficultyEnum.Beginner
        path = New Dictionary(Of Point, Blocco)
        For x = 0 To 5
            For y = 0 To 5
                path.Add(New Point(x, y), New Blocco(Form1.BColor.Empty))
            Next
        Next
        Me.SelectedBlock = New Point(0, 0)
    End Sub

    Sub New(number As UInteger, difficulty As DifficultyEnum, path As Dictionary(Of Point, Blocco), SelectedBlock As Point, Optional finito As Boolean = False)
        For x = 0 To 5
            For y = 0 To 5
                If Not path.ContainsKey(New Point(x, y)) Then
                    path.Add(New Point(x, y), New Blocco(Form1.BColor.Empty))
                End If
            Next
        Next
        Me.path = path
        Me.difficulty = difficulty
        Me.number = number
        Me.finito = finito
        Me.SelectedBlock = SelectedBlock
    End Sub
    Sub New(level As Level)
        For x = 0 To 5
            For y = 0 To 5
                If Not level.path.ContainsKey(New Point(x, y)) Then
                    level.path.Add(New Point(x, y), New Blocco(Form1.BColor.Empty))
                End If
            Next
        Next
        Me.path = level.path
        Me.difficulty = level.difficulty
        Me.number = level.number
        Me.finito = level.finito
        Me.SelectedBlock = level.SelectedBlock
    End Sub
    Public Function Clone() As Object Implements System.ICloneable.Clone
        Dim newpath As New Dictionary(Of Point, Blocco)
        For Each item In path
            newpath.Add(item.Key, item.Value.Clone)
        Next
        Dim copia As New Level(Me.number, Me.difficulty, newpath, Me.SelectedBlock, Me.finito)
        Return copia
    End Function
End Class

Public Enum DifficultyEnum
    Beginner
    Intermediate
    Advanced
    Expert
End Enum