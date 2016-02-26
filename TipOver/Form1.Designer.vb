<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla nell'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.tickloop = New System.Windows.Forms.Timer(Me.components)
        Me.LivelloSelezionato = New System.Windows.Forms.ComboBox()
        Me.keygrabber = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'tickloop
        '
        Me.tickloop.Enabled = True
        Me.tickloop.Interval = 40
        '
        'LivelloSelezionato
        '
        Me.LivelloSelezionato.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LivelloSelezionato.BackColor = System.Drawing.Color.Black
        Me.LivelloSelezionato.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.LivelloSelezionato.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.LivelloSelezionato.ForeColor = System.Drawing.Color.White
        Me.LivelloSelezionato.FormattingEnabled = True
        Me.LivelloSelezionato.Items.AddRange(New Object() {"Livello 1"})
        Me.LivelloSelezionato.Location = New System.Drawing.Point(111, 228)
        Me.LivelloSelezionato.Name = "LivelloSelezionato"
        Me.LivelloSelezionato.Size = New System.Drawing.Size(121, 21)
        Me.LivelloSelezionato.TabIndex = 1
        Me.LivelloSelezionato.TabStop = False
        '
        'keygrabber
        '
        Me.keygrabber.Cursor = System.Windows.Forms.Cursors.Arrow
        Me.keygrabber.Location = New System.Drawing.Point(16, 20)
        Me.keygrabber.Name = "keygrabber"
        Me.keygrabber.Size = New System.Drawing.Size(0, 0)
        Me.keygrabber.TabIndex = 0
        Me.keygrabber.Text = "Button1"
        Me.keygrabber.UseVisualStyleBackColor = False
        '
        'Form1
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(244, 261)
        Me.Controls.Add(Me.keygrabber)
        Me.Controls.Add(Me.LivelloSelezionato)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.Text = "TipOver"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tickloop As Timer
    Friend WithEvents LivelloSelezionato As ComboBox
    Friend WithEvents keygrabber As Button
End Class
