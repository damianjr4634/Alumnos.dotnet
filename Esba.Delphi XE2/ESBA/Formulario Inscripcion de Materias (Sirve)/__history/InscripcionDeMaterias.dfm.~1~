object InscripcionMaterias: TInscripcionMaterias
  Left = 168
  Top = 139
  BorderStyle = bsDialog
  Caption = 'Inscripci'#243'n de Materias'
  ClientHeight = 501
  ClientWidth = 585
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  KeyPreview = True
  OldCreateOrder = False
  Position = poScreenCenter
  OnActivate = FormActivate
  OnCreate = FormCreate
  OnKeyPress = FormKeyPress
  PixelsPerInch = 96
  TextHeight = 13
  object GroupBox1: TGroupBox
    Left = 0
    Top = 0
    Width = 585
    Height = 97
    Align = alTop
    Caption = 'Datos Acad'#233'micos del Alumno'
    TabOrder = 0
    object Label1: TLabel
      Left = 6
      Top = 32
      Width = 88
      Height = 13
      Caption = 'C'#243'digo del Alumno'
    end
    object Label2: TLabel
      Left = 306
      Top = 32
      Width = 85
      Height = 13
      Caption = 'Carrera que Cursa'
    end
    object Label3: TLabel
      Left = 6
      Top = 64
      Width = 85
      Height = 13
      Caption = 'Apellido y Nombre'
    end
    object Label4: TLabel
      Left = 314
      Top = 64
      Width = 54
      Height = 13
      Caption = 'Libro Matriz'
    end
    object MateriasCodigoAlumno: TEdit
      Left = 102
      Top = 24
      Width = 121
      Height = 21
      ReadOnly = True
      TabOrder = 0
    end
    object MateriasCarreraCursa: TEdit
      Left = 406
      Top = 24
      Width = 121
      Height = 21
      ReadOnly = True
      TabOrder = 1
    end
    object MateriasApellidoyNombre: TEdit
      Left = 102
      Top = 56
      Width = 203
      Height = 21
      ReadOnly = True
      TabOrder = 2
    end
    object MateriasLibroMatriz: TEdit
      Left = 406
      Top = 57
      Width = 121
      Height = 21
      ReadOnly = True
      TabOrder = 3
    end
  end
  object GroupBox2: TGroupBox
    Left = 0
    Top = 104
    Width = 585
    Height = 177
    Caption = 'Estado de Materias'
    TabOrder = 1
    object MateriasCursando: TDBGrid
      Left = 2
      Top = 15
      Width = 581
      Height = 160
      Align = alClient
      DataSource = DataSource1
      Options = [dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgRowSelect, dgCancelOnExit]
      TabOrder = 0
      TitleFont.Charset = DEFAULT_CHARSET
      TitleFont.Color = clWindowText
      TitleFont.Height = -11
      TitleFont.Name = 'MS Sans Serif'
      TitleFont.Style = []
      OnDblClick = modificacursadaClick
      OnKeyPress = MateriasCursandoKeyPress
      Columns = <
        item
          Expanded = False
          FieldName = 'MATRIZ'
          ReadOnly = True
          Title.Alignment = taCenter
          Title.Caption = 'Matriz'
          Width = 60
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CUTUCO'
          ReadOnly = True
          Title.Alignment = taCenter
          Title.Caption = 'Comisi'#243'n'
          Width = 71
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'COD_MAT'
          ReadOnly = True
          Title.Alignment = taCenter
          Title.Caption = 'Cod.Mat.'
          Width = 45
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'DESCRIPCI'
          ReadOnly = True
          Title.Caption = 'Descripcion'
          Width = 200
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CONDICION'
          ReadOnly = True
          Title.Alignment = taCenter
          Title.Caption = 'Condici'#243'n'
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CUA_ANIO'
          ReadOnly = True
          Title.Alignment = taCenter
          Title.Caption = 'Cuatrimestre/A'#241'o'
          Width = 85
          Visible = True
        end>
    end
  end
  object GroupBox3: TGroupBox
    Left = 0
    Top = 332
    Width = 585
    Height = 168
    Caption = 'Alta de Materias'
    TabOrder = 2
    object Label5: TLabel
      Left = 8
      Top = 24
      Width = 86
      Height = 13
      Caption = 'C'#243'digo de Materia'
    end
    object Label7: TLabel
      Left = 8
      Top = 53
      Width = 28
      Height = 13
      Caption = 'Turno'
    end
    object Label8: TLabel
      Left = 8
      Top = 82
      Width = 42
      Height = 13
      Caption = 'Comisi'#243'n'
    end
    object Label9: TLabel
      Left = 8
      Top = 111
      Width = 102
      Height = 13
      Caption = 'Condici'#243'n del Alumno'
    end
    object Label10: TLabel
      Left = 8
      Top = 139
      Width = 88
      Height = 13
      Caption = 'Cuatrimestre / A'#241'o'
    end
    object Label11: TLabel
      Left = 184
      Top = 53
      Width = 203
      Height = 13
      Caption = '1-Ma'#241'ana; 2-Tarde; 3-Vespertino; 4-Noche'
    end
    object NombreMat: TLabel
      Left = 200
      Top = 22
      Width = 3
      Height = 13
    end
    object Comision: TEdit
      Left = 128
      Top = 73
      Width = 49
      Height = 21
      MaxLength = 1
      TabOrder = 2
    end
    object ComboCondicionAlumno: TComboBox
      Left = 128
      Top = 102
      Width = 121
      Height = 19
      Style = csOwnerDrawFixed
      ItemHeight = 13
      Sorted = True
      TabOrder = 3
      Items.Strings = (
        'CURSANDO'
        'P/EQUIVALEN'
        'RECURSANDO')
    end
    object GrabaMateria: TBitBtn
      Left = 457
      Top = 80
      Width = 123
      Height = 33
      Caption = 'Graba Materia'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
        555555555555555555555555555555555555555555FF55555555555559055555
        55555555577FF5555555555599905555555555557777F5555555555599905555
        555555557777FF5555555559999905555555555777777F555555559999990555
        5555557777777FF5555557990599905555555777757777F55555790555599055
        55557775555777FF5555555555599905555555555557777F5555555555559905
        555555555555777FF5555555555559905555555555555777FF55555555555579
        05555555555555777FF5555555555557905555555555555777FF555555555555
        5990555555555555577755555555555555555555555555555555}
      NumGlyphs = 2
      TabOrder = 5
      OnClick = GrabaMateriaClick
    end
    object CancelaGrabacion: TBitBtn
      Left = 456
      Top = 120
      Width = 123
      Height = 34
      Caption = 'Cancela Grabaci'#243'n'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000130B0000130B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333333333333333333333333FFF33FF333FFF339993370733
        999333777FF37FF377733339993000399933333777F777F77733333399970799
        93333333777F7377733333333999399933333333377737773333333333990993
        3333333333737F73333333333331013333333333333777FF3333333333910193
        333333333337773FF3333333399000993333333337377737FF33333399900099
        93333333773777377FF333399930003999333337773777F777FF339993370733
        9993337773337333777333333333333333333333333333333333333333333333
        3333333333333333333333333333333333333333333333333333}
      NumGlyphs = 2
      TabOrder = 6
      OnClick = CancelaGrabacionClick
    end
    object turno: TEdit
      Left = 128
      Top = 45
      Width = 49
      Height = 21
      MaxLength = 1
      TabOrder = 1
    end
    object CuatrimestreAnio: TComboEdit
      Left = 128
      Top = 129
      Width = 41
      Height = 21
      ClickKey = 0
      EditMask = '9/99;1;_'
      ButtonWidth = 0
      MaxLength = 4
      NumGlyphs = 1
      TabOrder = 4
      Text = ' /  '
    end
    object ComboMaterias: TComboEdit
      Left = 128
      Top = 19
      Width = 65
      Height = 21
      ClickKey = 112
      GlyphKind = gkEllipsis
      MaxLength = 2
      NumGlyphs = 1
      TabOrder = 0
      OnButtonClick = ComboMateriasButtonClick
      OnExit = ComboMateriasExit
    end
  end
  object modificacursada: TBitBtn
    Left = 121
    Top = 288
    Width = 113
    Height = 33
    Caption = 'Modifica Cursada'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333000000
      000033333377777777773333330FFFFFFFF03FF3FF7FF33F3FF700300000FF0F
      00F077F777773F737737E00BFBFB0FFFFFF07773333F7F3333F7E0BFBF000FFF
      F0F077F3337773F3F737E0FBFBFBF0F00FF077F3333FF7F77F37E0BFBF00000B
      0FF077F3337777737337E0FBFBFBFBF0FFF077F33FFFFFF73337E0BF0000000F
      FFF077FF777777733FF7000BFB00B0FF00F07773FF77373377373330000B0FFF
      FFF03337777373333FF7333330B0FFFF00003333373733FF777733330B0FF00F
      0FF03333737F37737F373330B00FFFFF0F033337F77F33337F733309030FFFFF
      00333377737FFFFF773333303300000003333337337777777333}
    NumGlyphs = 2
    TabOrder = 4
    OnClick = modificacursadaClick
  end
  object eliminacursada: TBitBtn
    Left = 238
    Top = 288
    Width = 113
    Height = 33
    Caption = 'Elimina Cursada'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
      55555FFFFFFF5F55FFF5777777757559995777777775755777F7555555555550
      305555555555FF57F7F555555550055BB0555555555775F777F55555550FB000
      005555555575577777F5555550FB0BF0F05555555755755757F555550FBFBF0F
      B05555557F55557557F555550BFBF0FB005555557F55575577F555500FBFBFB0
      B05555577F555557F7F5550E0BFBFB00B055557575F55577F7F550EEE0BFB0B0
      B05557FF575F5757F7F5000EEE0BFBF0B055777FF575FFF7F7F50000EEE00000
      B0557777FF577777F7F500000E055550805577777F7555575755500000555555
      05555777775555557F5555000555555505555577755555557555}
    NumGlyphs = 2
    TabOrder = 5
    OnClick = eliminacursadaClick
  end
  object salir: TBitBtn
    Left = 512
    Top = 288
    Width = 65
    Height = 33
    Caption = 'Salir'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00330000000000
      03333377777777777F333301BBBBBBBB033333773F3333337F3333011BBBBBBB
      0333337F73F333337F33330111BBBBBB0333337F373F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337FF3337F33330111B0BBBB
      0333337F337733337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F3F7F33337F333301E10BBBBB0333337F7F7F33337F333301EE0BBBBB
      0333337F777FFFFF7F3333000000000003333377777777777333}
    NumGlyphs = 2
    TabOrder = 6
    OnClick = salirClick
  end
  object CursadaNueva: TBitBtn
    Left = 3
    Top = 288
    Width = 113
    Height = 33
    Caption = '&Nueva materia'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000130B0000130B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF0033333333B333
      333B33FF33337F3333F73BB3777BB7777BB3377FFFF77FFFF77333B000000000
      0B3333777777777777333330FFFFFFFF07333337F33333337F333330FFFFFFFF
      07333337F33333337F333330FFFFFFFF07333337F33333337F333330FFFFFFFF
      07333FF7F33333337FFFBBB0FFFFFFFF0BB37777F3333333777F3BB0FFFFFFFF
      0BBB3777F3333FFF77773330FFFF000003333337F333777773333330FFFF0FF0
      33333337F3337F37F3333330FFFF0F0B33333337F3337F77FF333330FFFF003B
      B3333337FFFF77377FF333B000000333BB33337777777F3377FF3BB3333BB333
      3BB33773333773333773B333333B3333333B7333333733333337}
    NumGlyphs = 2
    TabOrder = 3
    OnClick = CursadaNuevaClick
  end
  object Conjunto: TBitBtn
    Left = 355
    Top = 288
    Width = 113
    Height = 33
    Caption = '&Conjunto Mat.'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
      55555555FFFFFFFF5555555000000005555555577777777FF555550999999900
      55555575555555775F55509999999901055557F55555557F75F5001111111101
      105577FFFFFFFF7FF75F00000000000011057777777777775F755070FFFFFF0F
      01105777F555557F75F75500FFFFFF0FF0105577F555FF7F57575550FF700008
      8F0055575FF7777555775555000888888F005555777FFFFFFF77555550000000
      0F055555577777777F7F555550FFFFFF0F05555557F5FFF57F7F555550F000FF
      0005555557F777557775555550FFFFFF0555555557F555FF7F55555550FF7000
      05555555575FF777755555555500055555555555557775555555}
    NumGlyphs = 2
    TabOrder = 7
    OnClick = ConjuntoClick
  end
  object DataSource1: TDataSource
    DataSet = CustomerData.IbInscmaterias
    Left = 488
    Top = 104
  end
end
