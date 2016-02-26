Public Class Animazione
    Private _AnimationType As TipoAnimazione
    Property AnimationType As TipoAnimazione
        Get
            Return _AnimationType
        End Get
        Set(value As TipoAnimazione)
            _AnimationType = value
        End Set
    End Property

    Private _Proprietario As String
    Property Proprietario As String
        Get
            Return _Proprietario
        End Get
        Set(value As String)
            _Proprietario = value
        End Set
    End Property

    Private _Prop1 As Object
    Property Prop1 As Object
        Get
            Return _Prop1
        End Get
        Set(value As Object)
            _Prop1 = value
        End Set
    End Property

    Private _Prop2 As Object
    Property Prop2 As Object
        Get
            Return _Prop2
        End Get
        Set(value As Object)
            _Prop2 = value
        End Set
    End Property

    Private _TempoTicks As UInteger
    Property TempoTicks As UInteger
        Get
            Return _TempoTicks
        End Get
        Set(value As UInteger)
            _TempoTicks = value
        End Set
    End Property

    Property TempoAttualeTicks As UInteger = 0

    Private _Ripetizioni As Integer
    Property Ripetizioni As Integer
        Get
            Return _Ripetizioni
        End Get
        Set(value As Integer)
            _Ripetizioni = value
        End Set
    End Property

    Sub New(Proprietario As String, AnimationType As TipoAnimazione, Prop1 As Object, Prop2 As Object, TempoTicks As UInteger, Ripetizioni As Integer)
        _Proprietario = Proprietario
        _AnimationType = AnimationType
        _Prop1 = Prop1
        _Prop2 = Prop2
        _TempoTicks = TempoTicks
        _Ripetizioni = Ripetizioni
    End Sub
End Class

Public Enum TipoAnimazione
    Posizione
    Dimensione
    Immagine
End Enum