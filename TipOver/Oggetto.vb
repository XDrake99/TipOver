Public Class Oggetto
    Private _Name As String
    Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property
    Private _Location As Point
    Property Location As Point
        Get
            Return _Location
        End Get
        Set(value As Point)
            _Location = value
        End Set
    End Property

    Private _Size As Size
    Property Size As Size
        Get
            Return _Size
        End Get
        Set(value As Size)
            _Size = value
        End Set
    End Property

    Private _Image As Image
    Property Image As Image
        Get
            Return _Image
        End Get
        Set(value As Image)
            _Image = value
        End Set
    End Property
    
    Private _posizione As PosizioneZ
    Property posizione As PosizioneZ
        Get
            Return _posizione
        End Get
        Set(value As PosizioneZ)
            _posizione = value
        End Set
    End Property

    Private _visible As Boolean
    Property visible As Boolean
        Get
            Return _visible
        End Get
        Set(value As Boolean)
            _visible = value
        End Set
    End Property

    Sub New(Name As String, Location As Point, Size As Size, Image As Image, posizione As PosizioneZ, Optional visible As Boolean = True)
        _Name = Name
        _Image = Image
        _Size = Size
        _Location = Location
        _posizione = posizione
        _visible = visible
    End Sub

    Sub New(Name As String, Rect As Rectangle, Image As Image, posizione As PosizioneZ, Optional visible As Boolean = True)
        _Name = Name
        _Image = Image
        _Size = Rect.Size
        _Location = Rect.Location
        _posizione = posizione
        _visible = visible
    End Sub
End Class